using ChatLib.Models.OllamaChat;
using Microsoft.Extensions.DependencyInjection;

namespace ChatLib
{
    public static class MessageServiceExtension
    {
        public static IServiceCollection AddMessageService(this IServiceCollection services)
        {
            MessageService messageService = new();
            services.AddSingleton(messageService);
            return services;
        }
    }

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