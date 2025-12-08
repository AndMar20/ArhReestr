using Microsoft.AspNetCore.Components.Server.ProtectedBrowserStorage;

namespace WebApp.Services;

/// <summary>
/// Управляет списком избранных объектов в локальном хранилище браузера.
/// </summary>
public class FavoriteService
{
    private const string StorageKey = "arh:favorites";
    private readonly ProtectedLocalStorage _storage;
    private readonly ILogger<FavoriteService> _logger;
    private HashSet<int>? _cache;

    public FavoriteService(ProtectedLocalStorage storage, ILogger<FavoriteService> logger)
    {
        _storage = storage;
        _logger = logger;
    }

    public async Task<IReadOnlyCollection<int>> GetAsync()
    {
        _cache ??= await LoadAsync();
        return _cache;
    }

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

    public async Task<bool> ToggleAsync(int realEstateId)
    {
        var favorites = await EnsureCacheAsync();
        bool added;

        if (favorites.Contains(realEstateId))
        {
            favorites.Remove(realEstateId);
            added = false;
        }
        else
        {
            favorites.Add(realEstateId);
            added = true;
        }

        await PersistAsync(favorites);
        return added;
    }

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
        try
        {
            var result = await _storage.GetAsync<List<int>>(StorageKey);

            if (result.Success && result.Value is { Count: > 0 })
            {
                return new HashSet<int>(result.Value);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Не удалось загрузить избранное из локального хранилища, сбрасываем ключ.");

            try
            {
                await _storage.DeleteAsync(StorageKey);
            }
            catch
            {
                // Вторичную ошибку при удалении просто игнорируем
            }
        }

        return new HashSet<int>();
    }

    private async Task PersistAsync(HashSet<int> favorites)
    {
        _cache = favorites;

        try
        {
            await _storage.SetAsync(StorageKey, favorites.ToList());
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Не удалось сохранить избранное в локальное хранилище");
            throw; // пусть поймает UI и покажет сообщение
        }
    }
}