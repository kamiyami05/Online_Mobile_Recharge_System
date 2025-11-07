using System;
using System.Linq;
using System.Web.Mvc;
using sem3.Models.Entities;
using sem3.Models.ModelViews;
using Microsoft.AspNet.Identity;

namespace sem3.Controllers
{
    public class ProfileController : Controller
    {
        private readonly Recharge_SystemEntities _db = new Recharge_SystemEntities();

        // GET: Profile/Index
        public ActionResult Index()
        {
            if (Session["CurrentUser"] == null)
            {
                return RedirectToAction("Login", "Login");
            }

            var sessionUser = Session["CurrentUser"] as UserM;
            var userFromDb = _db.Users.Find(sessionUser.Id);
            if (userFromDb == null)
            {
                Session.Clear();
                return RedirectToAction("Login", "Login");
            }

            var viewModel = new UserM
            {
                Id = userFromDb.Id,
                FullName = userFromDb.FullName,
                Phone = userFromDb.Phone,
                Email = userFromDb.Email,
                Address = userFromDb.Address,
                CreatedAt = userFromDb.CreatedAt,
                Password = "",
                Role = userFromDb.Role,
                IsActive = userFromDb.IsActive
            };

            // Truyền các model rỗng cho các form popup
            ViewBag.EmailForm = new ChangeEmail();
            ViewBag.PasswordForm = new ChangePassword();

            return View(viewModel);
        }

        // POST: Profile/UpdateInfo (Form sửa thông tin cá nhân)
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult UpdateInfo(UserM model) // Đổi tên từ Index thành UpdateInfo
        {
            if (Session["CurrentUser"] == null)
            {
                return RedirectToAction("Login", "Login");
            }

            if (string.IsNullOrWhiteSpace(model.FullName))
            {
                TempData["ErrorMessage"] = "Update failed: Full name is required.";
                return RedirectToAction("Index");
            }

            try
            {
                var userInDb = _db.Users.Find(model.Id);
                if (userInDb == null) return HttpNotFound();

                userInDb.FullName = model.FullName;
                userInDb.Phone = model.Phone;
                userInDb.Address = model.Address;

                _db.SaveChanges();

                var sessionUser = Session["CurrentUser"] as UserM;
                sessionUser.FullName = model.FullName;
                sessionUser.Phone = model.Phone;
                sessionUser.Address = model.Address;
                Session["CurrentUser"] = sessionUser;

                TempData["SuccessMessage"] = "Profile updated successfully!";
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = "An error occurred: " + ex.Message;
            }

            return RedirectToAction("Index");
        }

        // POST: Profile/ChangeEmail
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult ChangeEmail(ChangeEmail model)
        {
            if (Session["CurrentUser"] == null)
            {
                return RedirectToAction("Login", "Login");
            }
            var sessionUser = Session["CurrentUser"] as UserM;

            if (_db.Users.Any(u => u.Email == model.NewEmail && u.Id != sessionUser.Id))
            {
                ModelState.AddModelError("NewEmail", "This email address is already in use.");
            }

            if (ModelState.IsValid)
            {
                try
                {
                    var userInDb = _db.Users.Find(sessionUser.Id);
                    if (userInDb == null) return HttpNotFound();

                    userInDb.Email = model.NewEmail;
                    _db.SaveChanges();

                    sessionUser.Email = model.NewEmail;
                    Session["CurrentUser"] = sessionUser;

                    TempData["SuccessMessage"] = "Email updated successfully!";
                }
                catch (Exception ex)
                {
                    TempData["ErrorMessage"] = "An error occurred: " + ex.Message;
                }
            }
            else
            {
                TempData["ErrorMessage"] = "Email update failed: " + string.Join("; ", ModelState.Values
                    .SelectMany(v => v.Errors)
                    .Select(e => e.ErrorMessage));
            }
            return RedirectToAction("Index");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult ChangePassword(ChangePassword model)
        {
            if (Session["CurrentUser"] == null)
            {
                return RedirectToAction("Login", "Login");
            }
            var sessionUser = Session["CurrentUser"] as UserM;
            var userInDb = _db.Users.Find(sessionUser.Id);
            if (userInDb == null) return HttpNotFound();

            if (!VerifyPassword(model.OldPassword, userInDb.Password))
            {
                ModelState.AddModelError("OldPassword", "Incorrect current password.");
            }

            if (ModelState.IsValid)
            {
                try
                {
                    userInDb.Password = HashPassword(model.NewPassword);
                    _db.SaveChanges();
                    TempData["SuccessMessage"] = "Password updated successfully!";
                }
                catch (Exception ex)
                {
                    TempData["ErrorMessage"] = "An error occurred: " + ex.Message;
                }
            }
            else
            {
                TempData["ErrorMessage"] = "Password update failed: " + string.Join("; ", ModelState.Values
                    .SelectMany(v => v.Errors)
                    .Select(e => e.ErrorMessage));
            }
            return RedirectToAction("Index");
        }

        private bool VerifyPassword(string providedPassword, string hashedPassword)
        {
            var passwordHasher = new PasswordHasher();
            return passwordHasher.VerifyHashedPassword(hashedPassword, providedPassword) == PasswordVerificationResult.Success;
        }

        private string HashPassword(string password)
        {
            var passwordHasher = new PasswordHasher();
            return passwordHasher.HashPassword(password);
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                _db.Dispose();
            }
            base.Dispose(disposing);
        }
    }
}