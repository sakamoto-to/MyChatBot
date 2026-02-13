using Microsoft.Extensions.Configuration;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.ChatCompletion;
using Mscc.GenerativeAI;

// 設定ファイルを読み込む
var configuration = new ConfigurationBuilder()
    .SetBasePath(Directory.GetCurrentDirectory())
    .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
    .Build();

var provider = configuration["Provider"] ?? "OpenAI";

Console.WriteLine("🤖 チャットボットへようこそ！");
Console.WriteLine($"📡 使用プロバイダー: {provider}");
Console.WriteLine("終了するには 'exit' または 'quit' と入力してください。");
Console.WriteLine("----------------------------------------");

if (provider == "OpenAI")
{
    await RunOpenAIChat(configuration);
}
else if (provider == "Gemini")
{
    await RunGeminiChat(configuration);
}
else
{
    Console.WriteLine($"❌ エラー: 未対応のプロバイダー '{provider}' です。");
}

// OpenAIでチャット
async Task RunOpenAIChat(IConfiguration config)
{
    var apiKey = config["OpenAI:ApiKey"];
    var modelId = config["OpenAI:ModelId"] ?? "gpt-4o";

    if (string.IsNullOrEmpty(apiKey) || apiKey == "YOUR_OPENAI_API_KEY_HERE")
    {
        Console.WriteLine("❌ エラー: appsettings.jsonにOpenAI APIキーを設定してください。");
        return;
    }

    var builder = Kernel.CreateBuilder();
    builder.AddOpenAIChatCompletion(modelId: modelId, apiKey: apiKey);
    var kernel = builder.Build();
    var chatService = kernel.GetRequiredService<IChatCompletionService>();

    var chatHistory = new ChatHistory();
    chatHistory.AddSystemMessage("あなたは親切で知識豊富なアシスタントです。日本語で丁寧に応答してください。");

    while (true)
    {
        Console.Write("\n👤 あなた: ");
        var userInput = Console.ReadLine();

        if (string.IsNullOrWhiteSpace(userInput)) continue;
        if (userInput.ToLower() is "exit" or "quit")
        {
            Console.WriteLine("\n👋 チャットボットを終了します。さようなら！");
            break;
        }

        chatHistory.AddUserMessage(userInput);

        try
        {
            Console.Write("\n🤖 アシスタント: ");
            var fullResponse = "";

            // ストリーミング応答
            await foreach (var chunk in chatService.GetStreamingChatMessageContentsAsync(chatHistory, kernel: kernel))
            {
                var content = chunk.Content ?? "";
                Console.Write(content);
                fullResponse += content;
            }

            Console.WriteLine();
            chatHistory.AddAssistantMessage(fullResponse);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"\n❌ エラーが発生しました: {ex.Message}");
        }
    }
}

// Geminiでチャット
async Task RunGeminiChat(IConfiguration config)
{
    var apiKey = config["Gemini:ApiKey"];
    var modelId = config["Gemini:ModelId"] ?? "gemini-2.0-flash-exp";

    if (string.IsNullOrEmpty(apiKey) || apiKey == "YOUR_GEMINI_API_KEY_HERE")
    {
        Console.WriteLine("❌ エラー: appsettings.jsonにGemini APIキーを設定してください。");
        return;
    }

    var gemini = new GoogleAI(apiKey);
    var model = gemini.GenerativeModel(model: modelId);
    var chat = model.StartChat();

    // システムメッセージを送信
    await chat.SendMessage("あなたは親切で知識豊富なアシスタントです。日本語で丁寧に応答してください。");

    while (true)
    {
        Console.Write("\n👤 あなた: ");
        var userInput = Console.ReadLine();

        if (string.IsNullOrWhiteSpace(userInput)) continue;
        if (userInput.ToLower() is "exit" or "quit")
        {
            Console.WriteLine("\n👋 チャットボットを終了します。さようなら！");
            break;
        }

        try
        {
            Console.Write("\n🤖 アシスタント: ");

            // ストリーミング応答
            await foreach (var chunk in chat.SendMessageStream(userInput))
            {
                var content = chunk.Text ?? "";
                Console.Write(content);
            }

            Console.WriteLine();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"\n❌ エラーが発生しました: {ex.Message}");
        }
    }
}
