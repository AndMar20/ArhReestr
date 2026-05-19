using DataLayer;
using DataLayer.Models;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Components.Server.ProtectedBrowserStorage;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace WebApp.Services;

/// <summary>
/// Управляет избранным: для авторизованных хранит в БД, для гостей — в локальном хранилище.
/// </summary>
public class FavoriteService
{
    private const string StorageKey = "arh:favorites";
    private readonly ProtectedLocalStorage _storage;
    private readonly IDbContextFactory<ArhReestrContext> _contextFactory;
    private readonly AuthenticationStateProvider _authenticationStateProvider;
    private readonly TimeProvider _timeProvider;
    private readonly ILogger<FavoriteService> _logger;
    private HashSet<int>? _cache;

    public FavoriteService(
        ProtectedLocalStorage storage,
        IDbContextFactory<ArhReestrContext> contextFactory,
        AuthenticationStateProvider authenticationStateProvider,
        TimeProvider timeProvider,
        ILogger<FavoriteService> logger)
    {
        _storage = storage;
        _contextFactory = contextFactory;
        _authenticationStateProvider = authenticationStateProvider;
        _timeProvider = timeProvider;
        _logger = logger;
    }

    public async Task<IReadOnlyCollection<int>> GetAsync()
    {
        var userId = await GetCurrentUserIdAsync();
        if (userId is null)
        {
            _cache ??= await LoadLocalAsync();
            return _cache;
        }

        await using var context = await _contextFactory.CreateDbContextAsync();
        return await context.UserFavorites
            .AsNoTracking()
            .Where(f => f.UserId == userId.Value)
            .Select(f => f.RealEstateId)
            .ToListAsync();
    }

    public async Task<bool> AddAsync(int realEstateId)
    {
        if (await IsSoldAsync(realEstateId))
        {
            throw new InvalidOperationException("Проданный объект нельзя добавить в избранное.");
        }

        var userId = await GetCurrentUserIdAsync();
        if (userId is null)
        {
            var favorites = await EnsureLocalCacheAsync();
            var added = favorites.Add(realEstateId);
            if (added) await PersistLocalAsync(favorites);
            return added;
        }

        await using var context = await _contextFactory.CreateDbContextAsync();
        var exists = await context.UserFavorites.AnyAsync(f => f.UserId == userId.Value && f.RealEstateId == realEstateId);
        if (exists) return false;

        context.UserFavorites.Add(new UserFavorite
        {
            UserId = userId.Value,
            RealEstateId = realEstateId,
            CreatedAt = _timeProvider.GetUtcNow().UtcDateTime
        });
        await context.SaveChangesAsync();
        return true;
    }

    public async Task<bool> RemoveAsync(int realEstateId)
    {
        var userId = await GetCurrentUserIdAsync();
        if (userId is null)
        {
            var favorites = await EnsureLocalCacheAsync();
            var removed = favorites.Remove(realEstateId);
            if (removed) await PersistLocalAsync(favorites);
            return removed;
        }

        await using var context = await _contextFactory.CreateDbContextAsync();
        var entity = await context.UserFavorites.FirstOrDefaultAsync(f => f.UserId == userId.Value && f.RealEstateId == realEstateId);
        if (entity is null) return false;
        context.UserFavorites.Remove(entity);
        await context.SaveChangesAsync();
        return true;
    }

    public async Task<bool> ToggleAsync(int realEstateId)
    {
        if (await IsFavoriteAsync(realEstateId))
        {
            await RemoveAsync(realEstateId);
            return false;
        }

        if (await IsSoldAsync(realEstateId))
        {
            throw new InvalidOperationException("Проданный объект нельзя добавить в избранное.");
        }

        await AddAsync(realEstateId);
        return true;
    }

    public async Task<bool> IsFavoriteAsync(int realEstateId)
    {
        var favorites = await GetAsync();
        return favorites.Contains(realEstateId);
    }

    private async Task<int?> GetCurrentUserIdAsync()
    {
        var state = await _authenticationStateProvider.GetAuthenticationStateAsync();
        var user = state.User;
        if (user.Identity?.IsAuthenticated != true) return null;

        var rawId = user.FindFirstValue(ClaimTypes.NameIdentifier);
        return int.TryParse(rawId, out var userId) ? userId : null;
    }

    private async Task<bool> IsSoldAsync(int realEstateId)
    {
        await using var context = await _contextFactory.CreateDbContextAsync();

        return await context.Interactions
            .AsNoTracking()
            .AnyAsync(i => i.RealEstateId == realEstateId
                && i.DeletedAt == null
                && i.Status != null
                && i.Status.Name.Contains("заверш"));
    }

    private async Task<HashSet<int>> EnsureLocalCacheAsync()
    {
        _cache ??= await LoadLocalAsync();
        return _cache;
    }

    private async Task<HashSet<int>> LoadLocalAsync()
    {
        try
        {
            var result = await _storage.GetAsync<List<int>>(StorageKey);
            if (result.Success && result.Value is { Count: > 0 }) return new HashSet<int>(result.Value);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Не удалось загрузить локальное избранное");
        }

        return new HashSet<int>();
    }

    private async Task PersistLocalAsync(HashSet<int> favorites)
    {
        _cache = favorites;
        await _storage.SetAsync(StorageKey, favorites.ToList());
    }
}
