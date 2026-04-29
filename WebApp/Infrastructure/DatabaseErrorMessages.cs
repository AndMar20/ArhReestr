using System.Data.Common;

namespace WebApp.Infrastructure;

/// <summary>
/// Описывает человеко-понятные сообщения об ошибках при работе с MySQL.
/// </summary>
public static class DatabaseErrorMessages
{
    public const string ConnectionFailed =
        "Не удалось подключиться к базе данных. Проверьте строку подключения DefaultConnection и доступность MySQL.";

    public const string SchemaMismatch =
        "Схема базы данных не соответствует модели приложения: отсутствует таблица или столбец. Проверьте миграции и структуру БД (например, ошибки 1146, 1051, 1054).";

    public const string DataError =
        "MySQL отклонил запрос из-за некорректных данных или типа. Проверьте значения полей и ограничения (например, ошибка 1366).";

    public const string HostUnreachable =
        "Не удаётся соединиться с MySQL: проверьте хост, порт и что служба запущена (ошибки 1042, 2002, 2003).";

    public const string AccessDenied =
        "MySQL отклонил подключение: проверьте имя пользователя, пароль и привилегии (ошибка 1044/1045).";

    public const string UnknownDatabase =
        "Указанная база данных не найдена. Проверьте имя в строке подключения (ошибка 1049).";

    public const string AuthenticationPlugin =
        "Сервер MySQL требует другой плагин аутентификации. Обновите пользователя или драйвер (ошибка 1251/2059).";

    public const string SslError =
        "Не удалось согласовать SSL-соединение с MySQL. Проверьте сертификаты или отключите SSL, если он не требуется (ошибка 2026).";

    public const string ConnectionLimit =
        "Достигнут предел подключений к MySQL. Освободите соединения или увеличьте лимит (ошибка 1040/1226).";

    public const string Timeout =
        "Таймаут при подключении к MySQL. Проверьте сеть и параметр Connection Timeout.";

    public const string UnexpectedError =
        "Произошла непредвиденная ошибка при обращении к базе данных. Обратитесь к администратору.";

    /// <summary>
    /// Подбирает сообщение об ошибке по общему исключению.
    /// </summary>
    public static string Resolve(Exception exception)
    {
        return exception switch
        {
            DbException dbException => Resolve(dbException),
            TimeoutException => Timeout,
            _ => UnexpectedError
        };
    }

    /// <summary>
    /// Подбирает сообщение об ошибке, анализируя специфичные для MySQL детали DbException.
    /// </summary>
    public static string Resolve(DbException dbException)
    {
        var code = TryGetErrorCode(dbException);
        if (code is not null)
        {
            switch (code)
            {
                case 1044:
                case 1045:
                    return AccessDenied;
                case 1049:
                    return UnknownDatabase;
                case 1040:
                case 1226:
                    return ConnectionLimit;
                case 1042:
                case 2002:
                case 2003:
                case 2005:
                    return HostUnreachable;
                case 1159:
                case 1184:
                case 1205:
                    return Timeout;
                case 1251:
                case 2059:
                    return AuthenticationPlugin;
                case 2026:
                    return SslError;
                case 1051:
                case 1054:
                case 1146:
                    return SchemaMismatch;
                case 1366:
                    return DataError;
            }
        }

        var sqlState = TryGetSqlState(dbException);
        if (!string.IsNullOrWhiteSpace(sqlState))
        {
            return sqlState switch
            {
                "08001" or "08S01" => HostUnreachable,
                "28000" => AccessDenied,
                _ => ConnectionFailed
            };
        }

        if (dbException.InnerException is TimeoutException)
        {
            return Timeout;
        }

        return $"{ConnectionFailed} Детали: {dbException.Message}";
    }

    /// <summary>
    /// Пробует достать числовой код ошибки из исключения провайдера.
    /// </summary>
    private static int? TryGetErrorCode(DbException exception)
    {
        var type = exception.GetType();
        var numberProperty = type.GetProperty("Number");
        if (numberProperty?.GetValue(exception) is int number)
        {
            return number;
        }

        var errorCodeProperty = type.GetProperty("ErrorCode");
        if (errorCodeProperty?.GetValue(exception) is int errorCode)
        {
            return errorCode;
        }

        return null;
    }

    /// <summary>
    /// Пробует извлечь строковый SQLSTATE из исключения провайдера.
    /// </summary>
    private static string? TryGetSqlState(DbException exception)
    {
        var type = exception.GetType();
        var sqlStateProperty = type.GetProperty("SqlState") ?? type.GetProperty("Sqlstate");
        return sqlStateProperty?.GetValue(exception)?.ToString();
    }
}
