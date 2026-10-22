using Microsoft.EntityFrameworkCore;
using OllamaSharp;
using OllamaSharp.Models.Chat;
using Shared;

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

app.MapGet("/models", async () =>
{
    var models = await ollama.ListLocalModelsAsync();
    return models.Select(m => m.Name).ToList();
});

app.MapPost("/chats", async (ChatDbContext db) =>
{
    var chat = new Chat {Title = "New chat", CreatedAt = DateTime.UtcNow};
    db.Chats.Add(chat);
    await db.SaveChangesAsync();

    return new ChatSummary(chat.Id, chat.Title, chat.CreatedAt);
});

app.MapGet("/chats", async (ChatDbContext db) =>
{
    return await db.Chats
        .OrderByDescending(c => c.CreatedAt)
        .Select(c => new ChatSummary(c.Id, c.Title, c.CreatedAt))
        .ToListAsync();
});


app.MapGet("/chats/{chatId}/messages", async (int chatId, ChatDbContext db) =>
{
    var chat = await db.Chats.FindAsync(chatId);
    if (chat == null)
    {
        return Results.NotFound();
    }

    var messages = await db.Messages
        .Where(m => m.ChatId == chatId)
        .OrderBy(m => m.Id)
        .Select(m => new MessageDto(m.Role, m.Content))
        .ToListAsync();

    return Results.Ok(messages);
});


app.MapPost("/chats/{chatId}/messages", async (int chatId, ChatInput input, ChatDbContext db) =>
{
    var chat = await db.Chats.FindAsync(chatId);
    if (chat == null)
    {
        return Results.NotFound();
    }

    if (chat.Title == "New chat")
    {
        chat.Title = input.Message;
        if (chat.Title.Length > 40)
        {
            chat.Title = chat.Title.Substring(0, 40) + "...";
        }
    }

    db.Messages.Add(new ChatMessage { ChatId = chatId, Role = "user", Content = input.Message, CreatedAt = DateTime.UtcNow });
    await db.SaveChangesAsync();

    var lastMessages = await db.Messages
        .Where(m => m.ChatId == chatId)
        .OrderByDescending(m => m.Id)
        .Take(20)
        .ToListAsync();
    lastMessages.Reverse();

    var request = new ChatRequest
    {
        Model = input.Model ?? ollama.SelectedModel,
        Messages = lastMessages
            .Select(m => new Message { Role = new ChatRole(m.Role), Content = m.Content })
            .ToList()
    };

    string answer = "";
    await foreach (var chunk in ollama.ChatAsync(request))
    {
        answer += chunk?.Message?.Content;
    }

    db.Messages.Add(new ChatMessage { ChatId = chatId, Role = "assistant", Content = answer, CreatedAt = DateTime.UtcNow });
    await db.SaveChangesAsync();

    return Results.Ok(new ChatOutput(answer));
});



app.MapDelete("/chats/{chatId}", async (int chatId, ChatDbContext db) =>
{
    var chat = await db.Chats.FindAsync(chatId);
    if (chat == null)
    {
        return Results.NotFound();
    }

    db.Chats.Remove(chat);
    await db.SaveChangesAsync();

    return Results.NoContent();
});

app.Run();

