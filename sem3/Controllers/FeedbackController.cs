using sem3.Models.Entities;
using sem3.Models.Helpers;
using sem3.Models.ModelViews;
using System;
using System.Linq;
using System.Web.Mvc;

namespace sem3.Controllers
{
    public class FeedbackController : Controller
    {
        private readonly OnlineRechargeDBEntities _db = new OnlineRechargeDBEntities();

        public ActionResult Index()
        {
            var model = new FeedbackM();

            if (Session["CurrentUser"] != null)
            {
                var user = Session["CurrentUser"] as sem3.Models.ModelViews.UserM;
                if (user != null)
                {
                    model.Name = user.FullName;
                    model.Email = user.Email;
                }
            }

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Index(FeedbackM model)
        {
            var sessionCaptcha = Session["CaptchaCode"] as string;
            if (string.IsNullOrEmpty(model.CaptchaCode) ||
                string.IsNullOrEmpty(sessionCaptcha) ||
                !sessionCaptcha.Equals(model.CaptchaCode, StringComparison.OrdinalIgnoreCase))
            {
                ModelState.AddModelError("CaptchaCode", "Invalid CAPTCHA code. Please try again.");
                Session.Remove("CaptchaCode"); 
                return View(model);
            }

            if (ModelState.IsValid)
            {
                int? userId = Session["CurrentUserId"] as int?;

                if (!CanSubmitFeedback(model.Email, userId))
                {
                    ModelState.AddModelError("", "You can only submit one feedback per hour. Please wait before submitting another feedback.");

                    Session.Remove("CaptchaCode");
                    return View(model);
                }

                var newFeedback = new Feedback
                {
                    Name = model.Name,
                    Email = model.Email,
                    FeedbackText = model.FeedbackText,
                    SubmitDate = DateTime.Now,
                    Rating = model.Rating
                };

                if (Session["CurrentUserId"] != null)
                {
                    newFeedback.UserID = (int)Session["CurrentUserId"];
                }

                try
                {
                    _db.Feedbacks.Add(newFeedback);
                    _db.SaveChanges();

                    Session.Remove("CaptchaCode");

                    ViewBag.SuccessMessage = "Thank you! Your feedback has been submitted successfully.";
                    return View(new FeedbackM());
                }
                catch (Exception ex)
                {
                    ModelState.AddModelError("", "An error occurred: " + ex.Message);
                }
            }

            Session.Remove("CaptchaCode");
            return View(model);
        }

        private bool CanSubmitFeedback(string email, int? userId)
        {
            DateTime oneHourAgo = DateTime.Now.AddHours(-1);

            if (userId.HasValue)
            {
                return !_db.Feedbacks.Any(f => f.UserID == userId && f.SubmitDate >= oneHourAgo);
            }
            else
            {
                return !_db.Feedbacks.Any(f => f.Email == email && f.UserID == null && f.SubmitDate >= oneHourAgo);
            }
        }

        public JsonResult GetLastFeedbackTime()
        {
            try
            {
                int? userId = Session["CurrentUserId"] as int?;
                string userEmail = null;

                if (Session["CurrentUser"] != null)
                {
                    var user = Session["CurrentUser"] as sem3.Models.ModelViews.UserM;
                    userEmail = user?.Email;
                }

                DateTime oneHourAgo = DateTime.Now.AddHours(-1);
                Feedback lastFeedback = null;

                if (userId.HasValue)
                {
                    lastFeedback = _db.Feedbacks
                        .Where(f => f.UserID == userId)
                        .OrderByDescending(f => f.SubmitDate)
                        .FirstOrDefault();
                }
                else if (!string.IsNullOrEmpty(userEmail))
                {
                    lastFeedback = _db.Feedbacks
                        .Where(f => f.Email == userEmail && f.UserID == null)
                        .OrderByDescending(f => f.SubmitDate)
                        .FirstOrDefault();
                }

                if (lastFeedback != null && lastFeedback.SubmitDate >= oneHourAgo)
                {
                    var timePassed = DateTime.Now - lastFeedback.SubmitDate.Value;
                    var minutesLeft = 60 - timePassed.TotalMinutes;

                    return Json(new
                    {
                        canSubmit = false,
                        minutesLeft = minutesLeft,
                        lastSubmitTime = lastFeedback.SubmitDate.Value.ToString("g")
                    }, JsonRequestBehavior.AllowGet);
                }

                return Json(new { canSubmit = true }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return Json(new { canSubmit = true }, JsonRequestBehavior.AllowGet);
            }
        }

        public ActionResult RefreshCaptcha()
        {
            var captchaCode = CaptchaHelper.GenerateCaptchaCode();
            Session["CaptchaCode"] = captchaCode;
            var imageBytes = CaptchaHelper.GenerateCaptchaImage(captchaCode);

            return File(imageBytes, "image/png");
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