using sem3.Models.Entities;
using sem3.Models.ModelViews;
using sem3.Services.Payment;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Web.Mvc;

namespace sem3.Controllers
{
    public class RechargeController : Controller
    {
        private OnlineRechargeDBEntities db = new OnlineRechargeDBEntities();

        public ActionResult Index()
        {
            return View();
        }

        [HttpPost]
        public ActionResult Proceed(string phone)
        {

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
            // Dùng hàm GetPlansFromDb để tìm plan
            var plan = GetPlansFromDb(op) // Lấy tất cả plan
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
        public JsonResult DetectOperatorAjax(string phone)
        {
            if (string.IsNullOrEmpty(phone) || phone.Length != 10)
            {
                return Json(new { success = false, message = "Invalid Number" });
            }

            string op = DetectOperator(phone);

            if (op != "Unknown")
            {
                return Json(new { success = true, operatorName = op });
            }
            else
            {
                return Json(new { success = false, message = "Cannot detect mobile operator." });
            }
        }

        [HttpGet]
        public JsonResult GetPlans(string op, string planType)
        {
            try
            {
                // 1. Chuẩn hóa dữ liệu đầu vào (Cắt khoảng trắng thừa, xử lý null)
                string searchOp = (op ?? "").Trim();
                string searchType = (planType ?? "").Trim();

                // 2. Truy vấn Database
                // Dùng .AsEnumerable() để xử lý chuỗi an toàn trên RAM (Tránh lỗi SQL khi dùng Trim)
                var query = db.RechargePlans.AsEnumerable()
                    .Where(p =>
                        // So sánh Nhà mạng (Bỏ qua hoa thường, bỏ qua khoảng trắng)
                        p.Operator != null && p.Operator.Trim().Equals(searchOp, StringComparison.OrdinalIgnoreCase) &&

                        // So sánh Loại gói (Prepaid/Data)
                        p.PlanType != null && p.PlanType.Trim().Equals(searchType, StringComparison.OrdinalIgnoreCase) &&

                        // Chỉ lấy gói đang kích hoạt (Active)
                        (p.IsActive == true)
                    );

                // 3. Chọn lọc dữ liệu cần thiết để trả về (Tránh lỗi 500 Circular Reference)
                var result = query.Select(p => new
                {
                    PlanID = p.PlanID,
                    PlanName = p.PlanType,
                    Amount = p.Amount,
                    Details = p.Details,
                    DataMB = p.DataMB,
                    TalkTimeMinutes = p.TalkTimeMinutes
                }).ToList();

                // Trả về JSON cho Web hiển thị
                return Json(result, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                // Ghi log lỗi nếu cần, trả về thông báo lỗi nhẹ nhàng
                return Json(new { success = false, message = "Lỗi tải dữ liệu: " + ex.Message }, JsonRequestBehavior.AllowGet);
            }
        }

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
                return "VinaPhone";

            // Mobifone
            if (mobile.StartsWith("070") || mobile.StartsWith("076") || mobile.StartsWith("077") ||
                mobile.StartsWith("078") || mobile.StartsWith("079") ||
                mobile.StartsWith("089") ||
                mobile.StartsWith("090") || mobile.StartsWith("093"))
                return "MobiFone";

            // Vietnamobile
            if (mobile.StartsWith("056") || mobile.StartsWith("058"))
                return "Vietnamobile";

            // Gmobile
            if (mobile.StartsWith("059"))
                return "Gmobile";

            // Unknown
            return "Unknown";
        }

        private List<sem3.Models.Entities.RechargePlan> GetPlansFromDb(string op)
        {
            // db.RechargePlans trả về Entity, nên hàm phải trả về Entity
            return db.RechargePlans
                     .Where(p => p.Operator == op && p.IsActive == true)
                     .ToList();
        }

        [HttpPost]
        public ActionResult ConfirmPayment(string method, string phone, string operatorName,
                                   string planName, decimal amount, string cardNumber,
                                   int? planId, int? loggedInUserId = null)
        {
            int? userId = loggedInUserId ?? (Session["CurrentUserId"] != null ? (int?)Session["CurrentUserId"] : null);

            // --- A. VALIDATE ---
            if (method == "Visa")
            {
                // 1. Làm sạch số thẻ (Xóa khoảng trắng nếu có)
                string cleanCardNumber = cardNumber != null ? cardNumber.Replace(" ", "") : "";

                // Kiểm tra độ dài cơ bản
                if (string.IsNullOrEmpty(cleanCardNumber) || cleanCardNumber.Length != 16)
                {
                    TempData["Error"] = "Số thẻ không hợp lệ! Vui lòng nhập đủ 16 số.";
                    return RedirectToAction("PaymentFailed");
                }


                if (cleanCardNumber == "4000000000000000")
                {
                    TempData["Error"] = "Số dư tài khoản không đủ.";
                    return RedirectToAction("PaymentFailed");
                }

                // Thẻ Thành công: 4222 2222 2222 2222 -> Cho qua
                // (Hoặc bất kỳ thẻ nào bắt đầu bằng 4 nhưng KHÔNG PHẢI thẻ lỗi ở trên)
                if (!cleanCardNumber.StartsWith("4"))
                {
                    TempData["Error"] = "Thẻ Visa phải bắt đầu bằng số 4.";
                    return RedirectToAction("PaymentFailed");
                }

                // Nếu là thẻ 4222... hoặc thẻ Visa hợp lệ khác -> Đi tiếp xuống phần Lưu DB
            }

            // --- B. LƯU DATABASE (Giữ nguyên code cũ của bạn) ---
            try
            {
                var transaction = new sem3.Models.Entities.Transaction
                {
                    MobileNumber = phone,
                    UserID = userId,
                    TransactionType = "Recharge",
                    PlanID = planId ?? 0,
                    Amount = amount,
                    TransactionDate = DateTime.Now,
                    Status = "Success"
                };

                db.Transactions.Add(transaction);
                db.SaveChanges();

                var paymentDetail = new sem3.Models.Entities.PaymentDetail
                {
                    TransactionID = transaction.TransactionID,
                    PaymentMethod = method,
                    ReferenceNumber = "TXN" + DateTime.Now.Ticks.ToString().Substring(10)
                };

                db.PaymentDetails.Add(paymentDetail);
                db.SaveChanges();

                return RedirectToAction("PaymentSuccess", new
                {
                    phone = phone,
                    op = operatorName,
                    plan = planName,
                    amount = amount,
                    tx = paymentDetail.ReferenceNumber
                });
            }
            catch (Exception ex)
            {
                TempData["Error"] = "Lỗi hệ thống: " + ex.Message;
                return RedirectToAction("PaymentFailed");
            }
        }
        public ActionResult LoadPaymentForm(string method, string phone, string operatorName, string planName, string amount, int planId)
        {
            decimal amountValue = 0m;
            if (!string.IsNullOrEmpty(amount))
            {
                decimal.TryParse(amount, NumberStyles.Any, System.Globalization.CultureInfo.InvariantCulture, out amountValue);
            }

            ViewBag.Method = method ?? "";
            ViewBag.Phone = phone ?? "";
            ViewBag.Operator = operatorName ?? "";
            ViewBag.PlanName = planName ?? "";
            ViewBag.Amount = amountValue;
            ViewBag.PlanId = planId;

            // SỬA Ở ĐÂY: Lấy từ Session["CurrentUserId"] thay vì Session["UserID"]
            ViewBag.CurrentUserID = Session["CurrentUserId"] != null ? (int?)Session["CurrentUserId"] : null;

            return PartialView("~/Views/Recharge/PaymentForm.cshtml");
        }

        [HttpGet]
        public ActionResult PaymentSuccess(string phone, string op, string plan, decimal amount, string tx)
        {
            // Nhận dữ liệu từ URL và đưa vào ViewBag để hiển thị
            ViewBag.Phone = phone;
            ViewBag.Operator = op;
            ViewBag.PlanName = plan;
            ViewBag.Amount = amount;
            ViewBag.TransactionId = tx;

            return View();
        }

        [HttpGet]
        public ActionResult PaymentFailed()
        {

            ViewBag.ErrorMessage = TempData["Error"] ?? "Payment Failed.";

            return View();
        }
    }
}