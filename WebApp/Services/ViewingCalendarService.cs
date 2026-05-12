using DataLayer;
using DataLayer.Models;
using Microsoft.EntityFrameworkCore;

namespace WebApp.Services;

public class ViewingCalendarService
{
    private readonly IDbContextFactory<ArhReestrContext> _contextFactory;

    public ViewingCalendarService(IDbContextFactory<ArhReestrContext> contextFactory)
    {
        _contextFactory = contextFactory;
    }

    public async Task<IReadOnlyList<ViewingSlot>> GetRealEstateSlotsAsync(int realEstateId, CancellationToken token = default)
    {
        await using var context = await _contextFactory.CreateDbContextAsync(token);
        return await context.ViewingSlots.AsNoTracking().Where(x => x.RealEstateId == realEstateId).OrderBy(x => x.StartsAt).ToListAsync(token);
    }
}
