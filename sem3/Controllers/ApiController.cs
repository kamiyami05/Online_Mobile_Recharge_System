using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using System.Web.Mvc;
using Newtonsoft.Json;
using sem3.Models.ModelViews;
using sem3.Models.Repositories;
using System.Linq;

namespace sem3.Controllers
{
    public class ApiController : Controller
    {
        private static readonly HttpClient client = new HttpClient();
        private readonly PlanRepository _planRepo = new PlanRepository();

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

                var plans = _planRepo.GetAll()
                    .Where(p => p.IsActive == true)
                    .Select(p => $"- {p.Operator} {p.Amount:N0}đ ({p.PlanType}): {p.Details}")
                    .ToList();

                string plansContext = string.Join("\n", plans);
                string apiUrl = "https://gpt1.shupremium.com/v1/chat/completions";

                client.DefaultRequestHeaders.Clear();
                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", apiKey);
                var systemPromptContent = $@"
                    You are the 'Recharge & Bill Payment Assistant'.

                    [IMPORTANT] REAL-TIME DATA FROM DATABASE:
                    Here is the list of currently available recharge plans in our system. 
                    Use ONLY this information to answer user questions about prices and plans. 
                    DO NOT hallucinate or make up plans that are not in this list.

                    AVAILABLE PLANS:
                    {plansContext}

                    FORMATTING RULES:
                    - Use clear line breaks.
                    - Use bullet points (•) for listing plans.
                    - Format currency as 50,000đ.
                    
                    Your primary purpose is to support mobile top-ups, bill payments, and answer queries based on the data provided above.
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
                    temperature = 0.5,
                    max_tokens = 500
                };
                string jsonPayload = JsonConvert.SerializeObject(apiPayload);
                var content = new StringContent(jsonPayload, Encoding.UTF8, "application/json");

                HttpResponseMessage response = await client.PostAsync(apiUrl, content);
                string responseBody = await response.Content.ReadAsStringAsync();

                if (!response.IsSuccessStatusCode)
                {
                    return Json(new { reply = $"API Error: {response.StatusCode}" }, JsonRequestBehavior.AllowGet);
                }

                dynamic result = JsonConvert.DeserializeObject(responseBody);
                string aiReply = result?.choices[0]?.message?.content?.ToString()?.Trim() ?? "Empty response.";

                return Json(new { reply = aiReply }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return Json(new { reply = $"Error: {ex.Message}" }, JsonRequestBehavior.AllowGet);
            }
        }
        protected override void Dispose(bool disposing)
        {
            if (disposing) _planRepo.Dispose();
            base.Dispose(disposing);
        }
    }
}
