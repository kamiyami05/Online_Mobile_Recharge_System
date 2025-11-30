using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Mail;
using System.Web;
using System.Web.Mvc;

namespace sem3.Controllers
{
    public class ContactUsController : Controller
    {
        public ActionResult Index()
        {
            return View();
        }

        [HttpPost]
        public ActionResult SendMessage(string Name, string Email, string Message)
        {
            try
            {
                var mail = new MailMessage();
                mail.From = new MailAddress("yourgmail@gmail.com"); // Gmail của bạn gửi đi
                mail.To.Add("zooxoox3@gmail.com"); // Gmail nhận
                mail.Subject = "New Contact Message";
                mail.Body = $"Name: {Name}\nEmail: {Email}\nMessage:\n{Message}";

                var smtp = new SmtpClient("smtp.gmail.com", 587)
                {
                    Credentials = new NetworkCredential("zooxoox3@gmail.com", "snlu azqk awyc rqgx"),
                    EnableSsl = true
                };

                smtp.Send(mail);

                TempData["Success"] = "Your message has been sent successfully!";
            }
            catch (Exception ex)
            {
                TempData["Error"] = ex.Message;
            }

            return RedirectToAction("Index", "ContactUs");
        }
    }
}