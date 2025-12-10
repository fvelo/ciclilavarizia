using ChatLib;
using Microsoft.AspNetCore.SignalR;
using System.Text;

namespace SellinBE.Hubs
{
    public class ChatHub : Hub
    {
        private readonly MessageService _messageService;

        public ChatHub(MessageService messageService)
        {
            _messageService = messageService;
        }

        public async Task RequestResponse(string user, string message)
        {
            await Clients.Caller.SendAsync("ShowMessage", user, message);

            await Clients.Caller.SendAsync("StartAssistantMessage");

            StringBuilder buffer = new();

            await foreach (var chunk in MessageService.AskBikeGPTStream("user", message, modelName: "BikeGPT"))
            {
                buffer.Append(chunk);

                string text = buffer.ToString();

                var words = text.Split([' ', '\n', '\t'], StringSplitOptions.None);

                for (int i = 0; i < words.Length - 1; i++)
                {
                    string separator = "";
                    if (i < words.Length - 1)
                    {
                        if (text.Contains($"{words[i]} ")) separator = " ";
                        else if (text.Contains($"{words[i]}\n")) separator = "\n";
                        else if (text.Contains($"{words[i]}\t")) separator = "\t";
                    }

                    await Clients.Caller.SendAsync("StreamMessageChunk", words[i] + separator);
                    await Task.Delay(30);
                }

                buffer.Clear();
                buffer.Append(words[^1]);
            }

            if (buffer.Length > 0)
            {
                await Clients.Caller.SendAsync("StreamMessageChunk", buffer.ToString());
            }

            await Clients.Caller.SendAsync("EndAssistantMessage");
        }
    }
}