using Microsoft.AspNetCore.SignalR;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.ChatCompletion;
using Mscc.GenerativeAI;

namespace MyChatBot.Web.Hubs;

public class ChatHub : Hub
{
    private readonly IConfiguration _configuration;

    public ChatHub(IConfiguration configuration)
    {
        _configuration = configuration;
    }

    public async Task SendMessage(string message)
    {
        var provider = _configuration["Provider"] ?? "OpenAI";

        try
        {
            if (provider == "OpenAI")
            {
                await StreamOpenAI(message);
            }
            else if (provider == "Gemini")
            {
                await StreamGemini(message);
            }
        }
        catch (Exception ex)
        {
            await Clients.Caller.SendAsync("ReceiveError", ex.Message);
        }
    }

    private async Task StreamOpenAI(string message)
    {
        var apiKey = _configuration["OpenAI:ApiKey"];
        var modelId = _configuration["OpenAI:ModelId"] ?? "gpt-4o";

        if (string.IsNullOrEmpty(apiKey) || apiKey == "YOUR_OPENAI_API_KEY_HERE")
        {
            await Clients.Caller.SendAsync("ReceiveError", "OpenAI APIキーが設定されていません");
            return;
        }

        var builder = Kernel.CreateBuilder();
        builder.AddOpenAIChatCompletion(modelId: modelId, apiKey: apiKey);
        var kernel = builder.Build();
        var chatService = kernel.GetRequiredService<IChatCompletionService>();

        var chatHistory = new ChatHistory();
        chatHistory.AddSystemMessage("あなたは親切で知識豊富なアシスタントです。日本語で丁寧に応答してください。");
        chatHistory.AddUserMessage(message);

        await foreach (var chunk in chatService.GetStreamingChatMessageContentsAsync(chatHistory, kernel: kernel))
        {
            var content = chunk.Content ?? "";
            if (!string.IsNullOrEmpty(content))
            {
                await Clients.Caller.SendAsync("ReceiveMessageChunk", content);
            }
        }

        await Clients.Caller.SendAsync("ReceiveMessageComplete");
    }

    private async Task StreamGemini(string message)
    {
        var apiKey = _configuration["Gemini:ApiKey"];
        var modelId = _configuration["Gemini:ModelId"] ?? "gemini-2.0-flash-exp";

        if (string.IsNullOrEmpty(apiKey) || apiKey == "YOUR_GEMINI_API_KEY_HERE")
        {
            await Clients.Caller.SendAsync("ReceiveError", "Gemini APIキーが設定されていません");
            return;
        }

        var gemini = new GoogleAI(apiKey);
        var model = gemini.GenerativeModel(model: modelId);
        var chat = model.StartChat();

        await foreach (var chunk in chat.SendMessageStream(message))
        {
            var content = chunk.Text ?? "";
            if (!string.IsNullOrEmpty(content))
            {
                await Clients.Caller.SendAsync("ReceiveMessageChunk", content);
            }
        }

        await Clients.Caller.SendAsync("ReceiveMessageComplete");
    }
}
