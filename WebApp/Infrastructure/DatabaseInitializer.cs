namespace WebApp.Infrastructure;

/// <summary>
/// Заглушка для инициализации БД: миграции и наполнение выполняются вне приложения.
/// Legacy seeding placeholder retained to simplify branch merges.
/// No database initialization is performed here; DefaultConnection must be configured and available.
/// </summary>
public static class DatabaseInitializer
{
    public static Task EnsureCreatedAsync(IServiceProvider serviceProvider, CancellationToken cancellationToken = default)
    {
        // Seeding was removed per current requirements; this method now acts as a no-op shim.
        // Ничего не делаем, чтобы сохранить совместимость с прежним интерфейсом.
        return Task.CompletedTask;
    }
}
