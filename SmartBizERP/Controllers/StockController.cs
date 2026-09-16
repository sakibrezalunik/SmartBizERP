using System;
using System.Data.Entity;
using System.Linq;
using System.Web.Mvc;
using SmartBizERP.Models;

namespace SmartBizERP.Controllers
{
    public class StockController : Controller
    {
        private SmartBizERPModel db = new SmartBizERPModel();

        // GET: Stock  -- current on-hand quantities across all warehouses
        public ActionResult Index(int? warehouseId, int? productId)
        {
            var stock = db.WarehouseStocks
                .Include(s => s.Warehouse)
                .Include(s => s.Product)
                .AsQueryable();

            if (warehouseId.HasValue)
                stock = stock.Where(s => s.WarehouseId == warehouseId.Value);

            if (productId.HasValue)
                stock = stock.Where(s => s.ProductId == productId.Value);

            ViewBag.WarehouseList = new SelectList(db.Warehouses, "WarehouseId", "WarehouseName", warehouseId);
            ViewBag.ProductList = new SelectList(db.Products, "ProductId", "ProductName", productId);

            return View(stock.OrderBy(s => s.Warehouse.WarehouseName).ThenBy(s => s.Product.ProductName).ToList());
        }

        // GET: Stock/Ledger -- full transaction history, most recent first
        public ActionResult Ledger(int? productId, int? warehouseId)
        {
            var transactions = db.StockTransactions
                .Include(t => t.Product)
                .Include(t => t.Warehouse)
                .AsQueryable();

            if (productId.HasValue)
                transactions = transactions.Where(t => t.ProductId == productId.Value);

            if (warehouseId.HasValue)
                transactions = transactions.Where(t => t.WarehouseId == warehouseId.Value);

            ViewBag.ProductList = new SelectList(db.Products, "ProductId", "ProductName", productId);
            ViewBag.WarehouseList = new SelectList(db.Warehouses, "WarehouseId", "WarehouseName", warehouseId);

            return View(transactions.OrderByDescending(t => t.TransactionDate).Take(200).ToList());
        }

        // GET: Stock/Adjustment
        public ActionResult Adjustment()
        {
            ViewBag.WarehouseList = new SelectList(db.Warehouses, "WarehouseId", "WarehouseName");
            ViewBag.ProductList = new SelectList(db.Products, "ProductId", "ProductName");
            return View();
        }

        // POST: Stock/Adjustment
        // A manual correction -- e.g. physical stock count found a
        // discrepancy. quantityChange can be positive (found extra stock)
        // or negative (stock is missing/damaged).
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Adjustment(int warehouseId, int productId, decimal quantityChange, string reason)
        {
            if (quantityChange == 0)
            {
                ModelState.AddModelError("", "Quantity change cannot be zero.");
            }

            if (ModelState.IsValid)
            {
                // Everything below must succeed together or not at all --
                // if the ledger entry is written but the running total
                // update fails (or vice versa), the two tables go out of
                // sync and WarehouseStock stops being trustworthy.
                using (var dbTransaction = db.Database.BeginTransaction())
                {
                    try
                    {
                        db.StockTransactions.Add(new StockTransaction
                        {
                            ProductId = productId,
                            WarehouseId = warehouseId,
                            TransactionType = "ADJUSTMENT",
                            Quantity = quantityChange,
                            ReferenceType = "Manual",
                            TransactionDate = DateTime.Now
                        });

                        var existingStock = db.WarehouseStocks
                            .FirstOrDefault(s => s.WarehouseId == warehouseId && s.ProductId == productId);

                        if (existingStock == null)
                        {
                            db.WarehouseStocks.Add(new WarehouseStock
                            {
                                WarehouseId = warehouseId,
                                ProductId = productId,
                                Quantity = quantityChange
                            });
                        }
                        else
                        {
                            existingStock.Quantity += quantityChange;
                        }

                        db.SaveChanges();
                        dbTransaction.Commit();

                        TempData["Success"] = "Stock adjustment recorded successfully.";
                        return RedirectToAction("Index");
                    }
                    catch
                    {
                        dbTransaction.Rollback();
                        ModelState.AddModelError("", "Something went wrong recording the adjustment. Please try again.");
                    }
                }
            }

            ViewBag.WarehouseList = new SelectList(db.Warehouses, "WarehouseId", "WarehouseName", warehouseId);
            ViewBag.ProductList = new SelectList(db.Products, "ProductId", "ProductName", productId);
            return View();
        }

        // GET: Stock/Transfer
        public ActionResult Transfer()
        {
            ViewBag.WarehouseList = new SelectList(db.Warehouses, "WarehouseId", "WarehouseName");
            ViewBag.ProductList = new SelectList(db.Products, "ProductId", "ProductName");
            return View();
        }

        // POST: Stock/Transfer
        // Moves stock from one warehouse to another. This writes TWO
        // ledger rows (an OUT at the source, an IN at the destination)
        // and updates BOTH warehouses' running totals -- all inside one
        // database transaction, so a transfer can never leave stock
        // "in transit and nowhere" if something fails halfway through.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Transfer(int fromWarehouseId, int toWarehouseId, int productId, decimal quantity)
        {
            if (fromWarehouseId == toWarehouseId)
            {
                ModelState.AddModelError("", "Source and destination warehouse must be different.");
            }

            if (quantity <= 0)
            {
                ModelState.AddModelError("", "Transfer quantity must be greater than zero.");
            }

            var sourceStock = db.WarehouseStocks
                .FirstOrDefault(s => s.WarehouseId == fromWarehouseId && s.ProductId == productId);

            if (sourceStock == null || sourceStock.Quantity < quantity)
            {
                ModelState.AddModelError("", "Not enough stock in the source warehouse to complete this transfer.");
            }

            if (ModelState.IsValid)
            {
                using (var dbTransaction = db.Database.BeginTransaction())
                {
                    try
                    {
                        var now = DateTime.Now;

                        // Ledger: stock leaving the source warehouse
                        db.StockTransactions.Add(new StockTransaction
                        {
                            ProductId = productId,
                            WarehouseId = fromWarehouseId,
                            TransactionType = "TRANSFER",
                            Quantity = -quantity,
                            ReferenceType = "Manual",
                            TransactionDate = now
                        });

                        // Ledger: stock arriving at the destination warehouse
                        db.StockTransactions.Add(new StockTransaction
                        {
                            ProductId = productId,
                            WarehouseId = toWarehouseId,
                            TransactionType = "TRANSFER",
                            Quantity = quantity,
                            ReferenceType = "Manual",
                            TransactionDate = now
                        });

                        // Update running totals for both warehouses
                        sourceStock.Quantity -= quantity;

                        var destStock = db.WarehouseStocks
                            .FirstOrDefault(s => s.WarehouseId == toWarehouseId && s.ProductId == productId);

                        if (destStock == null)
                        {
                            db.WarehouseStocks.Add(new WarehouseStock
                            {
                                WarehouseId = toWarehouseId,
                                ProductId = productId,
                                Quantity = quantity
                            });
                        }
                        else
                        {
                            destStock.Quantity += quantity;
                        }

                        db.SaveChanges();
                        dbTransaction.Commit();

                        TempData["Success"] = "Stock transferred successfully.";
                        return RedirectToAction("Index");
                    }
                    catch
                    {
                        dbTransaction.Rollback();
                        ModelState.AddModelError("", "Something went wrong recording the transfer. Please try again.");
                    }
                }
            }

            ViewBag.WarehouseList = new SelectList(db.Warehouses, "WarehouseId", "WarehouseName");
            ViewBag.ProductList = new SelectList(db.Products, "ProductId", "ProductName", productId);
            return View();
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing) db.Dispose();
            base.Dispose(disposing);
        }
    }
}
