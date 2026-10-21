public class ChatMessage
{
    public int Id {get;set;}
    public int ChatId {get;set;}
    public string Role {get;set;} = "";
    public string Content {get;set;} = "";
    public DateTime CreatedAt {get;set;}
}