namespace WebApp.Infrastructure;

/// <summary>
/// Хранит текущее состояние подключения к БД для вывода в UI.
/// </summary>
public class DatabaseHealthState
{
    public bool IsAvailable { get; private set; } = true;
    public string? Message { get; private set; }

    public void MarkUnavailable(string message)
    {
        IsAvailable = false;
        Message = message;
    }

    public void MarkAvailable()
    {
        IsAvailable = true;
        Message = null;
    }
}
