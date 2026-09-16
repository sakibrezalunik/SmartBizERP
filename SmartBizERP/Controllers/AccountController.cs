using System;
using System.Linq;
using System.Web.Mvc;
using System.Web.Security;
using SmartBizERP.Models;
using SmartBizERP.Utilities;

namespace SmartBizERP.Controllers
{
    public class AccountController : Controller
    {
        private SmartBizERPModel db = new SmartBizERPModel();

        // GET: Account/Login
        [AllowAnonymous]
        public ActionResult Login(string returnUrl)
        {
            ViewBag.ReturnUrl = returnUrl;
            return View();
        }

        // POST: Account/Login
        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public ActionResult Login(string username, string password, string returnUrl)
        {
            var user = db.Users.FirstOrDefault(u => u.Username == username && u.IsActive);

            if (user == null || !PasswordHasher.Verify(password, user.PasswordHash))
            {
                // Deliberately vague -- never reveal whether the username
                // or the password was the wrong part. Confirming a valid
                // username exists is itself useful information to an
                // attacker trying to guess their way in.
                ModelState.AddModelError("", "Invalid username or password.");
                return View();
            }

            FormsAuthentication.SetAuthCookie(user.Username, false);

            db.AuditLogs.Add(new AuditLog
            {
                UserId = user.UserId,
                Action = "Login",
                Details = "User logged in successfully.",
                ActionDate = DateTime.Now
            });
            db.SaveChanges();

            if (Url.IsLocalUrl(returnUrl))
                return Redirect(returnUrl);

            return RedirectToAction("Index", "Home");
        }

        // GET/POST: Account/Logout
        public ActionResult Logout()
        {
            var username = User.Identity.Name;
            var user = db.Users.FirstOrDefault(u => u.Username == username);

            if (user != null)
            {
                db.AuditLogs.Add(new AuditLog
                {
                    UserId = user.UserId,
                    Action = "Logout",
                    Details = "User logged out.",
                    ActionDate = DateTime.Now
                });
                db.SaveChanges();
            }

            FormsAuthentication.SignOut();
            return RedirectToAction("Login", "Account");
        }

        // GET: Account/AccessDenied
        [AllowAnonymous]
        public ActionResult AccessDenied()
        {
            return View();
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing) db.Dispose();
            base.Dispose(disposing);
        }
    }
}
