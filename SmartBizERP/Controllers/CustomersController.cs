using System;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Web.Mvc;
using SmartBizERP.Models;

namespace SmartBizERP.Controllers
{
    public class CustomersController : Controller
    {
        private SmartBizERPModel db = new SmartBizERPModel();

        public ActionResult Index(string searchTerm)
        {
            var customers = db.Customers.AsQueryable();
            if (!string.IsNullOrWhiteSpace(searchTerm))
                customers = customers.Where(c => c.CustomerName.Contains(searchTerm) || c.CustomerCode.Contains(searchTerm));
            ViewBag.SearchTerm = searchTerm;
            return View(customers.OrderBy(c => c.CustomerName).ToList());
        }

        public ActionResult Create() { return View(); }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create([Bind(Include = "CustomerCode,CustomerName,Phone,Email,Address,CreditLimit,IsActive")] Customer customer)
        {
            if (db.Customers.Any(c => c.CustomerCode == customer.CustomerCode))
                ModelState.AddModelError("CustomerCode", "This customer code already exists.");

            if (ModelState.IsValid)
            {
                customer.CreatedDate = DateTime.Now;
                customer.IsActive = true;
                db.Customers.Add(customer);
                db.SaveChanges();
                TempData["Success"] = "Customer created successfully.";
                return RedirectToAction("Index");
            }
            return View(customer);
        }

        public ActionResult Edit(int? id)
        {
            if (id == null) return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            Customer customer = db.Customers.Find(id);
            if (customer == null) return HttpNotFound();
            return View(customer);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit([Bind(Include = "CustomerId,CustomerCode,CustomerName,Phone,Email,Address,CreditLimit,IsActive,CreatedDate")] Customer customer)
        {
            if (ModelState.IsValid)
            {
                db.Entry(customer).State = EntityState.Modified;
                db.SaveChanges();
                TempData["Success"] = "Customer updated successfully.";
                return RedirectToAction("Index");
            }
            return View(customer);
        }

        public ActionResult Delete(int? id)
        {
            if (id == null) return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            Customer customer = db.Customers.Find(id);
            if (customer == null) return HttpNotFound();
            return View(customer);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {
            Customer customer = db.Customers.Find(id);
            customer.IsActive = false;
            db.Entry(customer).State = EntityState.Modified;
            db.SaveChanges();
            TempData["Success"] = "Customer deactivated.";
            return RedirectToAction("Index");
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing) db.Dispose();
            base.Dispose(disposing);
        }
    }
}
