using DataLayer;
using Microsoft.EntityFrameworkCore;

namespace WebApp.Services;

public class AdminReferenceService
{
    private readonly IDbContextFactory<ArhReestrContext> _contextFactory;
    private readonly AuditLogService _auditLogService;

    public AdminReferenceService(IDbContextFactory<ArhReestrContext> contextFactory, AuditLogService auditLogService)
    {
        _contextFactory = contextFactory;
        _auditLogService = auditLogService;
    }

    public async Task<IReadOnlyList<ReferenceItem>> GetDistrictsAsync(CancellationToken cancellationToken = default)
    {
        await using var context = await _contextFactory.CreateDbContextAsync(cancellationToken);

        return await context.Districts
            .AsNoTracking()
            .OrderBy(district => district.Name)
            .Select(district => new ReferenceItem(
                district.Id,
                district.Name,
                context.RealEstates.Count(realEstate =>
                    realEstate.DeletedAt == null
                    && realEstate.House != null
                    && realEstate.House.DistrictId == district.Id)))
            .ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<ReferenceItem>> GetStreetsAsync(CancellationToken cancellationToken = default)
    {
        await using var context = await _contextFactory.CreateDbContextAsync(cancellationToken);

        return await context.Streets
            .AsNoTracking()
            .OrderBy(street => street.Name)
            .Select(street => new ReferenceItem(
                street.Id,
                street.Name,
                context.RealEstates.Count(realEstate =>
                    realEstate.DeletedAt == null
                    && realEstate.House != null
                    && realEstate.House.StreetId == street.Id)))
            .ToListAsync(cancellationToken);
    }

    public async Task DeleteDistrictAsync(int districtId, int actorUserId, CancellationToken cancellationToken = default)
    {
        await using var context = await _contextFactory.CreateDbContextAsync(cancellationToken);
        var district = await context.Districts
            .Include(item => item.Houses)
                .ThenInclude(house => house.RealEstates)
            .FirstOrDefaultAsync(item => item.Id == districtId, cancellationToken);

        if (district is null)
        {
            throw new InvalidOperationException("Район не найден.");
        }

        if (district.Houses.Any(house => house.RealEstates.Any(realEstate => realEstate.DeletedAt == null)))
        {
            throw new InvalidOperationException("Нельзя удалить район, к которому привязаны активные объекты.");
        }

        var unusedHouses = district.Houses
            .Where(house => !house.RealEstates.Any())
            .ToList();

        if (unusedHouses.Count > 0)
        {
            context.Houses.RemoveRange(unusedHouses);
        }

        if (district.Houses.Any(house => house.RealEstates.Any()))
        {
            throw new InvalidOperationException("Нельзя удалить район: к нему привязаны архивные записи объектов.");
        }

        var oldValue = district.Name;
        context.Districts.Remove(district);
        await context.SaveChangesAsync(cancellationToken);

        await _auditLogService.WriteAsync("District", "delete", districtId, actorUserId, oldValue, null, cancellationToken);
    }

    public async Task DeleteStreetAsync(int streetId, int actorUserId, CancellationToken cancellationToken = default)
    {
        await using var context = await _contextFactory.CreateDbContextAsync(cancellationToken);
        var street = await context.Streets
            .Include(item => item.Houses)
                .ThenInclude(house => house.RealEstates)
            .FirstOrDefaultAsync(item => item.Id == streetId, cancellationToken);

        if (street is null)
        {
            throw new InvalidOperationException("Улица не найдена.");
        }

        if (street.Houses.Any(house => house.RealEstates.Any(realEstate => realEstate.DeletedAt == null)))
        {
            throw new InvalidOperationException("Нельзя удалить улицу, к которой привязаны активные объекты.");
        }

        var unusedHouses = street.Houses
            .Where(house => !house.RealEstates.Any())
            .ToList();

        if (unusedHouses.Count > 0)
        {
            context.Houses.RemoveRange(unusedHouses);
        }

        if (street.Houses.Any(house => house.RealEstates.Any()))
        {
            throw new InvalidOperationException("Нельзя удалить улицу: к ней привязаны архивные записи объектов.");
        }

        var oldValue = street.Name;
        context.Streets.Remove(street);
        await context.SaveChangesAsync(cancellationToken);

        await _auditLogService.WriteAsync("Street", "delete", streetId, actorUserId, oldValue, null, cancellationToken);
    }
}

public sealed record ReferenceItem(int Id, string Name, int HouseCount);
