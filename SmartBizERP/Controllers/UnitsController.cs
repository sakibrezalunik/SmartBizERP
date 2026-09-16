using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Web.Mvc;
using SmartBizERP.Models;

namespace SmartBizERP.Controllers
{
    public class UnitsController : Controller
    {
        private SmartBizERPModel db = new SmartBizERPModel();

        public ActionResult Index(string searchTerm)
        {
            var units = db.Units.AsQueryable();
            if (!string.IsNullOrWhiteSpace(searchTerm))
                units = units.Where(u => u.UnitName.Contains(searchTerm));
            ViewBag.SearchTerm = searchTerm;
            return View(units.OrderBy(u => u.UnitName).ToList());
        }

        public ActionResult Create() { return View(); }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create([Bind(Include = "UnitName,ShortName,IsActive")] Unit unit)
        {
            if (ModelState.IsValid)
            {
                unit.IsActive = true;
                db.Units.Add(unit);
                db.SaveChanges();
                TempData["Success"] = "Unit created successfully.";
                return RedirectToAction("Index");
            }
            return View(unit);
        }

        public ActionResult Edit(int? id)
        {
            if (id == null) return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            Unit unit = db.Units.Find(id);
            if (unit == null) return HttpNotFound();
            return View(unit);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit([Bind(Include = "UnitId,UnitName,ShortName,IsActive")] Unit unit)
        {
            if (ModelState.IsValid)
            {
                db.Entry(unit).State = EntityState.Modified;
                db.SaveChanges();
                TempData["Success"] = "Unit updated successfully.";
                return RedirectToAction("Index");
            }
            return View(unit);
        }

        public ActionResult Delete(int? id)
        {
            if (id == null) return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            Unit unit = db.Units.Find(id);
            if (unit == null) return HttpNotFound();
            return View(unit);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {
            Unit unit = db.Units.Find(id);
            unit.IsActive = false;
            db.Entry(unit).State = EntityState.Modified;
            db.SaveChanges();
            TempData["Success"] = "Unit deactivated.";
            return RedirectToAction("Index");
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing) db.Dispose();
            base.Dispose(disposing);
        }
    }
}
