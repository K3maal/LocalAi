using OllamaSharp;
using OllamaSharp.Models.Chat;
 
var ollama = new OllamaApiClient(new Uri("http://localhost:11434"));
ollama.SelectedModel = "llama3.2:1b";

var history = new List<Message>();

Console.WriteLine ("Welcpme to MyAi! Type 'exit' to quit.");
while (true)
{
    Console.Write("You:");
    string? input = Console.ReadLine();

    if (input?.ToLower() == "exit")
    {
        break;
    }
    
    if (string.IsNullOrWhiteSpace(input))
    {
        continue;
    }
    
    history.Add(new Message{ Role = ChatRole.User, Content = input}); 

    var request = new ChatRequest
    {
        Model = ollama.SelectedModel,
        Messages = history.TakeLast(20).ToList()
    };


    Console.Write("AI: ");
    string answer = "";

    await foreach (var chunk in ollama.ChatAsync(request))
    {
        string? piece = chunk?.Message?.Content;
        Console.Write(piece);
        answer += piece;
    }
    Console.WriteLine();

    history.Add(new Message {Role = ChatRole.Assistant, Content= answer});
}

Console.WriteLine("Goodbye!");