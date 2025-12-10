using ChatLib.Models.OllamaChat;

namespace ChatLib
{
    public class MessageService
    {

        public static async IAsyncEnumerable<string> AskBikeGPTStream(string user, string message, string modelName)
        {

            List<OllamaMessages> ollamaMessages =
            [
                new OllamaMessages
                {
                    Role = user,
                    Content = message,
                },
            ];

            OllamaService ollamaService = new(modelName: modelName);

            await foreach (var chunk in ollamaService.ChatOllamaStreamAsync(ollamaMessages))
            {
                yield return chunk;
            }
        }
    }
}