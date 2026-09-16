using System;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Web.Mvc;
using SmartBizERP.Models;

namespace SmartBizERP.Controllers
{
    public class SuppliersController : Controller
    {
        private SmartBizERPModel db = new SmartBizERPModel();

        public ActionResult Index(string searchTerm)
        {
            var suppliers = db.Suppliers.AsQueryable();
            if (!string.IsNullOrWhiteSpace(searchTerm))
                suppliers = suppliers.Where(s => s.SupplierName.Contains(searchTerm) || s.SupplierCode.Contains(searchTerm));
            ViewBag.SearchTerm = searchTerm;
            return View(suppliers.OrderBy(s => s.SupplierName).ToList());
        }

        public ActionResult Create() { return View(); }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create([Bind(Include = "SupplierCode,SupplierName,Phone,Email,Address,IsActive")] Supplier supplier)
        {
            if (db.Suppliers.Any(s => s.SupplierCode == supplier.SupplierCode))
                ModelState.AddModelError("SupplierCode", "This supplier code already exists.");

            if (ModelState.IsValid)
            {
                supplier.CreatedDate = DateTime.Now;
                supplier.IsActive = true;
                db.Suppliers.Add(supplier);
                db.SaveChanges();
                TempData["Success"] = "Supplier created successfully.";
                return RedirectToAction("Index");
            }
            return View(supplier);
        }

        public ActionResult Edit(int? id)
        {
            if (id == null) return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            Supplier supplier = db.Suppliers.Find(id);
            if (supplier == null) return HttpNotFound();
            return View(supplier);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit([Bind(Include = "SupplierId,SupplierCode,SupplierName,Phone,Email,Address,IsActive,CreatedDate")] Supplier supplier)
        {
            if (ModelState.IsValid)
            {
                db.Entry(supplier).State = EntityState.Modified;
                db.SaveChanges();
                TempData["Success"] = "Supplier updated successfully.";
                return RedirectToAction("Index");
            }
            return View(supplier);
        }

        public ActionResult Delete(int? id)
        {
            if (id == null) return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            Supplier supplier = db.Suppliers.Find(id);
            if (supplier == null) return HttpNotFound();
            return View(supplier);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {
            Supplier supplier = db.Suppliers.Find(id);
            supplier.IsActive = false;
            db.Entry(supplier).State = EntityState.Modified;
            db.SaveChanges();
            TempData["Success"] = "Supplier deactivated.";
            return RedirectToAction("Index");
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing) db.Dispose();
            base.Dispose(disposing);
        }
    }
}
