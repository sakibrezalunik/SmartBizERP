using System;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Web.Mvc;
using SmartBizERP.Models;

namespace SmartBizERP.Controllers
{
    public class WarehousesController : Controller
    {
        private SmartBizERPModel db = new SmartBizERPModel();

        public ActionResult Index(string searchTerm)
        {
            var warehouses = db.Warehouses.AsQueryable();
            if (!string.IsNullOrWhiteSpace(searchTerm))
                warehouses = warehouses.Where(w => w.WarehouseName.Contains(searchTerm) || w.WarehouseCode.Contains(searchTerm));
            ViewBag.SearchTerm = searchTerm;
            return View(warehouses.OrderBy(w => w.WarehouseName).ToList());
        }

        public ActionResult Create() { return View(); }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create([Bind(Include = "WarehouseCode,WarehouseName,Address,IsActive")] Warehouse warehouse)
        {
            if (db.Warehouses.Any(w => w.WarehouseCode == warehouse.WarehouseCode))
                ModelState.AddModelError("WarehouseCode", "This warehouse code already exists.");

            if (ModelState.IsValid)
            {
                warehouse.CreatedDate = DateTime.Now;
                warehouse.IsActive = true;
                db.Warehouses.Add(warehouse);
                db.SaveChanges();
                TempData["Success"] = "Warehouse created successfully.";
                return RedirectToAction("Index");
            }
            return View(warehouse);
        }

        public ActionResult Edit(int? id)
        {
            if (id == null) return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            Warehouse warehouse = db.Warehouses.Find(id);
            if (warehouse == null) return HttpNotFound();
            return View(warehouse);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit([Bind(Include = "WarehouseId,WarehouseCode,WarehouseName,Address,IsActive,CreatedDate")] Warehouse warehouse)
        {
            if (ModelState.IsValid)
            {
                db.Entry(warehouse).State = EntityState.Modified;
                db.SaveChanges();
                TempData["Success"] = "Warehouse updated successfully.";
                return RedirectToAction("Index");
            }
            return View(warehouse);
        }

        public ActionResult Delete(int? id)
        {
            if (id == null) return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            Warehouse warehouse = db.Warehouses.Find(id);
            if (warehouse == null) return HttpNotFound();
            return View(warehouse);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {
            Warehouse warehouse = db.Warehouses.Find(id);
            warehouse.IsActive = false;
            db.Entry(warehouse).State = EntityState.Modified;
            db.SaveChanges();
            TempData["Success"] = "Warehouse deactivated.";
            return RedirectToAction("Index");
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing) db.Dispose();
            base.Dispose(disposing);
        }
    }
}
