public class Chat
{
    public int Id { get; set; }
    public string Title { get; set; } = "";
    public DateTime CreatedAt {get; set; } 
    public List<ChatMessage> Messages {get; set;} = new List<ChatMessage>();
    
    
}