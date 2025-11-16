using sem3.Models.ModelViews;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;

namespace sem3.Controllers
{
    public class RechargeController : Controller
    {
        public ActionResult Index()
        {
            return View();
        }

        [HttpPost]
        public ActionResult Proceed(string phone)
        {
            // Logic cũ của bạn:
            // Hàm DetectOperator sẽ được gọi trước bằng AJAX,
            // nhưng chúng ta vẫn kiểm tra lại ở đây cho chắc chắn.
            if (string.IsNullOrEmpty(phone))
            {
                ViewBag.Error = "Please enter a valid phone number.";
                return View("Index");
            }

            string op = DetectOperator(phone);
            if (op == "Unknown")
            {
                ViewBag.Error = "Unknown operator!";
                return View("Index");
            }

            TempData["Phone"] = phone;
            TempData["Operator"] = op;
            return RedirectToAction("TopUp");
        }

        public ActionResult TopUp()
        {
            string phone = TempData["Phone"] as string;
            string op = TempData["Operator"] as string;

            if (phone == null || op == null)
                return RedirectToAction("Index");

            // Chúng ta không tải 'plans' ở đây nữa.
            // View sẽ tự gọi AJAX để tải.
            ViewBag.Phone = phone;
            ViewBag.Operator = op;

            return View(); // Trả về TopUp.cshtml
        }

        [HttpPost]
        public ActionResult SubmitTopUp(int planId, string phone, string op)
        {
            // Dùng hàm GetMockPlans để tìm plan
            var plan = GetMockPlans(op) // Lấy tất cả plan
                        .FirstOrDefault(p => p.PlanID == planId);

            if (plan == null)
            {
                // Nếu không tìm thấy, quay lại trang TopUp
                // và phải truyền lại TempData
                TempData["Phone"] = phone;
                TempData["Operator"] = op;
                return RedirectToAction("TopUp");
            }

            ViewBag.Phone = phone;
            ViewBag.Operator = op;
            ViewBag.Plan = plan;

            return View("Payment");
        }

        [HttpPost]
        public ActionResult ConfirmPayment(string phone, string operatorName, string planName, decimal amount, string cardNumber)
        {
            string transactionId = Guid.NewGuid().ToString().Substring(0, 8).ToUpper();
            DateTime time = DateTime.Now;

            ViewBag.Phone = phone;
            ViewBag.Operator = operatorName;
            ViewBag.PlanName = planName;
            ViewBag.Amount = amount;
            ViewBag.TransactionId = transactionId;
            ViewBag.Time = time;

            return View("Receipt");
        }

        // -----------------------------------------------------------------
        // CÁC HÀM MỚI VÀ HÀM CŨ ĐƯỢC CẬP NHẬT
        // -----------------------------------------------------------------

        /// <summary>
        /// [MỚI] Action này được gọi bằng AJAX từ trang Index.
        /// </summary>
        [HttpPost]
        public JsonResult DetectOperatorAjax(string phone)
        {
            if (string.IsNullOrEmpty(phone) || phone.Length != 10)
            {
                return Json(new { success = false, message = "Số không hợp lệ" });
            }

            string op = DetectOperator(phone);

            if (op != "Unknown")
            {
                return Json(new { success = true, operatorName = op });
            }
            else
            {
                return Json(new { success = false, message = "Không nhận diện được nhà mạng." });
            }
        }

        /// <summary>
        /// [MỚI] Action này được gọi bằng AJAX từ trang TopUp.
        /// </summary>
        [HttpGet]
        public JsonResult GetPlans(string op, string planType)
        {
            // Lấy danh sách plan (giả lập từ DB)
            var allPlans = GetMockPlans(op);

            // Lọc theo loại plan ("Prepaid" hoặc "Data")
            // Quan trọng: Đảm bảo PlanType trong DB của bạn khớp với "Prepaid", "Data"
            var filteredPlans = allPlans
                .Where(p => p.PlanType.Equals(planType, StringComparison.OrdinalIgnoreCase) && p.IsActive)
                .ToList();

            return Json(filteredPlans, JsonRequestBehavior.AllowGet);
        }

        /// <summary>
        /// [Hàm nội bộ] Hàm logic của bạn để phát hiện nhà mạng.
        /// </summary>
        private string DetectOperator(string mobile)
        {
            // Viettel
            if (mobile.StartsWith("096") || mobile.StartsWith("097") || mobile.StartsWith("098") ||
                mobile.StartsWith("086") ||
                mobile.StartsWith("032") || mobile.StartsWith("033") || mobile.StartsWith("034") ||
                mobile.StartsWith("035") || mobile.StartsWith("036") || mobile.StartsWith("037") ||
                mobile.StartsWith("038") || mobile.StartsWith("039"))
                return "Viettel";

            // Vinaphone
            if (mobile.StartsWith("081") || mobile.StartsWith("082") || mobile.StartsWith("083") ||
                mobile.StartsWith("084") || mobile.StartsWith("085") ||
                mobile.StartsWith("088") ||
                mobile.StartsWith("091") || mobile.StartsWith("094"))
                return "Vinaphone";

            // Mobifone
            if (mobile.StartsWith("070") || mobile.StartsWith("076") || mobile.StartsWith("077") ||
                mobile.StartsWith("078") || mobile.StartsWith("079") ||
                mobile.StartsWith("089") ||
                mobile.StartsWith("090") || mobile.StartsWith("093"))
                return "Mobifone";

            // Vietnamobile
            if (mobile.StartsWith("056") || mobile.StartsWith("058"))
                return "Vietnamobile";

            // Gmobile
            if (mobile.StartsWith("059"))
                return "Gmobile";

            // Unknown
            return "Unknown";
        }


        /// <summary>
        /// [MỚI] Hàm giả lập việc lấy dữ liệu từ Database
        /// </summary>
        private List<RechargePlans> GetMockPlans(string op)
        {
            // !! Thay thế phần này bằng logic gọi DB thật
            // List<RechargePlans> plans = db.RechargePlans.Where(p => p.Operator == op).ToList();

            // Dữ liệu giả lập
            var mockPlans = new List<RechargePlans>
            {
                // Gói "Prepaid" (Nạp tiền)
                new RechargePlans { PlanID = 1, PlanType = "Prepaid", PlanName = "Nạp 50,000", Amount = 50000, TalkTimeMinutes = 50, Details = "Có 50,000đ trong tài khoản chính", IsActive = true },
                new RechargePlans { PlanID = 2, PlanType = "Prepaid", PlanName = "Nạp 100,000", Amount = 100000, TalkTimeMinutes = 100, Details = "Có 100,000đ trong tài khoản chính", IsActive = true },
                new RechargePlans { PlanID = 3, PlanType = "Prepaid", PlanName = "Nạp 200,000", Amount = 200000, TalkTimeMinutes = 200, Details = "Có 200,000đ trong tài khoản chính", IsActive = true },
                
                // Gói "Data"
                new RechargePlans { PlanID = 4, PlanType = "Data", PlanName = "Gói 2GB", Amount = 50000, DataMB = 2048, Details = "2GB / 30 ngày", IsActive = true },
                new RechargePlans { PlanID = 5, PlanType = "Data", PlanName = "Gói 5GB", Amount = 100000, DataMB = 5120, Details = "5GB / 30 ngày", IsActive = true },
                new RechargePlans { PlanID = 6, PlanType = "Data", PlanName = "Gói 12GB", Amount = 200000, DataMB = 12288, Details = "12GB / 30 ngày", IsActive = true },

                // Gói của nhà mạng khác (ví dụ)
                new RechargePlans { PlanID = 7, PlanType = "Data", PlanName = "Gói Viettel 10GB", Amount = 120000, DataMB = 10240, Details = "10GB / 30 ngày", IsActive = true }
            };

            // Lọc theo nhà mạng (logic giả)
            if (op == "Viettel")
            {
                return mockPlans.Where(p => p.PlanID == 6 || p.PlanID == 7).ToList();
            }

            // Trả về các gói khác cho Mobifone/Vinaphone
            return mockPlans.Take(6).ToList();
        }
    }
}