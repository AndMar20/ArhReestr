using System;

namespace WebApp.Infrastructure;

/// <summary>
/// Расширения для <see cref="TimeProvider">, фиксирующие работу с часовым поясом МСК.
/// </summary>
public static class TimeProviderExtensions
{
    private static readonly TimeSpan MoscowOffset = TimeSpan.FromHours(3);

    /// <summary>
    /// Возвращает текущее время в часовом поясе Москвы.
    /// </summary>
    public static DateTime GetMoscowDateTime(this TimeProvider timeProvider)
    {
        return timeProvider.GetUtcNow().ToOffset(MoscowOffset).DateTime;
    }
}