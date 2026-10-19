using Microsoft.EntityFrameworkCore;
using OllamaSharp;
using OllamaSharp.Models.Chat;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddDbContext<ChatDbContext>(options =>
    options.UseSqlite("Data Source=chat.db"));

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var dbContext = scope.ServiceProvider.GetRequiredService<ChatDbContext>();
    dbContext.Database.EnsureCreated();
}

var ollama = new OllamaApiClient(new Uri("http://localhost:11434"));
ollama.SelectedModel = "llama3.2:1b";

app.MapGet("/", () => "Hello from MyAi API!");

app.MapPost("/chat", async (ChatInput input, ChatDbContext db) =>
{
    db.Messages.Add(new ChatMessage { Role = "user", Content = input.Message, CreatedAt = DateTime.UtcNow });
    await db.SaveChangesAsync();

    var lastMessages = await db.Messages
        .OrderByDescending(m => m.Id)
        .Take(20)
        .ToListAsync();
    lastMessages.Reverse();

    var request = new ChatRequest
    {
        Model = ollama.SelectedModel,
        Messages = lastMessages
            .Select(m => new Message { Role = new ChatRole(m.Role), Content = m.Content })
            .ToList()
    };

    string answer = "";
    await foreach (var chunk in ollama.ChatAsync(request))
    {
        answer += chunk?.Message?.Content;
    }

    db.Messages.Add(new ChatMessage { Role = "assistant", Content = answer, CreatedAt = DateTime.UtcNow });
    await db.SaveChangesAsync();

    return new ChatOutput(answer);
});

app.Run();

record ChatInput(string Message);
record ChatOutput(string Answer);
