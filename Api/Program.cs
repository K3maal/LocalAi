using OllamaSharp;
using OllamaSharp.Models.Chat;

var builder = WebApplication.CreateBuilder(args);
var app = builder.Build();

var ollama = new OllamaApiClient(new Uri("http://localhost:11434"));
ollama.SelectedModel = "llama3.2:1b";

var history = new List<Message>();

app.MapGet("/", () => "Hello from MyAi API!");

app.MapPost("/chat", async (ChatInput input) =>
{
    history.Add(new Message { Role = ChatRole.User, Content = input.Message });

    var request = new ChatRequest
    {
        Model = ollama.SelectedModel,
        Messages = history.TakeLast(20).ToList()
    };

    string answer = "";
    await foreach (var chunk in ollama.ChatAsync(request))
    {
        answer += chunk?.Message?.Content;
    }

    history.Add(new Message { Role = ChatRole.Assistant, Content = answer });

    return new ChatOutput(answer);
});

app.Run();

record ChatInput(string Message);
record ChatOutput(string Answer);
