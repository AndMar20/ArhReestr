using Microsoft.AspNetCore.Components.Server.ProtectedBrowserStorage;

namespace WebApp.Services;

/// <summary>
/// Управляет списком избранных объектов в локальном хранилище браузера.
/// </summary>
public class FavoriteService
{
    private const string StorageKey = "arh:favorites";
    private readonly ProtectedLocalStorage _storage;
    private HashSet<int>? _cache;

    /// <summary>
    /// Внедряем защищённое локальное хранилище Blazor для сохранения избранного.
    /// </summary>
    public FavoriteService(ProtectedLocalStorage storage)
    {
        _storage = storage;
    }

    /// <summary>
    /// Возвращает текущий набор избранных идентификаторов, подгружая его из storage один раз.
    /// </summary>
    public async Task<IReadOnlyCollection<int>> GetAsync()
    {
        _cache ??= await LoadAsync();
        return _cache;
    }

    /// <summary>
    /// Добавляет объект в избранное; возвращает true, если элемент добавлен впервые.
    /// </summary>
    public async Task<bool> AddAsync(int realEstateId)
    {
        var favorites = await EnsureCacheAsync();
        var added = favorites.Add(realEstateId);
        if (added)
        {
            await PersistAsync(favorites);
        }

        return added;
    }

    /// <summary>
    /// Удаляет объект из избранного; возвращает true, если он там был.
    /// </summary>
    public async Task<bool> RemoveAsync(int realEstateId)
    {
        var favorites = await EnsureCacheAsync();
        var removed = favorites.Remove(realEstateId);
        if (removed)
        {
            await PersistAsync(favorites);
        }

        return removed;
    }

    /// <summary>
    /// Переключает наличие элемента: если был — удаляет, если не было — добавляет.
    /// </summary>
    public async Task<bool> ToggleAsync(int realEstateId)
    {
        var favorites = await EnsureCacheAsync();
        var added = favorites.Contains(realEstateId)
            ? !favorites.Remove(realEstateId)
            : favorites.Add(realEstateId);

        await PersistAsync(favorites);
        return added;
    }

    /// <summary>
    /// Проверяет, находится ли объект в избранном.
    /// </summary>
    public async Task<bool> IsFavoriteAsync(int realEstateId)
    {
        var favorites = await EnsureCacheAsync();
        return favorites.Contains(realEstateId);
    }

    private async Task<HashSet<int>> EnsureCacheAsync()
    {
        _cache ??= await LoadAsync();
        return _cache;
    }

    private async Task<HashSet<int>> LoadAsync()
    {
        var result = await _storage.GetAsync<List<int>>(StorageKey);
        return result.Success && result.Value is { Count: > 0 }
            ? new HashSet<int>(result.Value)
            : new HashSet<int>();
    }

    private async Task PersistAsync(HashSet<int> favorites)
    {
        _cache = favorites;
        await _storage.SetAsync(StorageKey, favorites.ToList());
    }
}
