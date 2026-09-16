using System.Data.Entity;
using System.Linq;
using System.Web.Mvc;
using SmartBizERP.Models;

namespace SmartBizERP.Controllers
{
    public class ReportsController : Controller
    {
        private SmartBizERPModel db = new SmartBizERPModel();

        public ActionResult Index()
        {
            return View();
        }

        public ActionResult SalesReport()
        {
            var orders = db.SalesOrders
                .Include(o => o.Customer)
                .Where(o => o.Status == "Delivered")
                .OrderByDescending(o => o.OrderDate)
                .ToList();

            ViewBag.GrandTotal = orders.Sum(o => o.TotalAmount);
            return View(orders);
        }

        public ActionResult PurchaseReport()
        {
            var orders = db.PurchaseOrders
                .Include(o => o.Supplier)
                .Where(o => o.Status == "Received")
                .OrderByDescending(o => o.OrderDate)
                .ToList();

            ViewBag.GrandTotal = orders.Sum(o => o.TotalAmount);
            return View(orders);
        }

        public ActionResult InventoryReport()
        {
            // Reuses the same low-stock comparison as Stock/Index in
            // Phase 3, but scoped to just the products actually below
            // their reorder level -- this is meant to be read at a
            // glance, not browsed like the full stock grid.
            var lowStock = db.WarehouseStocks
                .Include(s => s.Warehouse)
                .Include(s => s.Product)
                .Where(s => s.Quantity <= s.Product.ReorderLevel)
                .OrderBy(s => s.Product.ProductName)
                .ToList();

            return View(lowStock);
        }

        public ActionResult ProfitReport()
        {
            // Profit per delivered line = (what it sold for - what it
            // cost) * quantity. This only makes sense for DELIVERED sales
            // orders -- a Draft order hasn't actually moved any stock or
            // recognized any revenue yet.
            var lines = db.SalesOrderDetails
                .Include(d => d.SalesOrder)
                .Include(d => d.Product)
                .Where(d => d.SalesOrder.Status == "Delivered")
                .ToList();

            var report = lines.Select(d => new
            {
                d.SalesOrder.SalesOrderNo,
                d.SalesOrder.OrderDate,
                ProductName = d.Product.ProductName,
                d.Quantity,
                d.UnitPrice,
                CostPrice = d.Product.CostPrice,
                Revenue = d.TotalAmount,
                Profit = (d.UnitPrice - d.Product.CostPrice) * d.Quantity
            }).OrderByDescending(r => r.OrderDate).ToList();

            ViewBag.TotalRevenue = report.Sum(r => r.Revenue);
            ViewBag.TotalProfit = report.Sum(r => r.Profit);

            return View(report);
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing) db.Dispose();
            base.Dispose(disposing);
        }
    }
}
