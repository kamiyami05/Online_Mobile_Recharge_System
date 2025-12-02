using sem3.Models;
using sem3.Models.Entities;
using System;
using System.Data.Entity;
using System.Linq;
using System.Web.Mvc;

namespace sem3.Controllers
{
    public class PostpaidController : Controller
    {
        private OnlineRechargeDBEntities db = new OnlineRechargeDBEntities();


        public ActionResult Index()
        {
            if (Session["CurrentUser"] == null)
                return RedirectToAction("Login", "Login");

            string mobile = Session["MobileNumber"].ToString();

            var bills = db.PostpaidBills
                          .Where(p => p.MobileNumber == mobile)
                          .OrderByDescending(p => p.BillingCycle)
                          .ToList();

            return View(bills);
        }


        public ActionResult Pay(int id)
        {
            if (Session["CurrentUser"] == null)
                return RedirectToAction("Login", "Login");

            string mobile = Session["MobileNumber"].ToString();

            var bill = db.PostpaidBills.FirstOrDefault(x => x.BillID == id && x.MobileNumber == mobile);

            if (bill == null)
                return HttpNotFound("Bill not found or you do not have permission.");

            return View(bill);
        }

        [HttpPost]
        public ActionResult PayPost(int billId)
        {
            if (Session["CurrentUser"] == null)
                return RedirectToAction("Login", "Login");


            string mobile = Session["MobileNumber"].ToString();

            var bill = db.PostpaidBills.FirstOrDefault(x => x.BillID == billId && x.MobileNumber == mobile);

            if (bill == null)
                return HttpNotFound("Bill not found or you do not have permission.");


            if ((bool)bill.IsPaid)
                return Content("This bill was already paid.");


            var user = db.Users.FirstOrDefault(u => u.MobileNumber == mobile);
            if (user == null)
                return HttpNotFound("User not found.");

            var trans = new Transaction
            {
                UserID = user.UserID,
                MobileNumber = user.MobileNumber,
                Amount = bill.TotalAmount,
                TransactionType = "POSTPAID_PAYMENT",
                Status = "Success",
                TransactionDate = DateTime.Now
            };

            db.Transactions.Add(trans);
            db.SaveChanges();

            bill.IsPaid = true;
            bill.PaymentTransactionID = trans.TransactionID;
            db.SaveChanges();

            string script = $"POSTPAID BILL PAYMENT\n" +
                            $"Mobile: {bill.MobileNumber}\n" +
                            $"Billing cycle: {bill.BillingCycle:dd/MM/yyyy}\n" +
                            $"Paid at: {DateTime.Now:dd/MM/yyyy HH:mm}\n" +
                            $"Amount: {bill.TotalAmount} VND\n" +
                            $"Transaction: {trans.TransactionID}\n";

            db.TransactionScripts.Add(new TransactionScript
            {
                TransactionID = trans.TransactionID,
                ScriptContent = script
            });

            db.SaveChanges();


            if (!string.IsNullOrEmpty(user.Email))
            {
                EmailHelper.SendBillPaidEmail(user.Email, bill.MobileNumber, bill.TotalAmount);
            }


            return RedirectToAction("Receipt", new { id = trans.TransactionID });
        }

        public ActionResult Receipt(int id)
        {
            var script = db.TransactionScripts.FirstOrDefault(x => x.TransactionID == id);

            if (script == null)
                return HttpNotFound();

            return View(script);
        }

        public ActionResult Print(int id)
        {
            return new Rotativa.ActionAsPdf("Receipt", new { id = id });
        }

        public ActionResult History()
        {
            if (Session["MobileNumber"] == null)
                return RedirectToAction("Login", "Account");

            string mobile = Session["MobileNumber"].ToString();

            var history = db.Transactions
                            .Where(t => t.MobileNumber == mobile && t.TransactionType == "POSTPAID_PAYMENT")
                            .OrderByDescending(t => t.TransactionDate)
                            .ToList();

            return View(history);
        }
    }
}
