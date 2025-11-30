using Microsoft.AspNet.Identity;
using sem3.Models.Entities;
using sem3.Models.ModelViews;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using User = sem3.Models.ModelViews.UserM;

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
                    else
                    {
                        System.Diagnostics.Debug.WriteLine($"ADMIN PASSWORD INVALID");
                    }
                }
                else
                {
                    System.Diagnostics.Debug.WriteLine($"No admin user found with phone: {model.PhoneNumber}");
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
                        Session["CurrentUser"] = new User
                        {
                            UserID = user.UserID,
                            FullName = user.FullName,
                            MobileNumber = user.MobileNumber,
                            Email = user.Email,
                            PasswordHash = user.PasswordHash,
                            Address = user.Address,
                            RegistrationDate = user.RegistrationDate,
                        };

                        Session["CurrentUserId"] = user.UserID;
                        Session["IsAdmin"] = false;

                        return RedirectToAction("Index", "Home");
                    }
                }
                else
                {
                    System.Diagnostics.Debug.WriteLine($"No regular user found with phone: {model.PhoneNumber}");
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
        public ActionResult Register(Register model)
        {
            if (ModelState.IsValid)
            {
                bool phoneExistsInAdmin = _db.AdminUsers.Any(a => a.MobileNumber == model.Phone);
                bool phoneExistsInUsers = _db.Users.Any(u => u.MobileNumber == model.Phone);

                if (phoneExistsInAdmin || phoneExistsInUsers)
                {
                    ModelState.AddModelError("", "Phone number already exists.");
                    return View(model);
                }
                var newUser = new sem3.Models.Entities.User
                {
                    FullName = model.FullName,
                    MobileNumber = model.Phone,
                    PasswordHash = HashPassword(model.Password),
                    RegistrationDate = DateTime.Now,
                    Email = model.Email,
                    Address = model.Address,
                    Active = true
                };

                _db.Users.Add(newUser);
                try
                {
                    _db.SaveChanges();
                    return RedirectToAction("Login");
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
                    ModelState.AddModelError("", "Validation errors: " + string.Join("; ", errorMessages));
                }
                catch (System.Data.Entity.Infrastructure.DbUpdateException ex)
                {
                    Exception inner = ex;
                    while (inner.InnerException != null)
                    {
                        inner = inner.InnerException;
                    }
                    ModelState.AddModelError("", $"Database error: {inner.Message}");
                }
                catch (Exception ex)
                {
                    ModelState.AddModelError("", $"Unexpected error: {ex.Message}");
                }
            }
            return View(model);
        }

        private bool VerifyPasswordForUser(string providedPassword, string storedPassword)
        {
            System.Diagnostics.Debug.WriteLine($"=== VERIFY USER PASSWORD ===");
            System.Diagnostics.Debug.WriteLine($"Stored password: '{storedPassword}'");
            System.Diagnostics.Debug.WriteLine($"Stored password length: {storedPassword?.Length}");
            System.Diagnostics.Debug.WriteLine($"Stored password is plain text: {IsPlainTextPassword(storedPassword)}");

            if (IsPlainTextPassword(storedPassword))
            {
                bool result = providedPassword == storedPassword;
                return result;
            }
            try
            {
                var passwordHasher = new PasswordHasher();
                var result = passwordHasher.VerifyHashedPassword(storedPassword, providedPassword);

                return result == PasswordVerificationResult.Success;
            }
            catch (Exception ex)
            {
                return false;
            }
        }

        private bool VerifyPasswordForAdmin(string providedPassword, string storedPassword)
        {
            System.Diagnostics.Debug.WriteLine($"=== VERIFY ADMIN PASSWORD ===");
            System.Diagnostics.Debug.WriteLine($"Stored admin password: '{storedPassword}'");
            System.Diagnostics.Debug.WriteLine($"Stored admin password length: {storedPassword?.Length}");

            bool result = providedPassword == storedPassword;
            System.Diagnostics.Debug.WriteLine($"Admin password comparison result: {result}");
            return result;
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

            System.Diagnostics.Debug.WriteLine($"IsPlainTextPassword check: Length={password.Length}, ContainsSlash={password.Contains("/")}, ContainsPlus={password.Contains("+")}, ContainsEqual={password.Contains("=")}, Result={isPlainText}");

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