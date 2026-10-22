using System.Net.Http.Json;
using Shared;

var http = new HttpClient();
http.BaseAddress = new Uri("http://localhost:5265");
http.Timeout = TimeSpan.FromMinutes(5);
string model = "llama3.2:1b";

Console.WriteLine("Welcome to MyAi!");

while (true)
{
    Console.WriteLine();
    Console.WriteLine("1. New chat");
    Console.WriteLine("2. Open chat");
    Console.WriteLine("3. Delete chat");
    Console.WriteLine($"4. Choose model (now: {model})");
    Console.WriteLine("5. Exit");
    Console.Write("Choose: ");
    string? choice = Console.ReadLine();

    if (choice == "1")
    {
        var response = await http.PostAsync("/chats", null);
        response.EnsureSuccessStatusCode();
        var chat = await response.Content.ReadFromJsonAsync<ChatSummary>();
        if (chat != null)
        {
            await ChatLoop(chat.Id);
        }
    }
    else if (choice == "2")
    {
        var chat = await PickChat();
        if (chat != null)
        {
            var messages = await http.GetFromJsonAsync<List<MessageDto>>($"/chats/{chat.Id}/messages");
            if (messages != null)
            {
                foreach (var m in messages)
                {
                    if (m.Role == "user")
                    {
                        Console.WriteLine($"You: {m.Content}");
                    }
                    else
                    {
                        Console.WriteLine($"AI: {m.Content}");
                    }
                }
            }
            await ChatLoop(chat.Id);
        }
    }
    else if (choice == "3")
    {
        var chat = await PickChat();
        if (chat != null)
        {
            var response = await http.DeleteAsync($"/chats/{chat.Id}");
            response.EnsureSuccessStatusCode();
            Console.WriteLine($"Deleted: {chat.Title}");
        }
    }
    else if (choice == "4")
    {
        var models = await http.GetFromJsonAsync<List<string>>("/models");
        if (models != null)
        {
            for (int i = 0; i < models.Count; i++)
            {
                Console.WriteLine($"{i + 1}. {models[i]}");
            }

            Console.Write("Model number: ");
            string? text = Console.ReadLine();

            if (int.TryParse(text, out int number) && number >= 1 && number <= models.Count)
            {
                model = models[number - 1];
                Console.WriteLine($"Model is now {model}");
            }
            else
            {
                Console.WriteLine("Invalid number.");
            }
        }
    }
    else if (choice == "5")
    {
        break;
    }
}

Console.WriteLine("Goodbye!");

async Task ChatLoop(int chatId)
{
    Console.WriteLine("Type 'back' to return to the menu.");

    while (true)
    {
        Console.Write("You: ");
        string? input = Console.ReadLine();

        if (input?.ToLower() == "back")
        {
            break;
        }

        if (string.IsNullOrWhiteSpace(input))
        {
            continue;
        }

        Console.Write("AI: ");

        var response = await http.PostAsJsonAsync($"/chats/{chatId}/messages", new ChatInput(input, model));
        response.EnsureSuccessStatusCode();

        var output = await response.Content.ReadFromJsonAsync<ChatOutput>();
        Console.WriteLine(output?.Answer);
    }
}

async Task<ChatSummary?> PickChat()
{
    var chats = await http.GetFromJsonAsync<List<ChatSummary>>("/chats");
    if (chats == null || chats.Count == 0)
    {
        Console.WriteLine("No chats yet.");
        return null;
    }

    for (int i = 0; i < chats.Count; i++)
    {
        Console.WriteLine($"{i + 1}. {chats[i].Title}");
    }

    Console.Write("Chat number: ");
    string? text = Console.ReadLine();

    if (int.TryParse(text, out int number) && number >= 1 && number <= chats.Count)
    {
        return chats[number - 1];
    }

    Console.WriteLine("Invalid number.");
    return null;
}