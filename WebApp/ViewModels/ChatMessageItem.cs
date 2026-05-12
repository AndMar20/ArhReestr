namespace WebApp.ViewModels;

public record ChatMessageItem(int Id, int SenderId, string SenderName, string Message, DateTime SentAt, DateTime? ReadAt);
