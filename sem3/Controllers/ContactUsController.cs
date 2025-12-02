using sem3.Models.Helpers;
using System;
using System.Net;
using System.Net.Mail;
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
        [ValidateAntiForgeryToken]
        public ActionResult SendMessage(string Name, string Email, string Message, string CaptchaCode)
        {
            // Validate CAPTCHA
            var sessionCaptcha = Session["CaptchaCode"] as string;

            if (string.IsNullOrEmpty(CaptchaCode) ||
                string.IsNullOrEmpty(sessionCaptcha) ||
                !sessionCaptcha.Equals(CaptchaCode, StringComparison.OrdinalIgnoreCase))
            {
                TempData["Error"] = "Invalid CAPTCHA code. Please try again.";
                Session.Remove("CaptchaCode");
                return RedirectToAction("Index");
            }

            try
            {
                // 1. Email gửi cho công ty (Internal)
                SendInternalNotification(Name, Email, Message);

                // 2. Email xác nhận cho khách hàng (Auto-reply)
                SendCustomerConfirmation(Name, Email, Message);

                // Xóa CAPTCHA sau khi gửi thành công
                Session.Remove("CaptchaCode");

                TempData["Success"] = "Thank you for contacting us! We have received your message and will respond within 24 hours. A confirmation email has been sent to your inbox.";
            }
            catch (SmtpException smtpEx)
            {
                TempData["Error"] = "We encountered an issue sending your message. Please try again later or contact us directly at zooxoox3@gmail.com";
                Session.Remove("CaptchaCode");
            }
            catch (Exception ex)
            {
                TempData["Error"] = "An unexpected error occurred. Please try again or call our support hotline: +1 (555) 123-4567";
                Session.Remove("CaptchaCode");
            }

            return RedirectToAction("Index");
        }

        private void SendInternalNotification(string name, string email, string message)
        {
            var mail = new MailMessage();
            mail.From = new MailAddress("zooxoox3@gmail.com", "Recharge System Demo");
            mail.To.Add("zooxoox3@gmail.com");
            mail.Subject = $"📩 New Contact Message from {name} (DEMO)";
            mail.IsBodyHtml = true;

            // HTML Email Template for Internal Team
            mail.Body = $@"
            <!DOCTYPE html>
            <html lang='en'>
            <head>
                <meta charset='UTF-8'>
                <meta name='viewport' content='width=device-width, initial-scale=1.0'>
                <style>
                    body {{ font-family: 'Segoe UI', Arial, sans-serif; line-height: 1.6; color: #333; }}
                    .container {{ max-width: 600px; margin: 0 auto; padding: 20px; }}
                    .header {{ background: linear-gradient(135deg, #667eea 0%, #764ba2 100%); color: white; padding: 30px; text-align: center; border-radius: 10px 10px 0 0; }}
                    .content {{ background: #f9f9f9; padding: 30px; border-radius: 0 0 10px 10px; border: 1px solid #eaeaea; }}
                    .info-box {{ background: white; border-left: 4px solid #667eea; padding: 15px; margin: 20px 0; }}
                    .label {{ font-weight: bold; color: #667eea; }}
                    .message-box {{ background: #fff8e1; border: 1px solid #ffd54f; padding: 20px; border-radius: 8px; margin: 20px 0; }}
                    .footer {{ margin-top: 30px; padding-top: 20px; border-top: 1px solid #eee; text-align: center; color: #666; font-size: 12px; }}
                    .priority {{ display: inline-block; background: #ff6b6b; color: white; padding: 5px 15px; border-radius: 20px; font-size: 12px; }}
                    .response-time {{ background: #e3f2fd; padding: 10px; border-radius: 5px; margin: 15px 0; }}
                    .demo-banner {{ background: #ff9800; color: white; padding: 10px; text-align: center; font-weight: bold; border-radius: 5px; margin-bottom: 20px; }}
                </style>
            </head>
            <body>
                <div class='container'>
                    <div class='demo-banner'>🚀 DEMO SYSTEM - TEST EMAIL</div>
                    
                    <div class='header'>
                        <h1>📬 New Customer Message</h1>
                        <p>Recharge System Demo - Customer Support Portal</p>
                    </div>
                    
                    <div class='content'>
                        <div class='priority'>⏰ RESPONSE REQUIRED: Within 24 hours</div>
                        
                        <div class='info-box'>
                            <h3 style='color: #667eea; margin-top: 0;'>Customer Information</h3>
                            <p><span class='label'>👤 Name:</span> {name}</p>
                            <p><span class='label'>📧 Email:</span> <a href='mailto:{email}'>{email}</a></p>
                            <p><span class='label'>🕒 Received:</span> {DateTime.Now.ToString("MMMM dd, yyyy HH:mm")}</p>
                            <p><span class='label'>🔗 Source:</span> Website Contact Form (Demo)</p>
                        </div>

                        <div class='message-box'>
                            <h4 style='color: #d84315; margin-top: 0;'>📝 Customer Message:</h4>
                            <p style='white-space: pre-line;'>{message}</p>
                        </div>

                        <div class='response-time'>
                            <h4 style='color: #1565c0;'>⏱️ Action Required</h4>
                            <p><strong>Response Timeline:</strong> Please respond within 24 hours</p>
                            <p><strong>Assigned To:</strong> Customer Support Team (Demo)</p>
                            <p><strong>Ticket ID:</strong> RS-DEMO-{DateTime.Now:yyyyMMddHHmm}</p>
                        </div>

                        <div style='margin-top: 25px;'>
                            <a href='mailto:{email}' style='background: #667eea; color: white; padding: 12px 25px; text-decoration: none; border-radius: 5px; display: inline-block;'>
                                ✉️ Reply to Customer
                            </a>
                        </div>
                    </div>

                    <div class='footer'>
                        <p><strong>⚠️ DEMO SYSTEM NOTIFICATION</strong></p>
                        <p>This is a test email from Recharge System Demo Application</p>
                        <p>All emails are sent to: zooxoox3@gmail.com</p>
                        <p>© {DateTime.Now.Year} Recharge System Demo. For demonstration purposes only.</p>
                    </div>
                </div>
            </body>
            </html>
            ";

            var smtp = GetSmtpClient();
            smtp.Send(mail);
        }

        private void SendCustomerConfirmation(string name, string email, string message)
        {
            var mail = new MailMessage();
            mail.From = new MailAddress("zooxoox3@gmail.com", "Recharge System Demo");
            mail.To.Add(email);
            mail.ReplyToList.Add("zooxoox3@gmail.com");
            mail.Subject = "✅ Your Message Has Been Received - Recharge System (Demo)";
            mail.IsBodyHtml = true;

            // HTML Email Template for Customer
            mail.Body = $@"
            <!DOCTYPE html>
            <html lang='en'>
            <head>
                <meta charset='UTF-8'>
                <meta name='viewport' content='width=device-width, initial-scale=1.0'>
                <style>
                    body {{ font-family: 'Segoe UI', Arial, sans-serif; line-height: 1.6; color: #333; margin: 0; padding: 0; }}
                    .container {{ max-width: 600px; margin: 0 auto; background: #ffffff; }}
                    .header {{ background: linear-gradient(135deg, #667eea 0%, #764ba2 100%); padding: 40px 30px; text-align: center; color: white; }}
                    .logo {{ font-size: 32px; font-weight: bold; margin-bottom: 10px; }}
                    .tagline {{ font-size: 16px; opacity: 0.9; }}
                    .content {{ padding: 40px 30px; }}
                    .greeting {{ font-size: 24px; color: #2c3e50; margin-bottom: 30px; }}
                    .confirmation-box {{ background: #f8f9fa; border-radius: 10px; padding: 25px; margin: 25px 0; border-left: 4px solid #28a745; }}
                    .message-summary {{ background: #e8f4fd; border-radius: 8px; padding: 20px; margin: 20px 0; }}
                    .cta-button {{ display: inline-block; background: linear-gradient(135deg, #667eea 0%, #764ba2 100%); color: white; padding: 15px 30px; text-decoration: none; border-radius: 50px; font-weight: bold; margin: 20px 0; }}
                    .next-steps {{ margin-top: 40px; }}
                    .step {{ display: flex; align-items: flex-start; margin: 15px 0; }}
                    .step-number {{ background: #667eea; color: white; width: 30px; height: 30px; border-radius: 50%; display: flex; align-items: center; justify-content: center; margin-right: 15px; flex-shrink: 0; }}
                    .support-section {{ background: #f1f8ff; border-radius: 10px; padding: 25px; margin-top: 30px; }}
                    .footer {{ background: #2c3e50; color: white; padding: 30px; text-align: center; font-size: 14px; }}
                    .social-links {{ margin: 20px 0; }}
                    .social-icon {{ display: inline-block; margin: 0 10px; color: white; text-decoration: none; }}
                    .demo-note {{ background: #fff3cd; border: 1px solid #ffeaa7; padding: 15px; border-radius: 5px; margin: 20px 0; color: #856404; }}
                </style>
            </head>
            <body>
                <div class='container'>
                    <!-- Demo Note -->
                    <div class='demo-note' style='text-align: center;'>
                        <strong>⚠️ DEMONSTRATION EMAIL</strong><br>
                        This is a test email from Recharge System Demo Application
                    </div>

                    <!-- Header -->
                    <div class='header'>
                        <div class='logo'>⚡ RECHARGE SYSTEM</div>
                        <div class='tagline'>Demo Application - Mobile Recharge Platform</div>
                    </div>

                    <!-- Main Content -->
                    <div class='content'>
                        <h1 class='greeting'>Dear {name},</h1>
                        
                        <div class='confirmation-box'>
                            <h2 style='color: #28a745; margin-top: 0;'>✅ Message Successfully Received!</h2>
                            <p>Thank you for reaching out to Recharge System Demo. We have received your message and our team is reviewing it.</p>
                            <p><strong>Your Reference ID:</strong> <code style='background: #2c3e50; color: white; padding: 5px 10px; border-radius: 4px;'>RS-DEMO-{DateTime.Now:yyyyMMddHHmm}</code></p>
                        </div>

                        <div class='message-summary'>
                            <h3 style='color: #667eea;'>📋 Message Summary:</h3>
                            <p><strong>Date & Time:</strong> {DateTime.Now.ToString("dddd, MMMM dd, yyyy 'at' HH:mm")}</p>
                            <p><strong>Email Address:</strong> {email}</p>
                            <div style='margin-top: 15px;'>
                                <strong>Your Message Preview:</strong>
                                <blockquote style='border-left: 3px solid #667eea; padding-left: 15px; margin-left: 0; color: #555;'>
                                    {(message.Length > 200 ? message.Substring(0, 200) + "..." : message)}
                                </blockquote>
                            </div>
                        </div>

                        <!-- What Happens Next -->
                        <div class='next-steps'>
                            <h2 style='color: #2c3e50;'>📅 What Happens Next? (Demo Flow)</h2>
                            
                            <div class='step'>
                                <div class='step-number'>1</div>
                                <div>
                                    <strong>Immediate Acknowledgement</strong>
                                    <p>You have received this automatic confirmation email</p>
                                </div>
                            </div>
                            
                            <div class='step'>
                                <div class='step-number'>2</div>
                                <div>
                                    <strong>Simulated Processing</strong>
                                    <p>In a real system, your inquiry would be assigned to our team</p>
                                </div>
                            </div>
                            
                            <div class='step'>
                                <div class='step-number'>3</div>
                                <div>
                                    <strong>Demo Response Time</strong>
                                    <p>This is a demonstration - no actual response will be sent</p>
                                </div>
                            </div>
                        </div>

                        <!-- Support Section -->
                        <div class='support-section'>
                            <h3 style='color: #d35400;'>🚀 System Demonstration</h3>
                            <p>This email demonstrates how our customer communication system works:</p>
                            <ul>
                                <li><strong>📧 Email Template:</strong> Professional HTML design</li>
                                <li><strong>🔐 CAPTCHA Protection:</strong> Form submission security</li>
                                <li><strong>🔄 Auto-Reply:</strong> Immediate customer confirmation</li>
                                <li><strong>🎯 Ticket Tracking:</strong> Reference ID generation</li>
                            </ul>
                        </div>

                        <!-- CTA Button -->
                        <div style='text-align: center; margin: 40px 0;'>
                            <p><em>In a live system, you would click below to explore our services</em></p>
                            <a href='#' class='cta-button'>Explore Services (Demo) →</a>
                        </div>
                    </div>

                    <!-- Footer -->
                    <div class='footer'>
                        <p><strong>⚡ Recharge System - DEMO APPLICATION</strong><br>
                        For Educational and Demonstration Purposes Only</p>
                        
                        <div style='margin: 20px 0;'>
                            <p>📍 Demo Address: 123 Demo Street, Virtual City</p>
                            <p>📞 Demo Phone: +1 (555) 123-4567</p>
                            <p>📧 All Emails: zooxoox3@gmail.com</p>
                        </div>
                        
                        <div class='social-links'>
                            <p>Demo Social Links:</p>
                            <a href='#' class='social-icon'>📘 Facebook</a>
                            <a href='#' class='social-icon'>🐦 Twitter</a>
                            <a href='#' class='social-icon'>💼 LinkedIn</a>
                            <a href='#' class='social-icon'>📸 Instagram</a>
                        </div>
                        
                        <div style='margin-top: 25px; padding-top: 20px; border-top: 1px solid rgba(255,255,255,0.1);'>
                            <p style='font-size: 12px; opacity: 0.8;'>
                                <strong>DEMO SYSTEM DISCLAIMER:</strong><br>
                                This is a demonstration application. No real services are provided.<br>
                                All emails are sent to zooxoox3@gmail.com for testing purposes.<br>
                                © {DateTime.Now.Year} Recharge System Demo.
                            </p>
                        </div>
                    </div>
                </div>
            </body>
            </html>
            ";

            var smtp = GetSmtpClient();
            smtp.Send(mail);
        }

        private SmtpClient GetSmtpClient()
        {
            return new SmtpClient("smtp.gmail.com", 587)
            {
                Credentials = new NetworkCredential("zooxoox3@gmail.com", "snlu azqk awyc rqgx"),
                EnableSsl = true,
                DeliveryMethod = SmtpDeliveryMethod.Network,
                Timeout = 30000
            };
        }

        // CAPTCHA methods
        public ActionResult GetCaptcha()
        {
            var captchaCode = CaptchaHelper.GenerateCaptchaCode();
            Session["CaptchaCode"] = captchaCode;
            var imageBytes = CaptchaHelper.GenerateCaptchaImage(captchaCode);
            return File(imageBytes, "image/png");
        }

        public ActionResult RefreshCaptcha()
        {
            var captchaCode = CaptchaHelper.GenerateCaptchaCode();
            Session["CaptchaCode"] = captchaCode;
            var imageBytes = CaptchaHelper.GenerateCaptchaImage(captchaCode);
            return File(imageBytes, "image/png");
        }

        [HttpPost]
        public JsonResult ValidateCaptcha(string captchaInput)
        {
            var sessionCaptcha = Session["CaptchaCode"] as string;
            var isValid = !string.IsNullOrEmpty(sessionCaptcha) &&
                         sessionCaptcha.Equals(captchaInput, StringComparison.OrdinalIgnoreCase);
            return Json(new { valid = isValid });
        }
    }
}