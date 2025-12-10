using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace ChatLib.Models.OllamaChat
{
    public class OllamaChatRequest
    {
        [JsonPropertyName("model")]
        public string? Model { get; set; }

        [JsonPropertyName("messages")]
        public List<OllamaMessages>? Messages { get; set; }

        [JsonPropertyName("stream")]
        public bool Stream { get; set; }
    }

    public class OllamaMessages
    {
        [JsonPropertyName("role")]
        public string? Role { get; set; }

        [JsonPropertyName("content")]
        public string? Content { get; set; }
    }

    public class OllamaChatResponse
    {
        [JsonPropertyName("model")]
        public string? Model { get; set; }

        [JsonPropertyName("created_at")]
        public string? CreatedAt { get; set; }

        [JsonPropertyName("message")]
        public OllamaMessages? Message { get; set; }

        [JsonPropertyName("done")]
        public bool Done { get; set; }

    }
}
