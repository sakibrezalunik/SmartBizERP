using System;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Web.Mvc;
using SmartBizERP.Models;
using SmartBizERP.Utilities;

namespace SmartBizERP.Controllers
{
    public class UsersController : Controller
    {
        private SmartBizERPModel db = new SmartBizERPModel();

        // Only Admins can manage user accounts -- this is checked at the
        // top of every action here, not just hidden from the menu. Hiding
        // a link is not security; someone could still type the URL
        // directly. Re-checking on the server, every time, is what
        // actually enforces the restriction.
        private bool CurrentUserIsAdmin()
        {
            var username = User.Identity.Name;
            return db.Users
                .Where(u => u.Username == username)
                .SelectMany(u => u.UserRoles)
                .Any(ur => ur.Role.RoleName == "Admin");
        }

        public ActionResult Index(string searchTerm)
        {
            if (!CurrentUserIsAdmin()) return RedirectToAction("AccessDenied", "Account");

            var users = db.Users
                .Include(u => u.UserRoles.Select(ur => ur.Role))
                .AsQueryable();

            if (!string.IsNullOrWhiteSpace(searchTerm))
                users = users.Where(u => u.Username.Contains(searchTerm) || u.FullName.Contains(searchTerm));

            ViewBag.SearchTerm = searchTerm;
            return View(users.OrderBy(u => u.Username).ToList());
        }

        public ActionResult Create()
        {
            if (!CurrentUserIsAdmin()) return RedirectToAction("AccessDenied", "Account");

            ViewBag.Roles = db.Roles.ToList();
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create(string username, string password, string fullName, string email, int[] roleIds)
        {
            if (!CurrentUserIsAdmin()) return RedirectToAction("AccessDenied", "Account");

            if (db.Users.Any(u => u.Username == username))
                ModelState.AddModelError("", "This username already exists.");

            if (string.IsNullOrWhiteSpace(password) || password.Length < 6)
                ModelState.AddModelError("", "Password must be at least 6 characters.");

            if (ModelState.IsValid)
            {
                var user = new User
                {
                    Username = username,
                    PasswordHash = PasswordHasher.Hash(password),
                    FullName = fullName,
                    Email = email,
                    IsActive = true,
                    CreatedDate = DateTime.Now
                };
                db.Users.Add(user);
                db.SaveChanges();

                if (roleIds != null)
                {
                    foreach (var roleId in roleIds)
                        db.UserRoles.Add(new UserRole { UserId = user.UserId, RoleId = roleId });
                    db.SaveChanges();
                }

                TempData["Success"] = "User created successfully.";
                return RedirectToAction("Index");
            }

            ViewBag.Roles = db.Roles.ToList();
            return View();
        }

        public ActionResult Edit(int? id)
        {
            if (!CurrentUserIsAdmin()) return RedirectToAction("AccessDenied", "Account");
            if (id == null) return new HttpStatusCodeResult(HttpStatusCode.BadRequest);

            var user = db.Users.Include(u => u.UserRoles).FirstOrDefault(u => u.UserId == id);
            if (user == null) return HttpNotFound();

            ViewBag.Roles = db.Roles.ToList();
            ViewBag.SelectedRoleIds = user.UserRoles.Select(ur => ur.RoleId).ToList();
            return View(user);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit(int userId, string fullName, string email, bool isActive, int[] roleIds)
        {
            if (!CurrentUserIsAdmin()) return RedirectToAction("AccessDenied", "Account");

            var user = db.Users.Include(u => u.UserRoles).FirstOrDefault(u => u.UserId == userId);
            if (user == null) return HttpNotFound();

            user.FullName = fullName;
            user.Email = email;
            user.IsActive = isActive;

            // Replace this user's role assignments wholesale rather than
            // trying to diff old vs new -- simpler and just as correct
            // for a small role set like this.
            var existingRoles = db.UserRoles.Where(ur => ur.UserId == userId).ToList();
            foreach (var er in existingRoles)
                db.UserRoles.Remove(er);

            if (roleIds != null)
            {
                foreach (var roleId in roleIds)
                    db.UserRoles.Add(new UserRole { UserId = userId, RoleId = roleId });
            }

            db.SaveChanges();
            TempData["Success"] = "User updated successfully.";
            return RedirectToAction("Index");
        }

        public ActionResult ResetPassword(int? id)
        {
            if (!CurrentUserIsAdmin()) return RedirectToAction("AccessDenied", "Account");
            if (id == null) return new HttpStatusCodeResult(HttpStatusCode.BadRequest);

            var user = db.Users.Find(id);
            if (user == null) return HttpNotFound();
            return View(user);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult ResetPassword(int userId, string newPassword)
        {
            if (!CurrentUserIsAdmin()) return RedirectToAction("AccessDenied", "Account");

            if (string.IsNullOrWhiteSpace(newPassword) || newPassword.Length < 6)
            {
                TempData["Error"] = "Password must be at least 6 characters.";
                return RedirectToAction("ResetPassword", new { id = userId });
            }

            var user = db.Users.Find(userId);
            if (user == null) return HttpNotFound();

            user.PasswordHash = PasswordHasher.Hash(newPassword);
            db.SaveChanges();

            TempData["Success"] = "Password reset successfully.";
            return RedirectToAction("Index");
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing) db.Dispose();
            base.Dispose(disposing);
        }
    }
}
