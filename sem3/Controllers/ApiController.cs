using Newtonsoft.Json;
using sem3.Models.ModelViews;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using System.Web.Mvc;

namespace sem3.Controllers
{
    public class ApiController : Controller
    {
        private static readonly HttpClient client = new HttpClient();

        [HttpPost]
        public async Task<ActionResult> Chat(ChatRequestModel request)
        {
            // TẠM THỜI: Luôn trả về response local để test chatbox
            string userMessage = request.Messages?.LastOrDefault()?.Content?.ToLower() ?? "";

            // Simulate API delay
            await Task.Delay(500);

            string response = GetRuleBasedResponse(userMessage);

            return Json(new { reply = response }, JsonRequestBehavior.AllowGet);
        }

        private string GetRuleBasedResponse(string userMessage)
        {
            if (string.IsNullOrEmpty(userMessage))
                return "Hello! How can I help with your mobile recharge or bill payment today?";

            if (userMessage.Contains("recharge") || userMessage.Contains("top up") || userMessage.Contains("top-up"))
            {
                return "To recharge your phone: 1) Go to 'Online Recharge' section 2) Select your operator 3) Enter mobile number 4) Choose a plan 5) Complete payment. We support all major operators!";
            }
            else if (userMessage.Contains("bill") || userMessage.Contains("payment"))
            {
                return "For bill payments, visit 'Post Bill Payment' section. You can pay electricity, water, gas bills. Enter your customer ID and proceed with secure payment.";
            }
            else if (userMessage.Contains("hello") || userMessage.Contains("hi") || userMessage.Contains("hey"))
            {
                return "Hello! Welcome to Recharge System. I can help with mobile recharges, bill payments, and account support. What do you need help with?";
            }
            else if (userMessage.Contains("thank"))
            {
                return "You're welcome! Let me know if you need anything else regarding recharges or bill payments.";
            }
            else if (userMessage.Contains("support") || userMessage.Contains("help") || userMessage.Contains("how"))
            {
                return "I can help you with: 📱 Mobile recharges 💡 Bill payments 💳 Payment methods 🔍 Transaction status. What do you need assistance with?";
            }
            else if (userMessage.Contains("operator") || userMessage.Contains("carrier"))
            {
                return "We support all major operators: Viettel, Mobifone, Vinaphone, and more. You can recharge any prepaid mobile number!";
            }
            else if (userMessage.Contains("plan") || userMessage.Contains("package"))
            {
                return "We offer various recharge plans from $1 to $50 with different data and call benefits. Check the 'Online Recharge' section for all available plans!";
            }
            else
            {
                return "I specialize in mobile recharge and bill payment services. You can ask me about: 📱 How to recharge your phone 💡 Bill payment options 🏪 Supported operators 💳 Payment methods. How can I assist you today?";
            }
        }
    }
}