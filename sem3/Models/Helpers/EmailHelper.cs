using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Mail;
using System.Web;

namespace sem3.Models.Helpers
{
    public class EmailHelper
    {
        public static void SendBillPaidEmail(string email, string mobile, decimal amount)
        {
            try
            {
                MailMessage mail = new MailMessage("youremail@gmail.com", email);
                mail.Subject = "Xác nhận thanh toán hóa đơn trả sau";
                mail.Body = $"Số thuê bao {mobile}\n" +
                            $"Số tiền: {amount} VNĐ\n" +
                            $"Thanh toán thành công!";

                SmtpClient client = new SmtpClient("smtp.gmail.com", 587);
                client.Credentials = new System.Net.NetworkCredential("youremail@gmail.com", "your_app_password");
                client.EnableSsl = true;
                client.Send(mail);
            }
            catch { /* Bỏ lỗi nếu demo */ }
        }
    }
}