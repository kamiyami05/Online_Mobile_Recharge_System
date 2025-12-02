using Microsoft.AspNet.Identity;
using sem3.Models.Entities;
using sem3.Models.ModelViews;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;

namespace sem3.Controllers
{
    public class LoginController : Controller
    {
        private readonly OnlineRechargeDBEntities _db = new OnlineRechargeDBEntities();

        public ActionResult Login()
        {
            return View(new Login());
        }

        [HttpPost]
        public ActionResult Login(Login model)
        {
            if (ModelState.IsValid)
            {
                var adminUser = _db.AdminUsers.FirstOrDefault(a => a.MobileNumber == model.PhoneNumber);

                if (adminUser != null)
                {
                    bool isAdminPasswordValid = VerifyPasswordForAdmin(model.Password, adminUser.PasswordHash);
                    if (isAdminPasswordValid)
                    {
                        Session["CurrentUser"] = new User
                        {
                            UserID = adminUser.AdminID,
                            FullName = "Administrator",
                            MobileNumber = adminUser.MobileNumber,
                            Email = adminUser.Email,
                            Address = "System Admin"
                        };

                        Session["CurrentUserId"] = adminUser.AdminID;
                        Session["IsAdmin"] = true;

                        return RedirectToAction("Index", "Usermgmt", new { area = "Admin" });
                    }
                }

                var user = _db.Users.FirstOrDefault(u => u.MobileNumber == model.PhoneNumber);
                if (user != null)
                {
                    if (user.Active == false)
                    {
                        ModelState.AddModelError("", "Your account has been locked.");
                        return View(model);
                    }

                    bool isUserPasswordValid = VerifyPasswordForUser(model.Password, user.PasswordHash);

                    if (isUserPasswordValid)
                    {
                        Session["CurrentUser"] = new UserM
                        {
                            UserID = user.UserID,
                            FullName = user.FullName,
                            MobileNumber = user.MobileNumber,
                            Email = user.Email,
                            PasswordHash = user.PasswordHash,
                            Address = user.Address,
                            RegistrationDate = user.RegistrationDate
                        };

                        Session["CurrentUserId"] = user.UserID;
                        Session["IsAdmin"] = false;

                        return RedirectToAction("Index", "Home");
                    }
                }

                ModelState.AddModelError("", "Incorrect phone number or password.");
            }
            return View(model);
        }

        public ActionResult Logout()
        {
            Session.Clear();
            if (Request.Cookies["ASP.NET_SessionId"] != null)
                Response.Cookies["ASP.NET_SessionId"].Expires = DateTime.Now.AddDays(-1);

            return RedirectToAction("Index", "Home");
        }

        public ActionResult Register()
        {
            return View(new Register());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public JsonResult Register(Register model)
        {
            // Remove OTP validation from ModelState
            ModelState.Remove("OTP");

            if (!ModelState.IsValid)
            {
                var errors = ModelState.Values.SelectMany(v => v.Errors);
                return Json(new
                {
                    success = false,
                    message = "Invalid data: " + string.Join(", ", errors.Select(e => e.ErrorMessage))
                });
            }

            // OTP validation (mặc định là 1234)
            if (model.OTP != "1234")
            {
                return Json(new
                {
                    success = false,
                    message = "Invalid OTP. Please enter 1234."
                });
            }

            // Check if phone number already exists
            bool phoneExistsInAdmin = _db.AdminUsers.Any(a => a.MobileNumber == model.Phone);
            bool phoneExistsInUsers = _db.Users.Any(u => u.MobileNumber == model.Phone);

            if (phoneExistsInAdmin || phoneExistsInUsers)
            {
                return Json(new
                {
                    success = false,
                    message = "Phone number already exists."
                });
            }

            // Create new user with all required fields
            var newUser = new sem3.Models.Entities.User
            {
                FullName = model.Phone,
                MobileNumber = model.Phone,
                PasswordHash = HashPassword(model.Password),
                RegistrationDate = DateTime.Now,
                Email = null,
                Address = null,
                Active = true
            };

            _db.Users.Add(newUser);

            try
            {
                _db.SaveChanges();

                return Json(new
                {
                    success = true,
                    message = "Registration successful!"
                });
            }
            catch (System.Data.Entity.Validation.DbEntityValidationException ex)
            {
                var errorMessages = new List<string>();
                foreach (var validationErrors in ex.EntityValidationErrors)
                {
                    foreach (var validationError in validationErrors.ValidationErrors)
                    {
                        errorMessages.Add($"Property: {validationError.PropertyName} Error: {validationError.ErrorMessage}");
                    }
                }
                return Json(new
                {
                    success = false,
                    message = "Validation errors: " + string.Join("; ", errorMessages)
                });
            }
            catch (System.Data.Entity.Infrastructure.DbUpdateException ex)
            {
                Exception inner = ex;
                while (inner.InnerException != null)
                {
                    inner = inner.InnerException;
                }
                return Json(new
                {
                    success = false,
                    message = $"Database error: {inner.Message}"
                });
            }
            catch (Exception ex)
            {
                return Json(new
                {
                    success = false,
                    message = $"Unexpected error: {ex.Message}"
                });
            }
        }

        [HttpPost]
        public JsonResult SendOTP(string phoneNumber)
        {
            return Json(new
            {
                success = true,
                message = "OTP sent successfully. Use 1234 as OTP."
            }, JsonRequestBehavior.AllowGet);
        }

        private bool VerifyPasswordForUser(string providedPassword, string storedPassword)
        {
            if (IsPlainTextPassword(storedPassword))
            {
                return providedPassword == storedPassword;
            }

            try
            {
                var passwordHasher = new PasswordHasher();
                var result = passwordHasher.VerifyHashedPassword(storedPassword, providedPassword);
                return result == PasswordVerificationResult.Success;
            }
            catch
            {
                return false;
            }
        }

        private bool VerifyPasswordForAdmin(string providedPassword, string storedPassword)
        {
            return providedPassword == storedPassword;
        }

        private bool IsPlainTextPassword(string password)
        {
            if (string.IsNullOrEmpty(password))
                return true;

            bool isPlainText = password.Length <= 20 &&
                               !password.Contains("/") &&
                               !password.Contains("+") &&
                               !password.Contains("=") &&
                               !password.Contains(" ");

            return isPlainText;
        }

        private string HashPassword(string password)
        {
            var passwordHasher = new PasswordHasher();
            return passwordHasher.HashPassword(password);
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
                _db.Dispose();
            base.Dispose(disposing);
        }
    }
}
