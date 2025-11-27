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
            // ƯU TIÊN 1: Dùng fallback cho các câu hỏi thông thường
            string userMessage = request.Messages?.LastOrDefault()?.Content?.ToLower() ?? "";
            string fallbackResponse = GetRuleBasedResponse(userMessage);

            // ƯU TIÊN 2: Chỉ gọi API cho các câu hỏi PHỨC TẠP về recharge
            if (!IsComplexRechargeQuestion(userMessage))
            {
                // Trả về fallback ngay lập tức cho câu hỏi đơn giản
                await Task.Delay(300); // Simulate processing time
                return Json(new { reply = fallbackResponse }, JsonRequestBehavior.AllowGet);
            }

            // ƯU TIÊN 3: Thử API với retry logic (cho câu hỏi phức tạp)
            int retryCount = 0;
            const int maxRetries = 2;

            while (retryCount < maxRetries)
            {
                try
                {
                    string apiKey = Environment.GetEnvironmentVariable("AI_SERVICE_API_KEY");
                    if (string.IsNullOrEmpty(apiKey)) break;

                    using (var httpClient = new HttpClient())
                    {
                        httpClient.DefaultRequestHeaders.Authorization =
                            new AuthenticationHeaderValue("Bearer", apiKey);
                        httpClient.Timeout = TimeSpan.FromSeconds(15);

                        var systemPrompt = new ApiMessage
                        {
                            Role = "system",
                            Content = "You are a helpful assistant for mobile recharge and bill payment services. Provide concise, professional answers."
                        };

                        var combinedMessages = new List<ApiMessage> { systemPrompt };
                        combinedMessages.AddRange(request.Messages);

                        var apiPayload = new
                        {
                            model = "gpt-3.5-turbo",
                            messages = combinedMessages,
                            temperature = 0.7,
                            max_tokens = 300
                        };

                        string jsonPayload = JsonConvert.SerializeObject(apiPayload);
                        var content = new StringContent(jsonPayload, Encoding.UTF8, "application/json");

                        var response = await httpClient.PostAsync("https://gpt1.shupremium.com/v1/chat/completions", content);

                        if (response.IsSuccessStatusCode)
                        {
                            string responseBody = await response.Content.ReadAsStringAsync();
                            dynamic result = JsonConvert.DeserializeObject(responseBody);
                            string aiReply = result?.choices[0]?.message?.content?.ToString()?.Trim();

                            if (!string.IsNullOrEmpty(aiReply))
                            {
                                return Json(new { reply = aiReply }, JsonRequestBehavior.AllowGet);
                            }
                        }
                        else if (response.StatusCode == (System.Net.HttpStatusCode)429)
                        {
                            retryCount++;
                            if (retryCount < maxRetries)
                            {
                                await Task.Delay(2000 * retryCount); // Wait 2s, then 4s
                                continue;
                            }
                        }
                    }
                }
                catch
                {
                    retryCount++;
                    if (retryCount >= maxRetries) break;
                    await Task.Delay(1000 * retryCount);
                }

                break;
            }

            // Fallback cuối cùng
            return Json(new { reply = fallbackResponse }, JsonRequestBehavior.AllowGet);
        }

        private bool IsComplexRechargeQuestion(string userMessage)
        {
            if (string.IsNullOrEmpty(userMessage)) return false;

            var complexKeywords = new[]
            {
        "trouble", "problem", "error", "failed", "not working", "why can't",
        "how to fix", "support", "technical", "issue", "complaint",
        "refund", "cancel", "dispute", "transaction failed"
    };

            string lowerMessage = userMessage.ToLower();
            return complexKeywords.Any(keyword => lowerMessage.Contains(keyword));
        }

        private string GetRuleBasedResponse(string userMessage)
        {
            if (string.IsNullOrEmpty(userMessage))
                return "👋 Hello! I'm your Recharge Assistant. How can I help with your mobile recharge or bill payment today?";

            userMessage = userMessage.ToLower();

            if (userMessage.Contains("recharge") || userMessage.Contains("top up") || userMessage.Contains("top-up"))
            {
                return @"📱 **Mobile Recharge Guide**

        1. **Go to Online Recharge** section
        2. **Select your operator** (Viettel, Mobifone, Vinaphone, etc.)
        3. **Enter your mobile number**
        4. **Choose a recharge plan** ($1 to $50 available)
        5. **Complete secure payment**

        ⚡ Recharges are instant! You'll receive confirmation immediately.";
            }
            else if (userMessage.Contains("bill") || userMessage.Contains("payment") || userMessage.Contains("invoice"))
            {
                return @"💡 **Bill Payment Services**

        • ⚡ Electricity bills
        • 💧 Water bills 
        • 🔥 Gas bills
        • 📶 Internet & TV bills

        **How to pay:**
        1. Visit 'Post Bill Payment' section
        2. Enter your customer ID
        3. Verify bill details
        4. Complete secure payment

        ✅ All payments are processed instantly!";
            }
            else if (userMessage.Contains("operator") || userMessage.Contains("carrier") || userMessage.Contains("mobifone") || userMessage.Contains("viettel") || userMessage.Contains("vinaphone"))
            {
                return @"🏪 **Supported Mobile Operators**

        • **Viettel** - Full support
        • **Mobifone** - Full support  
        • **Vinaphone** - Full support
        • **Vietnamobile** - Full support
        • **Gmobile** - Full support

        All prepaid numbers are supported with instant recharge! 🚀";
            }
            else if (userMessage.Contains("plan") || userMessage.Contains("package") || userMessage.Contains("data") || userMessage.Contains("call"))
            {
                return @"💰 **Popular Recharge Plans**

        • **$1 Plan**: 1GB data + 30 mins calls
        • **$5 Plan**: 5GB data + 200 mins calls  
        • **$10 Plan**: 10GB data + 500 mins calls
        • **$20 Plan**: Unlimited data + 1000 mins calls
        • **Custom Amounts**: $1 to $50 available

        💎 Check 'Online Recharge' for all available plans!";
            }
            else if (userMessage.Contains("hello") || userMessage.Contains("hi") || userMessage.Contains("hey"))
            {
                return @"👋 Hello! Welcome to **Recharge System**! 

        I'm your AI assistant and I can help you with:

        • 📱 Mobile Recharges
        • 💡 Bill Payments  
        • 🏪 Operator Information
        • 💰 Plan Details & Pricing
        • 🔧 Technical Support

        What would you like to know today? 😊";
            }
            else if (userMessage.Contains("thank") || userMessage.Contains("thanks"))
            {
                return @"😊 You're welcome! 

        I'm always here to help with your recharge and bill payment needs. 

        If you have any other questions, don't hesitate to ask! 💫";
            }
            else if (userMessage.Contains("support") || userMessage.Contains("help") || userMessage.Contains("problem") || userMessage.Contains("issue"))
            {
                return @"🔧 **Support Center**

        **Common Solutions:**
        • Recharge not working? Check transaction history
        • Payment failed? Verify customer ID
        • Technical issues? Clear browser cache

        **Contact Options:**
        • Phone: 1800-1234 (8AM-10PM)
        • Email: support@recharge.com
        • Live Chat: Available 24/7

        We're here to help! 🛠️";
            }

            // Ensure all code paths return a value
            return "Sorry, I couldn't understand your request. Please provide more details about your recharge or bill payment question.";
        }

    }
}