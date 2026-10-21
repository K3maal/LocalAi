using System.Net.Http.Json;
using Shared;

var http = new HttpClient();
http.BaseAddress = new Uri("http://localhost:5265");
http.Timeout = TimeSpan.FromMinutes(5);

Console.WriteLine("Welcome to MyAi! Type 'exit' to quit.");

while (true)
{
    Console.Write("You: ");
    string? input = Console.ReadLine();

    if (input?.ToLower() == "exit")
    {
        break;
    }

    if (string.IsNullOrWhiteSpace(input))
    {
        continue;
    }

    Console.Write("AI: ");

    var response = await http.PostAsJsonAsync("/chat", new ChatInput(input));
    response.EnsureSuccessStatusCode();

    var output = await response.Content.ReadFromJsonAsync<ChatOutput>();
    Console.WriteLine(output?.Answer);
}

Console.WriteLine("Goodbye!");
