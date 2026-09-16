using System;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Web.Mvc;
using SmartBizERP.Models;

namespace SmartBizERP.Controllers
{
    public class SalesOrdersController : Controller
    {
        private SmartBizERPModel db = new SmartBizERPModel();

        public ActionResult Index()
        {
            var orders = db.SalesOrders
                .Include(o => o.Customer)
                .Include(o => o.Warehouse)
                .OrderByDescending(o => o.OrderDate)
                .ToList();
            return View(orders);
        }

        public ActionResult Create()
        {
            ViewBag.CustomerId = new SelectList(db.Customers, "CustomerId", "CustomerName");
            ViewBag.WarehouseId = new SelectList(db.Warehouses, "WarehouseId", "WarehouseName");
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create(int customerId, int warehouseId)
        {
            var order = new SalesOrder
            {
                CustomerId = customerId,
                WarehouseId = warehouseId,
                OrderDate = DateTime.Now,
                Status = "Draft",
                TotalAmount = 0
            };

            db.SalesOrders.Add(order);
            db.SaveChanges();

            order.SalesOrderNo = "SO-" + order.SalesOrderId.ToString("D4");
            db.SaveChanges();

            return RedirectToAction("Details", new { id = order.SalesOrderId });
        }

        public ActionResult Details(int? id)
        {
            if (id == null) return new HttpStatusCodeResult(HttpStatusCode.BadRequest);

            var order = db.SalesOrders
                .Include(o => o.Customer)
                .Include(o => o.Warehouse)
                .Include(o => o.SalesOrderDetails.Select(d => d.Product))
                .FirstOrDefault(o => o.SalesOrderId == id);

            if (order == null) return HttpNotFound();

            ViewBag.ProductId = new SelectList(db.Products, "ProductId", "ProductName");
            return View(order);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult AddLine(int salesOrderId, int productId, decimal quantity, decimal unitPrice)
        {
            var order = db.SalesOrders.Find(salesOrderId);
            if (order == null || order.Status != "Draft")
            {
                TempData["Error"] = "This order can no longer be edited.";
                return RedirectToAction("Details", new { id = salesOrderId });
            }

            db.SalesOrderDetails.Add(new SalesOrderDetail
            {
                SalesOrderId = salesOrderId,
                ProductId = productId,
                Quantity = quantity,
                UnitPrice = unitPrice
            });
            db.SaveChanges();

            order.TotalAmount = db.SalesOrderDetails
                .Where(d => d.SalesOrderId == salesOrderId)
                .Sum(d => d.TotalAmount);
            db.SaveChanges();

            return RedirectToAction("Details", new { id = salesOrderId });
        }

        // Marks the order Delivered and posts every line item into the
        // stock ledger as a SALE transaction. Unlike Receiving (which can
        // never fail on stock levels -- goods are arriving), Delivering
        // must check enough stock actually exists first, for every line,
        // BEFORE changing anything -- otherwise you could deliver a
        // sales order into negative stock.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Deliver(int id)
        {
            var order = db.SalesOrders
                .Include(o => o.SalesOrderDetails)
                .FirstOrDefault(o => o.SalesOrderId == id);

            if (order == null) return HttpNotFound();

            if (order.Status != "Draft")
            {
                TempData["Error"] = "This order has already been delivered.";
                return RedirectToAction("Details", new { id });
            }

            if (!order.SalesOrderDetails.Any())
            {
                TempData["Error"] = "Add at least one line item before delivering.";
                return RedirectToAction("Details", new { id });
            }

            // Pre-flight check across ALL lines before touching anything.
            foreach (var line in order.SalesOrderDetails)
            {
                var stock = db.WarehouseStocks
                    .FirstOrDefault(s => s.WarehouseId == order.WarehouseId && s.ProductId == line.ProductId);

                if (stock == null || stock.Quantity < line.Quantity)
                {
                    TempData["Error"] = "Not enough stock to deliver this order in full. Delivery cancelled.";
                    return RedirectToAction("Details", new { id });
                }
            }

            using (var dbTransaction = db.Database.BeginTransaction())
            {
                try
                {
                    foreach (var line in order.SalesOrderDetails)
                    {
                        db.StockTransactions.Add(new StockTransaction
                        {
                            ProductId = line.ProductId,
                            WarehouseId = order.WarehouseId,
                            TransactionType = "SALE",
                            Quantity = -line.Quantity,
                            ReferenceType = "SalesOrder",
                            ReferenceId = order.SalesOrderId,
                            TransactionDate = DateTime.Now
                        });

                        var stock = db.WarehouseStocks
                            .First(s => s.WarehouseId == order.WarehouseId && s.ProductId == line.ProductId);
                        stock.Quantity -= line.Quantity;
                    }

                    order.Status = "Delivered";
                    db.SaveChanges();
                    dbTransaction.Commit();

                    TempData["Success"] = "Order delivered and stock updated.";
                }
                catch
                {
                    dbTransaction.Rollback();
                    TempData["Error"] = "Something went wrong delivering this order.";
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
