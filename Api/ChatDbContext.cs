using Microsoft.EntityFrameworkCore;

public class ChatDbContext : DbContext
{
    public ChatDbContext(DbContextOptions<ChatDbContext> options) : base(options)
    {
    }
    public DbSet<Chat> Chats => Set<Chat>();
    public DbSet<ChatMessage> Messages => Set<ChatMessage>();
}