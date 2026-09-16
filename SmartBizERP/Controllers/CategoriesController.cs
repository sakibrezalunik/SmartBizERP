using System;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Web.Mvc;
using SmartBizERP.Models;

namespace SmartBizERP.Controllers
{
    public class CategoriesController : Controller
    {
        private SmartBizERPModel db = new SmartBizERPModel();

        public ActionResult Index(string searchTerm)
        {
            var categories = db.Categories.AsQueryable();

            if (!string.IsNullOrWhiteSpace(searchTerm))
                categories = categories.Where(c => c.CategoryName.Contains(searchTerm));

            ViewBag.SearchTerm = searchTerm;
            return View(categories.OrderBy(c => c.CategoryName).ToList());
        }

        public ActionResult Create()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create([Bind(Include = "CategoryName,Description,IsActive")] Category category)
        {
            if (ModelState.IsValid)
            {
                category.CreatedDate = DateTime.Now;
                category.IsActive = true;
                db.Categories.Add(category);
                db.SaveChanges();
                TempData["Success"] = "Category created successfully.";
                return RedirectToAction("Index");
            }
            return View(category);
        }

        public ActionResult Edit(int? id)
        {
            if (id == null) return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            Category category = db.Categories.Find(id);
            if (category == null) return HttpNotFound();
            return View(category);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit([Bind(Include = "CategoryId,CategoryName,Description,IsActive,CreatedDate")] Category category)
        {
            if (ModelState.IsValid)
            {
                db.Entry(category).State = EntityState.Modified;
                db.SaveChanges();
                TempData["Success"] = "Category updated successfully.";
                return RedirectToAction("Index");
            }
            return View(category);
        }

        public ActionResult Delete(int? id)
        {
            if (id == null) return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            Category category = db.Categories.Find(id);
            if (category == null) return HttpNotFound();
            return View(category);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {
            Category category = db.Categories.Find(id);
            category.IsActive = false;
            db.Entry(category).State = EntityState.Modified;
            db.SaveChanges();
            TempData["Success"] = "Category deactivated.";
            return RedirectToAction("Index");
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing) db.Dispose();
            base.Dispose(disposing);
        }
    }
}
