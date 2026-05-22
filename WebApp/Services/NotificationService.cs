using System.Data;
using System.Data.Common;
using DataLayer;
using DataLayer.Models;
using Microsoft.EntityFrameworkCore;

namespace WebApp.Services;

public class NotificationService
{
    private readonly IDbContextFactory<ArhReestrContext> _contextFactory;
    private readonly TimeProvider _timeProvider;

    public NotificationService(IDbContextFactory<ArhReestrContext> contextFactory, TimeProvider timeProvider)
    {
        _contextFactory = contextFactory;
        _timeProvider = timeProvider;
    }

    public async Task CreateAsync(int userId, string title, string message, CancellationToken token = default)
    {
        await CreateAsync(userId, title, message, null, token);
    }

    public async Task CreateAsync(int userId, string title, string message, string? linkUrl, CancellationToken token = default)
    {
        await using var context = await _contextFactory.CreateDbContextAsync(token);
        var hasLinkColumn = await EnsureLinkColumnAsync(context, token);

        await ExecuteWithConnectionAsync(context, async connection =>
        {
            await using var command = connection.CreateCommand();
            if (hasLinkColumn)
            {
                command.CommandText = """
                    INSERT INTO Notifications (userId, title, message, linkUrl, isRead, createdAt)
                    VALUES (@userId, @title, @message, @linkUrl, 0, @createdAt);
                    """;
                AddParameter(command, "@linkUrl", linkUrl);
            }
            else
            {
                command.CommandText = """
                    INSERT INTO Notifications (userId, title, message, isRead, createdAt)
                    VALUES (@userId, @title, @message, 0, @createdAt);
                    """;
            }

            AddParameter(command, "@userId", userId);
            AddParameter(command, "@title", title);
            AddParameter(command, "@message", message);
            AddParameter(command, "@createdAt", _timeProvider.GetUtcNow().UtcDateTime);
            await command.ExecuteNonQueryAsync(token);
        }, token);
    }

    public async Task<IReadOnlyList<Notification>> GetForUserAsync(int userId, CancellationToken token = default)
    {
        await using var context = await _contextFactory.CreateDbContextAsync(token);
        var hasLinkColumn = await EnsureLinkColumnAsync(context, token);
        var notifications = new List<Notification>();

        await ExecuteWithConnectionAsync(context, async connection =>
        {
            await using var command = connection.CreateCommand();
            command.CommandText = hasLinkColumn
                ? """
                    SELECT id, userId, title, message, linkUrl, isRead, createdAt
                    FROM Notifications
                    WHERE userId = @userId
                    ORDER BY createdAt DESC
                    LIMIT 100;
                    """
                : """
                    SELECT id, userId, title, message, isRead, createdAt
                    FROM Notifications
                    WHERE userId = @userId
                    ORDER BY createdAt DESC
                    LIMIT 100;
                    """;
            AddParameter(command, "@userId", userId);

            await using var reader = await command.ExecuteReaderAsync(token);
            while (await reader.ReadAsync(token))
            {
                notifications.Add(ReadNotification(reader, hasLinkColumn));
            }
        }, token);

        return notifications;
    }

    public async Task<int> GetUnreadCountAsync(int userId, CancellationToken token = default)
    {
        await using var context = await _contextFactory.CreateDbContextAsync(token);
        var hasLinkColumn = await EnsureLinkColumnAsync(context, token);
        var count = 0;

        await ExecuteWithConnectionAsync(context, async connection =>
        {
            await using var command = connection.CreateCommand();
            command.CommandText = hasLinkColumn
                ? """
                    SELECT COUNT(*)
                    FROM Notifications
                    WHERE userId = @userId
                      AND isRead = 0;
                    """
                : "SELECT COUNT(*) FROM Notifications WHERE userId = @userId AND isRead = 0;";
            AddParameter(command, "@userId", userId);
            count = Convert.ToInt32(await command.ExecuteScalarAsync(token));
        }, token);

        return count;
    }

    public async Task MarkAllAsReadAsync(int userId, CancellationToken token = default)
    {
        await using var context = await _contextFactory.CreateDbContextAsync(token);
        await ExecuteWithConnectionAsync(context, async connection =>
        {
            await using var command = connection.CreateCommand();
            command.CommandText = "UPDATE Notifications SET isRead = 1 WHERE userId = @userId AND isRead = 0;";
            AddParameter(command, "@userId", userId);
            await command.ExecuteNonQueryAsync(token);
        }, token);
    }

    public async Task MarkAsReadAsync(int userId, int notificationId, CancellationToken token = default)
    {
        await using var context = await _contextFactory.CreateDbContextAsync(token);
        await ExecuteWithConnectionAsync(context, async connection =>
        {
            await using var command = connection.CreateCommand();
            command.CommandText = "UPDATE Notifications SET isRead = 1 WHERE id = @id AND userId = @userId AND isRead = 0;";
            AddParameter(command, "@id", notificationId);
            AddParameter(command, "@userId", userId);
            await command.ExecuteNonQueryAsync(token);
        }, token);
    }

    private static async Task<bool> EnsureLinkColumnAsync(ArhReestrContext context, CancellationToken token)
    {
        if (context.Database.IsInMemory())
        {
            return true;
        }

        try
        {
            var exists = false;
            await ExecuteWithConnectionAsync(context, async connection =>
            {
                await using var checkCommand = connection.CreateCommand();
                checkCommand.CommandText = """
                    SELECT COUNT(*)
                    FROM INFORMATION_SCHEMA.COLUMNS
                    WHERE TABLE_SCHEMA = DATABASE()
                      AND TABLE_NAME = 'Notifications'
                      AND COLUMN_NAME = 'linkUrl';
                    """;

                exists = Convert.ToInt32(await checkCommand.ExecuteScalarAsync(token)) > 0;
                if (exists)
                {
                    return;
                }

                await using var alterCommand = connection.CreateCommand();
                alterCommand.CommandText = "ALTER TABLE `Notifications` ADD COLUMN `linkUrl` varchar(500) CHARACTER SET utf8mb4 COLLATE utf8mb4_unicode_ci NULL AFTER `message`;";
                await alterCommand.ExecuteNonQueryAsync(token);
                exists = true;
            }, token);

            return exists;
        }
        catch
        {
            return false;
        }
    }

    private static Notification ReadNotification(DbDataReader reader, bool hasLinkColumn)
    {
        return new Notification
        {
            Id = Convert.ToInt32(reader.GetValue(0)),
            UserId = Convert.ToInt32(reader.GetValue(1)),
            Title = reader.GetString(2),
            Message = reader.GetString(3),
            LinkUrl = hasLinkColumn && !reader.IsDBNull(4) ? reader.GetString(4) : null,
            IsRead = Convert.ToBoolean(reader.GetValue(hasLinkColumn ? 5 : 4)),
            CreatedAt = Convert.ToDateTime(reader.GetValue(hasLinkColumn ? 6 : 5))
        };
    }

    private static async Task ExecuteWithConnectionAsync(ArhReestrContext context, Func<DbConnection, Task> action, CancellationToken token)
    {
        var connection = context.Database.GetDbConnection();
        var shouldClose = connection.State != ConnectionState.Open;

        if (shouldClose)
        {
            await connection.OpenAsync(token);
        }

        try
        {
            await action(connection);
        }
        finally
        {
            if (shouldClose)
            {
                await connection.CloseAsync();
            }
        }
    }

    private static void AddParameter(DbCommand command, string name, object? value)
    {
        var parameter = command.CreateParameter();
        parameter.ParameterName = name;
        parameter.Value = value ?? DBNull.Value;
        command.Parameters.Add(parameter);
    }
}
