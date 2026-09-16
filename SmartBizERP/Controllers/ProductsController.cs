using System;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Web.Mvc;
using SmartBizERP.Models;

namespace SmartBizERP.Controllers
{
    public class ProductsController : Controller
    {
        private SmartBizERPModel db = new SmartBizERPModel();

        // GET: Products
        public ActionResult Index(string searchTerm, int? categoryId, int page = 1)
        {
            int pageSize = 10;

            var products = db.Products
                .Include(p => p.Category)
                .Include(p => p.Unit)
                .AsQueryable();

            if (!string.IsNullOrWhiteSpace(searchTerm))
            {
                products = products.Where(p =>
                    p.ProductName.Contains(searchTerm) ||
                    p.ProductCode.Contains(searchTerm));
            }

            if (categoryId.HasValue)
            {
                products = products.Where(p => p.CategoryId == categoryId.Value);
            }

            ViewBag.CategoryList = new SelectList(db.Categories.OrderBy(c => c.CategoryName), "CategoryId", "CategoryName", categoryId);
            ViewBag.SearchTerm = searchTerm;
            ViewBag.CurrentPage = page;

            var totalCount = products.Count();
            ViewBag.TotalPages = (int)Math.Ceiling(totalCount / (double)pageSize);

            var pagedProducts = products
                .OrderBy(p => p.ProductName)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToList();

            return View(pagedProducts);
        }

        // GET: Products/Details/5
        public ActionResult Details(int? id)
        {
            if (id == null)
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);

            Product product = db.Products
                .Include(p => p.Category)
                .Include(p => p.Unit)
                .FirstOrDefault(p => p.ProductId == id);

            if (product == null)
                return HttpNotFound();

            return View(product);
        }

        // GET: Products/Create
        public ActionResult Create()
        {
            ViewBag.CategoryId = new SelectList(db.Categories, "CategoryId", "CategoryName");
            ViewBag.UnitId = new SelectList(db.Units, "UnitId", "UnitName");
            return View();
        }

        // POST: Products/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create([Bind(Include = "ProductCode,ProductName,CategoryId,UnitId,CostPrice,SellingPrice,ReorderLevel,IsActive")] Product product)
        {
            if (db.Products.Any(p => p.ProductCode == product.ProductCode))
            {
                ModelState.AddModelError("ProductCode", "This product code already exists.");
            }

            if (ModelState.IsValid)
            {
                product.CreatedDate = DateTime.Now;
                product.IsActive = true;
                db.Products.Add(product);
                db.SaveChanges();
                TempData["Success"] = "Product created successfully.";
                return RedirectToAction("Index");
            }

            ViewBag.CategoryId = new SelectList(db.Categories, "CategoryId", "CategoryName", product.CategoryId);
            ViewBag.UnitId = new SelectList(db.Units, "UnitId", "UnitName", product.UnitId);
            return View(product);
        }

        // GET: Products/Edit/5
        public ActionResult Edit(int? id)
        {
            if (id == null)
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);

            Product product = db.Products.Find(id);
            if (product == null)
                return HttpNotFound();

            ViewBag.CategoryId = new SelectList(db.Categories, "CategoryId", "CategoryName", product.CategoryId);
            ViewBag.UnitId = new SelectList(db.Units, "UnitId", "UnitName", product.UnitId);
            return View(product);
        }

        // POST: Products/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit([Bind(Include = "ProductId,ProductCode,ProductName,CategoryId,UnitId,CostPrice,SellingPrice,ReorderLevel,IsActive,CreatedDate")] Product product)
        {
            if (db.Products.Any(p => p.ProductCode == product.ProductCode && p.ProductId != product.ProductId))
            {
                ModelState.AddModelError("ProductCode", "This product code already exists.");
            }

            if (ModelState.IsValid)
            {
                db.Entry(product).State = EntityState.Modified;
                db.SaveChanges();
                TempData["Success"] = "Product updated successfully.";
                return RedirectToAction("Index");
            }

            ViewBag.CategoryId = new SelectList(db.Categories, "CategoryId", "CategoryName", product.CategoryId);
            ViewBag.UnitId = new SelectList(db.Units, "UnitId", "UnitName", product.UnitId);
            return View(product);
        }

        // GET: Products/Delete/5
        public ActionResult Delete(int? id)
        {
            if (id == null)
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);

            Product product = db.Products
                .Include(p => p.Category)
                .Include(p => p.Unit)
                .FirstOrDefault(p => p.ProductId == id);

            if (product == null)
                return HttpNotFound();

            return View(product);
        }

        // POST: Products/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {
            Product product = db.Products.Find(id);

            // Soft delete instead of hard delete
            product.IsActive = false;
            db.Entry(product).State = EntityState.Modified;
            db.SaveChanges();

            TempData["Success"] = "Product deactivated successfully.";
            return RedirectToAction("Index");
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                db.Dispose();
            }
            base.Dispose(disposing);
        }
    }
}
