using ChatLib.Models.OllamaChat;
using System.Text;
using System.Text.Json;

namespace ChatLib
{
    public class OllamaService
    {
        private readonly HttpClient _httpClient;
        private readonly string _baseUrl;
        private readonly string _modelName;

        public OllamaService(string modelName, string baseUrl = "http://localhost:11434/")
        {
            _httpClient = new()
            {
                Timeout = TimeSpan.FromMinutes(5)
            };
            _baseUrl = baseUrl;
            _modelName = modelName;
        }

        public async IAsyncEnumerable<string> ChatOllamaStreamAsync(List<OllamaMessages> conversationHistory)
        {
            var requestPayload = new OllamaChatRequest
            {
                Model = _modelName,
                Messages = conversationHistory,
                Stream = true
            };

            string jsonContent = JsonSerializer.Serialize(requestPayload);
            var httpContent = new StringContent(jsonContent, encoding: Encoding.UTF8, "application/json");

            HttpResponseMessage response;

            response = await _httpClient.PostAsync($"{_baseUrl}api/chat", httpContent).ConfigureAwait(false);

            if (!response.IsSuccessStatusCode)
            {
                yield return $"Errore: {response.StatusCode}";
                yield break;
            }

            using var stream = await response.Content.ReadAsStreamAsync();
            using var reader = new StreamReader(stream);

            while (!reader.EndOfStream)
            {
                var line = await reader.ReadLineAsync();
                if (string.IsNullOrWhiteSpace(line)) continue;

                OllamaChatResponse? ollamaResponse = JsonSerializer.Deserialize<OllamaChatResponse>(line);

                if (ollamaResponse?.Message?.Content != null)
                {
                    yield return ollamaResponse.Message.Content;
                }
            }
        }
    }
}


/*
using ChatLib.Models.OllamaChat;
using System.Text;
using System.Text.Json;

namespace ChatLib
{
    public class OllamaService
    {
        private readonly HttpClient _httpClient;
        private readonly string _baseUrl;
        private readonly string _modelName;

        public OllamaService(string baseUrl = "http://localhost:11434/", string modelName = "BikeGPT")
        {
            _httpClient = new()
            {
                Timeout = TimeSpan.FromSeconds(60)
            };

            _baseUrl = baseUrl;
            _modelName = modelName;
    
        }

        public async Task<string> ChatOllamaAsync(List<OllamaMessages> conversationHistory)
        {
            var requestPayload = new OllamaChatRequest
            {
                Model = _modelName,
                Messages = conversationHistory,
                Stream = false
            };

            string jsonContent = JsonSerializer.Serialize(requestPayload);
            var httpContent = new StringContent(jsonContent, encoding: Encoding.UTF8, "application/json");

            try
            {
                var response = await _httpClient.PostAsync($"{_baseUrl}api/chat", httpContent);

                response.EnsureSuccessStatusCode();

                string jsonResponse = await response.Content.ReadAsStringAsync();
                var ollamaResponse = JsonSerializer.Deserialize<OllamaChatResponse>(jsonResponse);

                return ollamaResponse?.Message?.Content ?? "Errore: Risposta vuota da Ollama";
            }
            catch (HttpRequestException httpEx)
            {
                return $"Errore di connessione a Ollama: {httpEx.Message}. Vericare che Ollama sia in esecuzione";
                throw;
            }

            catch (Exception ex)
            {
                return $"Errore imprevisto sulla connessione ad Ollama: {ex.Message}";
            }

        }

    }
}
*/