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
                    return Json(new { reply = "Error: AI_SERVICE_API_KEY not found. Please check your .env file." }, JsonRequestBehavior.AllowGet);
                }

                string apiUrl = "https://gpt1.shupremium.com/v1/chat/completions";

                client.DefaultRequestHeaders.Clear();
                client.DefaultRequestHeaders.Authorization =
                    new AuthenticationHeaderValue("Bearer", apiKey);

                var systemPrompt = new ApiMessage
                {
                    Role = "system",
                    Content = "You are the 'Recharge & Bill Payment Assistant' for an online mobile recharge and bill payment platform. Your primary purpose is to provide helpful, concise, and professional support regarding mobile top-ups, checking balances, paying utility bills, transaction status inquiries, and accepted payment methods. Keep your answers strictly focused on the domain of mobile recharge and billing. Do not engage in conversations about general knowledge, coding, or unrelated topics."
                };

                var combinedMessages = new List<ApiMessage> { systemPrompt };
                combinedMessages.AddRange(request.Messages);


                var apiPayload = new
                {
                    model = "gpt-3.5-turbo",
                    messages = combinedMessages,
                    temperature = 0.7,
                    max_tokens = 400
                };

                string jsonPayload = JsonConvert.SerializeObject(apiPayload);
                var content = new StringContent(jsonPayload, Encoding.UTF8, "application/json");

                HttpResponseMessage response = await client.PostAsync(apiUrl, content);
                string responseBody = await response.Content.ReadAsStringAsync();

                if (!response.IsSuccessStatusCode)
                {
                    return Json(new { reply = $"API Error ({response.StatusCode}): {responseBody}" }, JsonRequestBehavior.AllowGet);
                }

                dynamic result = JsonConvert.DeserializeObject(responseBody);

                string aiReply = result?.choices[0]?.message?.content?.ToString()?.Trim() ?? "Sorry, I received an empty or malformed response from the AI.";

                return Json(new { reply = aiReply }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return Json(new { reply = $"An unexpected error occurred: {ex.Message}" }, JsonRequestBehavior.AllowGet);
            }
        }
    }
}