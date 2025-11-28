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
                System.Diagnostics.Debug.WriteLine($"=== LOGIN ATTEMPT ===");
                System.Diagnostics.Debug.WriteLine($"Phone: {model.PhoneNumber}, Password: {model.Password}");

                // 1. Kiểm tra trong bảng AdminUsers trước
                var adminUser = _db.AdminUsers.FirstOrDefault(a => a.MobileNumber == model.PhoneNumber);

                if (adminUser != null)
                {
                    System.Diagnostics.Debug.WriteLine($"Admin user found: {adminUser.Username}");

                    bool isAdminPasswordValid = VerifyPasswordForAdmin(model.Password, adminUser.PasswordHash);
                    System.Diagnostics.Debug.WriteLine($"Admin password verification result: {isAdminPasswordValid}");

                    if (isAdminPasswordValid)
                    {
                        System.Diagnostics.Debug.WriteLine($"ADMIN LOGIN SUCCESSFUL");
                        // Đăng nhập thành công với tài khoản admin
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

                // 2. Nếu không phải admin, kiểm tra trong bảng Users
                var user = _db.Users.FirstOrDefault(u => u.MobileNumber == model.PhoneNumber);
                if (user != null)
                {
                    System.Diagnostics.Debug.WriteLine($"Regular user found: {user.FullName}");
                    bool isUserPasswordValid = VerifyPasswordForUser(model.Password, user.PasswordHash);
                    System.Diagnostics.Debug.WriteLine($"User password verification result: {isUserPasswordValid}");

                    if (isUserPasswordValid)
                    {
                        System.Diagnostics.Debug.WriteLine($"USER LOGIN SUCCESSFUL");
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

                System.Diagnostics.Debug.WriteLine($"LOGIN FAILED - No valid user found");
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
                // Kiểm tra số điện thoại đã tồn tại trong cả 2 bảng
                bool phoneExistsInAdmin = _db.AdminUsers.Any(a => a.MobileNumber == model.Phone);
                bool phoneExistsInUsers = _db.Users.Any(u => u.MobileNumber == model.Phone);

                if (phoneExistsInAdmin || phoneExistsInUsers)
                {
                    ModelState.AddModelError("", "Phone number already exists.");
                    return View(model);
                }

                // Tiếp tục tạo user mới
                var newUser = new sem3.Models.Entities.User
                {
                    FullName = model.FullName,
                    MobileNumber = model.Phone,
                    PasswordHash = HashPassword(model.Password), // Hash password cho user mới
                    RegistrationDate = DateTime.Now,
                    Email = model.Email, // Thêm email nếu có
                    Address = model.Address // Thêm address nếu có
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

        // Verify User's password - Sửa lại để hoạt động chính xác
        private bool VerifyPasswordForUser(string providedPassword, string storedPassword)
        {
            System.Diagnostics.Debug.WriteLine($"=== VERIFY USER PASSWORD ===");
            System.Diagnostics.Debug.WriteLine($"Stored password: '{storedPassword}'");
            System.Diagnostics.Debug.WriteLine($"Stored password length: {storedPassword?.Length}");
            System.Diagnostics.Debug.WriteLine($"Stored password is plain text: {IsPlainTextPassword(storedPassword)}");

            // Nếu storedPassword là plain text (ngắn), so sánh trực tiếp
            if (IsPlainTextPassword(storedPassword))
            {
                System.Diagnostics.Debug.WriteLine($"Comparing plain text: '{providedPassword}' == '{storedPassword}'");
                bool result = providedPassword == storedPassword;
                System.Diagnostics.Debug.WriteLine($"Plain text comparison result: {result}");
                return result;
            }

            // Nếu đã là hash, verify bằng PasswordHasher
            try
            {
                System.Diagnostics.Debug.WriteLine($"Using PasswordHasher for verification");
                var passwordHasher = new PasswordHasher();
                var result = passwordHasher.VerifyHashedPassword(storedPassword, providedPassword);
                System.Diagnostics.Debug.WriteLine($"Password hasher result: {result}");

                return result == PasswordVerificationResult.Success;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error verifying hashed password: {ex.Message}");
                return false;
            }
        }

        // Verify Admin's password - Sửa tương tự
        private bool VerifyPasswordForAdmin(string providedPassword, string storedPassword)
        {
            System.Diagnostics.Debug.WriteLine($"=== VERIFY ADMIN PASSWORD ===");
            System.Diagnostics.Debug.WriteLine($"Stored admin password: '{storedPassword}'");
            System.Diagnostics.Debug.WriteLine($"Stored admin password length: {storedPassword?.Length}");

            // Admin luôn dùng plain text comparison
            bool result = providedPassword == storedPassword;
            System.Diagnostics.Debug.WriteLine($"Admin password comparison result: {result}");
            return result;
        }

        private bool IsPlainTextPassword(string password)
        {
            if (string.IsNullOrEmpty(password))
                return true;

            // Hash password thường dài > 30 ký tự và chứa ký tự đặc biệt
            // Plain text thường ngắn (6-20 ký tự) và chỉ chứa ký tự thông thường
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