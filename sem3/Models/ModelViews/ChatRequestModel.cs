using System.Collections.Generic;
using Newtonsoft.Json;

namespace sem3.Models.ModelViews
{
    public class ApiMessage
    {
        [JsonProperty("role")]
        public string Role { get; set; }

        [JsonProperty("content")]
        public string Content { get; set; } 
    }
    public class ChatRequestModel
    {
        [JsonProperty("model")]
        public string Model { get; set; } 

        [JsonProperty("messages")]
        public List<ApiMessage> Messages { get; set; }

        [JsonProperty("temperature")]
        public double Temperature { get; set; } 

        [JsonProperty("max_tokens")]
        public int? MaxTokens { get; set; }
    }
}