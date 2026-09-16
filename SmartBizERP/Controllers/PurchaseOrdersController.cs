using System;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Web.Mvc;
using SmartBizERP.Models;

namespace SmartBizERP.Controllers
{
    public class PurchaseOrdersController : Controller
    {
        private SmartBizERPModel db = new SmartBizERPModel();

        public ActionResult Index()
        {
            var orders = db.PurchaseOrders
                .Include(o => o.Supplier)
                .Include(o => o.Warehouse)
                .OrderByDescending(o => o.OrderDate)
                .ToList();
            return View(orders);
        }

        public ActionResult Create()
        {
            ViewBag.SupplierId = new SelectList(db.Suppliers, "SupplierId", "SupplierName");
            ViewBag.WarehouseId = new SelectList(db.Warehouses, "WarehouseId", "WarehouseName");
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create(int supplierId, int warehouseId)
        {
            var order = new PurchaseOrder
            {
                SupplierId = supplierId,
                WarehouseId = warehouseId,
                OrderDate = DateTime.Now,
                Status = "Draft",
                TotalAmount = 0
            };

            db.PurchaseOrders.Add(order);
            db.SaveChanges();

            // Order number depends on the identity value generated above,
            // so it can only be assigned after the first save.
            order.PurchaseOrderNo = "PO-" + order.PurchaseOrderId.ToString("D4");
            db.SaveChanges();

            return RedirectToAction("Details", new { id = order.PurchaseOrderId });
        }

        public ActionResult Details(int? id)
        {
            if (id == null) return new HttpStatusCodeResult(HttpStatusCode.BadRequest);

            var order = db.PurchaseOrders
                .Include(o => o.Supplier)
                .Include(o => o.Warehouse)
                .Include(o => o.PurchaseOrderDetails.Select(d => d.Product))
                .FirstOrDefault(o => o.PurchaseOrderId == id);

            if (order == null) return HttpNotFound();

            ViewBag.ProductId = new SelectList(db.Products, "ProductId", "ProductName");
            return View(order);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult AddLine(int purchaseOrderId, int productId, decimal quantity, decimal unitPrice)
        {
            var order = db.PurchaseOrders.Find(purchaseOrderId);
            if (order == null || order.Status != "Draft")
            {
                TempData["Error"] = "This order can no longer be edited.";
                return RedirectToAction("Details", new { id = purchaseOrderId });
            }

            db.PurchaseOrderDetails.Add(new PurchaseOrderDetail
            {
                PurchaseOrderId = purchaseOrderId,
                ProductId = productId,
                Quantity = quantity,
                UnitPrice = unitPrice
            });
            db.SaveChanges();

            // Recalculate the header total from its line items -- this is
            // the one field the database can't compute for us, since it
            // sums across rows in a child table.
            order.TotalAmount = db.PurchaseOrderDetails
                .Where(d => d.PurchaseOrderId == purchaseOrderId)
                .Sum(d => d.TotalAmount);
            db.SaveChanges();

            return RedirectToAction("Details", new { id = purchaseOrderId });
        }

        // Marks the order Received and posts every line item into the
        // stock ledger as a PURCHASE transaction, updating on-hand
        // quantities -- all inside one database transaction, same
        // pattern as Stock/Transfer in Phase 3.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Receive(int id)
        {
            var order = db.PurchaseOrders
                .Include(o => o.PurchaseOrderDetails)
                .FirstOrDefault(o => o.PurchaseOrderId == id);

            if (order == null) return HttpNotFound();

            if (order.Status != "Draft")
            {
                TempData["Error"] = "This order has already been received.";
                return RedirectToAction("Details", new { id });
            }

            if (!order.PurchaseOrderDetails.Any())
            {
                TempData["Error"] = "Add at least one line item before receiving.";
                return RedirectToAction("Details", new { id });
            }

            using (var dbTransaction = db.Database.BeginTransaction())
            {
                try
                {
                    foreach (var line in order.PurchaseOrderDetails)
                    {
                        db.StockTransactions.Add(new StockTransaction
                        {
                            ProductId = line.ProductId,
                            WarehouseId = order.WarehouseId,
                            TransactionType = "PURCHASE",
                            Quantity = line.Quantity,
                            ReferenceType = "PurchaseOrder",
                            ReferenceId = order.PurchaseOrderId,
                            TransactionDate = DateTime.Now
                        });

                        var stock = db.WarehouseStocks
                            .FirstOrDefault(s => s.WarehouseId == order.WarehouseId && s.ProductId == line.ProductId);

                        if (stock == null)
                        {
                            db.WarehouseStocks.Add(new WarehouseStock
                            {
                                WarehouseId = order.WarehouseId,
                                ProductId = line.ProductId,
                                Quantity = line.Quantity
                            });
                        }
                        else
                        {
                            stock.Quantity += line.Quantity;
                        }
                    }

                    order.Status = "Received";
                    db.SaveChanges();
                    dbTransaction.Commit();

                    TempData["Success"] = "Goods received and stock updated.";
                }
                catch
                {
                    dbTransaction.Rollback();
                    TempData["Error"] = "Something went wrong receiving this order.";
                }
            }

            return RedirectToAction("Details", new { id });
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing) db.Dispose();
            base.Dispose(disposing);
        }
    }
}
