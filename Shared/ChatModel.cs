namespace Shared;

public record ChatInput(string Message);
public record ChatOutput(string Answer);
public record ChatSummary(int Id, string Title, DateTime CreatedAt);
public record MessageDto(string Role, string Content);  