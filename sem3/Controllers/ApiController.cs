using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using System.Web.Mvc;
using Newtonsoft.Json;
using sem3.Models.ModelViews;

namespace sem3.Controllers
{
    public class ApiController : Controller
    {
        private static readonly HttpClient client = new HttpClient();

        [HttpPost]
        public async Task<ActionResult> Chat(ChatRequestModel request)
        {
            try
            {
                string apiKey = Environment.GetEnvironmentVariable("AI_SERVICE_API_KEY");

                if (string.IsNullOrEmpty(apiKey))
                {
                    return Json(new { reply = "Error: AI_SERVICE_API_KEY not found." }, JsonRequestBehavior.AllowGet);
                }

                string apiUrl = "https://gpt1.shupremium.com/v1/chat/completions";

                client.DefaultRequestHeaders.Clear();
                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", apiKey);

                var systemPromptContent = @"
                    You are the 'Recharge System Assistant'. Your role is to guide users on how to use the website features.
                    
                    --- WEBSITE USER MANUAL ---
                    
                    1. HOW TO RECHARGE MOBILE:
                       - Go to the 'Online Recharge' menu.
                       - Enter your phone number and select your operator (Viettel, Vinaphone, etc.).
                       - Choose a data or prepaid plan.
                       - Proceed to payment.

                    2. HOW TO PAY BILLS (Postpaid):
                       - Click on 'Post Bill Payment' in the navigation bar.
                       - Enter the contract number or phone number.
                       - The system will show the amount due. Click Pay.

                    3. HOW TO CHANGE PASSWORD:
                       - You must be logged in.
                       - Click on your Name (Profile) in the top-right corner.
                       - Select 'Edit Profile' or 'Change Password'.
                       - Enter your old password and the new one.

                    4. HOW TO CHECK TRANSACTION HISTORY:
                       - Log in to your account.
                       - Go to your Profile.
                       - Scroll down to the 'Transaction History' section to see past payments.

                    5. LOGIN / REGISTER issues:
                       - If you don't have an account, click 'Login' then 'Register' to create one.
                       - If you forgot your password, please contact Admin via the Contact Us page.

                    6. CONTACT SUPPORT:
                       - You can visit the 'Contact Us' page for email and phone info.
                       - Or go to 'Customer Care' for FAQs.

                    --- RULES ---
                    - Keep answers short, polite, and instructional.
                    - Do NOT ask for the user's password or credit card info.
                    - If the user asks about something not in the manual (like specific database data), say you cannot access that information but you can guide them where to check it.
                ";

                var systemPrompt = new ApiMessage
                {
                    Role = "system",
                    Content = systemPromptContent
                };

                var combinedMessages = new List<ApiMessage> { systemPrompt };
                combinedMessages.AddRange(request.Messages);

                var apiPayload = new
                {
                    model = "gpt-4o-mini",
                    messages = combinedMessages,
                    temperature = 0.3,
                    max_tokens = 300
                };

                string jsonPayload = JsonConvert.SerializeObject(apiPayload);
                var content = new StringContent(jsonPayload, Encoding.UTF8, "application/json");

                HttpResponseMessage response = await client.PostAsync(apiUrl, content);
                string responseBody = await response.Content.ReadAsStringAsync();

                if (!response.IsSuccessStatusCode)
                {
                    return Json(new { reply = "I am currently overloaded. Please try again later." }, JsonRequestBehavior.AllowGet);
                }

                dynamic result = JsonConvert.DeserializeObject(responseBody);
                string aiReply = result?.choices[0]?.message?.content?.ToString()?.Trim() ?? "I didn't understand that.";

                return Json(new { reply = aiReply }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception)
            {
                return Json(new { reply = "System error. Please try again." }, JsonRequestBehavior.AllowGet);
            }
        }
    }
}