using sem3.Helpers;
using System.Web.Mvc;

namespace sem3.Controllers
{
    public class CaptchaController : Controller
    {
        public ActionResult GetCaptcha()
        {
            // Tạo CAPTCHA mới
            var captchaCode = CaptchaHelper.GenerateCaptchaCode();

            // Lưu vào Session
            Session["CaptchaCode"] = captchaCode;

            // Tạo hình ảnh
            var imageBytes = CaptchaHelper.GenerateCaptchaImage(captchaCode);

            return File(imageBytes, "image/png");
        }

        [HttpPost]
        public JsonResult ValidateCaptcha(string userInput)
        {
            var sessionCaptcha = Session["CaptchaCode"] as string;
            var isValid = !string.IsNullOrEmpty(sessionCaptcha) &&
                         sessionCaptcha.Equals(userInput, System.StringComparison.OrdinalIgnoreCase);

            return Json(new { valid = isValid });
        }
    }
}