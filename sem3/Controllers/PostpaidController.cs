using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using sem3.Models;
using sem3.Models.Entities;


namespace sem3.Controllers
{
    public class PostpaidController : Controller
    {
        private OnlineRechargeDBEntities db = new OnlineRechargeDBEntities();

        // 📌 1) Danh sách hóa đơn theo số điện thoại
        public ActionResult Index(string mobile)
        {
            if (mobile == null)
                return View("Search");

            var bills = db.PostpaidBills.Where(p => p.MobileNumber == mobile).ToList();
            return View(bills);
        }

        // 📌 2) Trang thanh toán
        public ActionResult Pay(int id)
        {
            var bill = db.PostpaidBills.FirstOrDefault(x => x.BillID == id);
            if (bill == null) return HttpNotFound();
            return View(bill);
        }

        // 📌 3) Xử lý thanh toán bằng ví
        [HttpPost]
        public ActionResult PayPost(int billId)
        {
            var bill = db.PostpaidBills.FirstOrDefault(x => x.BillID == billId);
            if (bill == null || (bill.IsPaid.HasValue && bill.IsPaid.Value)) return HttpNotFound();

            var user = db.Users.FirstOrDefault(u => u.MobileNumber == bill.MobileNumber);

            // Tạo Transaction
            var trans = new Transaction
            {
                UserID = user.UserID,
                MobileNumber = user.MobileNumber,
                Amount = bill.TotalAmount,
                TransactionType = "POSTPAID_PAYMENT",
                Status = "SUCCESS",
                TransactionDate = DateTime.Now
            };
            db.Transactions.Add(trans);
            db.SaveChanges();

            // Gán vào hóa đơn
            bill.IsPaid = true;
            bill.PaymentTransactionID = trans.TransactionID;
            db.SaveChanges();

            // Ghi Payment Details
            db.PaymentDetails.Add(new PaymentDetail
            {
                TransactionID = trans.TransactionID,
                PaymentMethod = "WALLET",
                ReferenceNumber = Guid.NewGuid().ToString()
            });

            // 🧾 Tạo nội dung biên lai lưu DB
            string script = $"POSTPAID BILL PAYMENT\n" +
                            $"Phone number: {bill.MobileNumber}\n" +
                            $"Payment date: {DateTime.Now}\n" +
                            $"Amount due: {bill.TotalAmount} VND\n" +
                            $"Transaction ID: {trans.TransactionID}\n";

            db.TransactionScripts.Add(new TransactionScript
            {
                TransactionID = trans.TransactionID,
                ScriptContent = script
            });

            db.SaveChanges();

            // Gửi email
            EmailHelper.SendBillPaidEmail(user.Email, bill.MobileNumber, bill.TotalAmount);

            return RedirectToAction("Receipt", new { id = trans.TransactionID });
        }

        // 📌 4) Trang xem biên lai
        public ActionResult Receipt(int id)
        {
            var script = db.TransactionScripts.FirstOrDefault(x => x.TransactionID == id);

            if (script == null)
                return HttpNotFound("Receipt not found.");

            return View(script);
        }

        public ActionResult Print(int id)
        {
            return new Rotativa.ActionAsPdf("Receipt", new { id = id });
        }

        // 📌 Payment History (Postpaid payments only)
        public ActionResult History(string mobile)
        {
            if (string.IsNullOrEmpty(mobile))
                return View("SearchHistory");

            var history = db.Transactions
                            .Where(t => t.MobileNumber == mobile && t.TransactionType == "POSTPAID_PAYMENT")
                            .OrderByDescending(t => t.TransactionDate)
                            .ToList();

            return View(history);
        }

    }
}