using OllamaSharp;
 
var ollama = new OllamaApiClient(new Uri("http://localhost:11434"));
ollama.SelectedModel = "llama3.2:1b";

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

    Console.Write("AI: ");
    await foreach (var chunk in ollama.GenerateAsync(input))
    {
        Console.Write(chunk?.Response);
    }
    Console.WriteLine();
}