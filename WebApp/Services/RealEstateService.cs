using DataLayer;
using DataLayer.Models;
using Microsoft.AspNetCore.Components.Forms;
using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.Data.Common;
using System.IO;
using WebApp.Infrastructure;
using WebApp.ViewModels;

namespace WebApp.Services;

/// <summary>
/// Сервис для поиска и загрузки объектов недвижимости вместе со справочными данными.
/// </summary>
public class RealEstateService
{
    private readonly IDbContextFactory<ArhReestrContext> _contextFactory;
    private readonly ILogger<RealEstateService> _logger;
    private readonly TimeProvider _timeProvider;
    private readonly IWebHostEnvironment _environment;
    private readonly NotificationService _notificationService;
    private readonly AuditLogService _auditLogService;

    private const long MaxPhotoSize = 5 * 1024 * 1024;
    private const int MaxPhotoFileNameLength = 100;
    private static readonly string[] AllowedPhotoExtensions = new[] { ".jpg", ".jpeg", ".png", ".gif" };

    /// <summary>
    /// Внедряем зависимости через конструктор: контекст EF Core и логгер Blazor-приложения.
    /// </summary>
    public RealEstateService(
        IDbContextFactory<ArhReestrContext> contextFactory,
        ILogger<RealEstateService> logger,
        TimeProvider timeProvider,
        IWebHostEnvironment environment,
        NotificationService notificationService,
        AuditLogService auditLogService)
    {
        _contextFactory = contextFactory;
        _logger = logger;
        _timeProvider = timeProvider;
        _environment = environment;
        _notificationService = notificationService;
        _auditLogService = auditLogService;
    }

    /// <summary>
    /// Выполняет поиск объектов недвижимости по фильтру с пагинацией и сортировкой.
    /// </summary>
    public async Task<PagedResult<RealEstateSummary>> SearchAsync(
        RealEstateFilterModel filter,
        CancellationToken cancellationToken = default)
    {
        try
        {
            ValidateFilter(filter);

            await using var context = await _contextFactory.CreateDbContextAsync(cancellationToken);

            // Базовый запрос к EF Core без трекинга, чтобы лишний раз не держать сущности в контексте.
            var query = context.RealEstates
                .AsNoTracking()
                .Where(r => r.DeletedAt == null);

            if (!filter.IncludeUnavailable)
            {
                query = query.Where(r => r.Status != null && r.Status.Code == "active");
            }

            // Динамически накладываем условия фильтрации — провайдер EF Core превратит их в WHERE.
            if (filter.DistrictId is not null)
            {
                query = query.Where(r => r.House != null && r.House.DistrictId == filter.DistrictId);
            }

            if (filter.StreetId is not null)
            {
                query = query.Where(r => r.House != null && r.House.StreetId == filter.StreetId);
            }

            if (filter.TypeId is not null)
            {
                query = query.Where(r => r.TypeId == filter.TypeId);
            }

            if (filter.MinPrice is not null)
            {
                query = query.Where(r => r.Price >= filter.MinPrice);
            }

            if (filter.MaxPrice is not null)
            {
                query = query.Where(r => r.Price <= filter.MaxPrice);
            }

            if (filter.MinArea is not null)
            {
                query = query.Where(r => r.Area >= filter.MinArea);
            }

            if (filter.MaxArea is not null)
            {
                query = query.Where(r => r.Area <= filter.MaxArea);
            }

            if (filter.Rooms is not null)
            {
                query = query.Where(r => r.Rooms == filter.Rooms);
            }

            if (filter.HasBalcony is not null)
            {
                query = query.Where(r => r.HasBalcony == filter.HasBalcony);
            }

            if (filter.HasParking is not null)
            {
                query = query.Where(r => r.House != null && r.House.HasParking == filter.HasParking);
            }

            if (filter.HasElevator is not null)
            {
                query = query.Where(r => r.House != null && r.House.HasElevator == filter.HasElevator);
            }

            // Сначала считаем общее количество подходящих записей, чтобы вернуть корректную пагинацию.
            var totalCount = await query.CountAsync(cancellationToken);

            var page = filter.Page <= 0 ? 1 : filter.Page;
            var pageSize = Math.Min(Math.Max(filter.PageSize, 1), 200);

            // Подключаем связанные сущности через Include/ThenInclude — EF Core сам построит JOIN.
            query = query
                .Include(r => r.Type)
                .Include(r => r.Agent)
                .Include(r => r.House)
                .ThenInclude(h => h.District)
                .Include(r => r.House)
                .ThenInclude(h => h.Street)
                .Include(r => r.Status)
                .Include(r => r.Photos.Where(p => p.DeletedAt == null))
                .Include(r => r.Interactions.Where(i => i.DeletedAt == null))
                .ThenInclude(i => i.Status);

            // Применяем сортировку в зависимости от выбранного поля и направления.
            query = ApplySorting(query, filter);

            // Выбираем только нужную страницу (Skip/Take) и выполняем запрос асинхронно.
            var entities = await query
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync(cancellationToken);

            // Проецируем сущности в облегчённую модель для UI, не возвращая лишних данных.
            var mapped = entities.Select(MapToSummary).ToList();

            return new PagedResult<RealEstateSummary>(mapped, totalCount, page, pageSize);
        }
        catch (DbException ex)
        {
            var message = DatabaseErrorMessages.Resolve(ex);
            _logger.LogError(ex, message);
            throw new InvalidOperationException(message, ex);
        }
        catch (Exception ex)
        {
            var message = DatabaseErrorMessages.Resolve(ex);
            _logger.LogError(ex, message);
            throw new InvalidOperationException(message, ex);
        }
    }

    /// <summary>
    /// Возвращает краткие сведения по конкретным идентификаторам без учёта фильтров каталога.
    /// </summary>
    public async Task<IReadOnlyList<RealEstateSummary>> GetSummariesByIdsAsync(
        IEnumerable<int> ids,
        CancellationToken cancellationToken = default)
    {
        var idList = ids?.Distinct().ToList() ?? new List<int>();
        if (idList.Count == 0)
        {
            return Array.Empty<RealEstateSummary>();
        }

        try
        {
            await using var context = await _contextFactory.CreateDbContextAsync(cancellationToken);

            var entities = await context.RealEstates
                .AsNoTracking()
                .Where(r => r.DeletedAt == null && idList.Contains(r.Id))
                .Where(r => r.Status != null && r.Status.Code == "active")
                .Include(r => r.Type)
                .Include(r => r.Agent)
                .Include(r => r.House)
                .ThenInclude(h => h.District)
                .Include(r => r.House)
                .ThenInclude(h => h.Street)
                .Include(r => r.Status)
                .Include(r => r.Photos.Where(p => p.DeletedAt == null))
                .Include(r => r.Interactions.Where(i => i.DeletedAt == null))
                .ThenInclude(i => i.Status)
                .ToListAsync(cancellationToken);

            return entities.Select(MapToSummary).ToList();
        }
        catch (DbException ex)
        {
            var message = DatabaseErrorMessages.Resolve(ex);
            _logger.LogError(ex, message);
            throw new InvalidOperationException(message, ex);
        }
        catch (Exception ex)
        {
            var message = DatabaseErrorMessages.Resolve(ex);
            _logger.LogError(ex, message);
            throw new InvalidOperationException(message, ex);
        }
    }

    /// <summary>
    /// Возвращает объекты конкретного риелтора для панели управления.
    /// </summary>
    public async Task<IReadOnlyList<RealEstateSummary>> GetByAgentAsync(int agentId, CancellationToken cancellationToken = default)
    {
        if (agentId <= 0)
        {
            return Array.Empty<RealEstateSummary>();
        }

        try
        {
            await using var context = await _contextFactory.CreateDbContextAsync(cancellationToken);

            var items = await context.RealEstates
                .AsNoTracking()
                .Where(r => r.DeletedAt == null && r.AgentId == agentId)
                .Include(r => r.Type)
                .Include(r => r.Agent)
                .Include(r => r.House)
                .ThenInclude(h => h.District)
                .Include(r => r.House)
                .ThenInclude(h => h.Street)
                .Include(r => r.Status)
                .Include(r => r.Photos.Where(p => p.DeletedAt == null))
                .Include(r => r.Interactions.Where(i => i.DeletedAt == null))
                .ThenInclude(i => i.Status)
                .OrderByDescending(r => r.CreatedAt)
                .ToListAsync(cancellationToken);

            return items.Select(MapToSummary).ToList();
        }
        catch (DbException ex)
        {
            var message = DatabaseErrorMessages.Resolve(ex);
            _logger.LogError(ex, message);
            throw new InvalidOperationException(message, ex);
        }
        catch (Exception ex)
        {
            var message = DatabaseErrorMessages.Resolve(ex);
            _logger.LogError(ex, message);
            throw new InvalidOperationException(message, ex);
        }
    }

    /// <summary>
    /// Загружает одну карточку в кратком формате по идентификатору.
    /// </summary>
    public async Task<RealEstateSummary?> GetSummaryAsync(int id, CancellationToken cancellationToken = default)
    {
        var items = await GetSummariesByIdsAsync(new[] { id }, cancellationToken);
        return items.FirstOrDefault();
    }

    /// <summary>
    /// Проверяем DTO фильтра на корректность перед формированием SQL-запроса.
    /// </summary>
    private static void ValidateFilter(RealEstateFilterModel filter)
    {
        var results = new List<ValidationResult>();
        var context = new ValidationContext(filter);

        if (!Validator.TryValidateObject(filter, context, results, true))
        {
            var message = string.Join(" ", results.Select(r => r.ErrorMessage));
            throw new InvalidOperationException(message);
        }
    }

    /// <summary>
    /// Возвращает список районов для выпадающих списков фильтра или форм ввода.
    /// </summary>
    public async Task<IReadOnlyList<District>> GetDistrictsAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            await using var context = await _contextFactory.CreateDbContextAsync(cancellationToken);

            var districts = await context.Districts.AsNoTracking().OrderBy(d => d.Name).ToListAsync(cancellationToken);
            return districts;
        }
        catch (DbException ex)
        {
            var message = DatabaseErrorMessages.Resolve(ex);
            _logger.LogError(ex, message);
            throw new InvalidOperationException(message, ex);
        }
        catch (Exception ex)
        {
            var message = DatabaseErrorMessages.Resolve(ex);
            _logger.LogError(ex, message);
            throw new InvalidOperationException(message, ex);
        }
    }

    /// <summary>
    /// Загружает справочник типов недвижимости (квартиры, дома и т.п.).
    /// </summary>
    public async Task<IReadOnlyList<RealEstateType>> GetTypesAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            await using var context = await _contextFactory.CreateDbContextAsync(cancellationToken);

            var types = await context.RealEstateTypes.AsNoTracking().OrderBy(t => t.Name).ToListAsync(cancellationToken);
            return types;
        }
        catch (DbException ex)
        {
            var message = DatabaseErrorMessages.Resolve(ex);
            _logger.LogError(ex, message);
            throw new InvalidOperationException(message, ex);
        }
        catch (Exception ex)
        {
            var message = DatabaseErrorMessages.Resolve(ex);
            _logger.LogError(ex, message);
            throw new InvalidOperationException(message, ex);
        }
    }

    /// <summary>
    /// Детально получает объект недвижимости по идентификатору вместе со связанными сущностями.
    /// </summary>
    public async Task<RealEstate?> GetDetailsAsync(int id, bool includeUnavailable = false, CancellationToken cancellationToken = default)
    {
        try
        {
            await using var context = await _contextFactory.CreateDbContextAsync(cancellationToken);

            IQueryable<RealEstate> query = context.RealEstates
                .AsNoTracking()
                .Where(r => r.DeletedAt == null)
                .Include(r => r.Type)
                .Include(r => r.Agent)
                .Include(r => r.House)
                .ThenInclude(h => h.District)
                .Include(r => r.House)
                .ThenInclude(h => h.Street)
                .Include(r => r.Status)
                .Include(r => r.Photos.Where(p => p.DeletedAt == null))
                .Include(r => r.Interactions.Where(i => i.DeletedAt == null))
                .ThenInclude(i => i.Status);

            if (!includeUnavailable)
            {
                query = query.Where(r => r.Status != null && r.Status.Code == "active");
            }

            return await query.FirstOrDefaultAsync(r => r.Id == id, cancellationToken);
        }
        catch (DbException ex)
        {
            var message = DatabaseErrorMessages.Resolve(ex);
            _logger.LogError(ex, message);
            throw new InvalidOperationException(message, ex);
        }
        catch (Exception ex)
        {
            var message = DatabaseErrorMessages.Resolve(ex);
            _logger.LogError(ex, message);
            throw new InvalidOperationException(message, ex);
        }
    }

    /// <summary>
    /// Загружает список улиц для выпадающих списков формы добавления объекта.
    /// </summary>
    public async Task<IReadOnlyList<Street>> GetStreetsAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            await using var context = await _contextFactory.CreateDbContextAsync(cancellationToken);

            return await context.Streets.AsNoTracking().OrderBy(s => s.Name).ToListAsync(cancellationToken);
        }
        catch (DbException ex)
        {
            var message = DatabaseErrorMessages.Resolve(ex);
            _logger.LogError(ex, message);
            throw new InvalidOperationException(message, ex);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Не удалось загрузить список улиц");
            return Array.Empty<Street>();
        }
    }

    /// <summary>
    /// Создаёт новую улицу или возвращает существующую, если она уже есть.
    /// </summary>
    public async Task<Street> CreateStreetAsync(string name, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            throw new InvalidOperationException("Укажите название улицы");
        }

        var normalized = name.Trim();

        try
        {
            await using var context = await _contextFactory.CreateDbContextAsync(cancellationToken);

            var existing = await context.Streets.AsTracking()
                .FirstOrDefaultAsync(s => s.Name == normalized, cancellationToken);

            if (existing is not null)
            {
                return existing;
            }

            var street = new Street { Name = normalized };
            context.Streets.Add(street);
            await context.SaveChangesAsync(cancellationToken);

            return street;
        }
        catch (DbException ex)
        {
            var message = DatabaseErrorMessages.Resolve(ex);
            _logger.LogError(ex, message);
            throw new InvalidOperationException(message, ex);
        }
    }

    /// <summary>
    /// Создаёт новый объект недвижимости. При необходимости создаёт запись дома.
    /// </summary>
    public async Task<int> CreateAsync(RealEstateCreateModel model, int agentId, CancellationToken cancellationToken = default)
    {
        if (agentId <= 0)
        {
            throw new InvalidOperationException("Не удалось определить риелтора");
        }

        ValidateCreateModel(model);

        try
        {
            await using var context = await _contextFactory.CreateDbContextAsync(cancellationToken);

            var typeExists = await context.RealEstateTypes.AnyAsync(t => t.Id == model.TypeId, cancellationToken);
            if (!typeExists)
            {
                throw new InvalidOperationException("Тип недвижимости не найден");
            }

            var statusId = model.StatusId is > 0
                ? model.StatusId.Value
                : await GetStatusIdByCodeAsync(context, "active", cancellationToken);
            var statusExists = await context.RealEstateStatuses.AnyAsync(s => s.Id == statusId, cancellationToken);
            if (!statusExists)
            {
                throw new InvalidOperationException("Статус объекта не найден");
            }

            var districtExists = await context.Districts.AnyAsync(d => d.Id == model.DistrictId, cancellationToken);
            if (!districtExists)
            {
                throw new InvalidOperationException("Район не найден");
            }

            var streetId = await EnsureStreetAsync(context, model, cancellationToken);

            var normalizedNumber = model.HouseNumber.Trim();
            var house = await context.Houses
                .FirstOrDefaultAsync(h => h.DistrictId == model.DistrictId && h.StreetId == streetId && h.Number == normalizedNumber, cancellationToken);

            if (house is null)
            {
                house = new House
                {
                    StreetId = streetId,
                    DistrictId = model.DistrictId!.Value,
                    Number = normalizedNumber,
                    TotalFloors = model.TotalFloors,
                    HasParking = model.HasParking,
                    HasElevator = model.HasElevator,
                    BuildingYear = model.BuildingYear,
                    Latitude = model.Latitude,
                    Longitude = model.Longitude
                };

                context.Houses.Add(house);
            }
            else
            {
                house.DistrictId = model.DistrictId!.Value;
                house.TotalFloors = model.TotalFloors;
                house.HasParking = model.HasParking;
                house.HasElevator = model.HasElevator;
                house.BuildingYear = model.BuildingYear;
                house.Latitude = model.Latitude;
                house.Longitude = model.Longitude;
            }

            if (model.Floor > model.TotalFloors)
            {
                throw new InvalidOperationException("Этаж не может превышать количество этажей в доме");
            }

            var now = _timeProvider.GetUtcNow().UtcDateTime;
            var entity = new RealEstate
            {
                AgentId = agentId,
                TypeId = model.TypeId!.Value,
                StatusId = statusId,
                House = house,
                Description = model.Description,
                Price = model.Price,
                Rooms = model.Rooms,
                Area = model.Area,
                Floor = model.Floor,
                HasBalcony = model.HasBalcony,
                CreatedAt = now
            };

            context.RealEstates.Add(entity);
            await context.SaveChangesAsync(cancellationToken);

            await _auditLogService.WriteAsync("RealEstate", "create", entity.Id, agentId, null, $"Создан объект #{entity.Id}", cancellationToken);
            await _notificationService.CreateAsync(agentId, "Объект создан", $"Объект #{entity.Id} сохранён со статусом \"Активен\".", cancellationToken);

            return entity.Id;
        }
        catch (DbException ex)
        {
            var message = DatabaseErrorMessages.Resolve(ex);
            _logger.LogError(ex, message);
            throw new InvalidOperationException(message, ex);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Не удалось сохранить объект недвижимости");
            throw;
        }
    }

    /// <summary>
    /// Обновляет существующий объект недвижимости с проверкой прав доступа.
    /// </summary>
    public async Task UpdateAsync(RealEstateUpdateModel model, int requesterId, bool isAdmin, CancellationToken cancellationToken = default)
    {
        if (requesterId <= 0)
        {
            throw new InvalidOperationException("Не удалось определить пользователя");
        }

        ValidateUpdateModel(model);

        try
        {
            await using var context = await _contextFactory.CreateDbContextAsync(cancellationToken);

            var entity = await context.RealEstates
                .Include(r => r.House)
                .Include(r => r.Status)
                .FirstOrDefaultAsync(r => r.Id == model.Id && r.DeletedAt == null, cancellationToken);

            if (entity is null)
            {
                throw new InvalidOperationException("Объект недвижимости не найден");
            }

            if (!isAdmin && entity.AgentId != requesterId)
            {
                throw new InvalidOperationException("Недостаточно прав для изменения объекта");
            }

            var isSold = entity.Status?.Code == "sold";
            if (isSold && !isAdmin)
            {
                throw new InvalidOperationException("Проданный объект нельзя редактировать риелтору.");
            }

            var typeExists = await context.RealEstateTypes.AnyAsync(t => t.Id == model.TypeId, cancellationToken);
            if (!typeExists)
            {
                throw new InvalidOperationException("Тип недвижимости не найден");
            }

            var requestedStatusId = model.StatusId is > 0 ? model.StatusId.Value : entity.StatusId;
            var requestedStatus = await context.RealEstateStatuses.FirstOrDefaultAsync(s => s.Id == requestedStatusId, cancellationToken);
            if (requestedStatus is null)
            {
                throw new InvalidOperationException("Статус объекта не найден");
            }

            var districtExists = await context.Districts.AnyAsync(d => d.Id == model.DistrictId, cancellationToken);
            if (!districtExists)
            {
                throw new InvalidOperationException("Район не найден");
            }

            var streetId = await EnsureStreetAsync(context, model, cancellationToken);

            var normalizedNumber = model.HouseNumber.Trim();
            var house = await context.Houses
                .FirstOrDefaultAsync(h => h.DistrictId == model.DistrictId && h.StreetId == streetId && h.Number == normalizedNumber, cancellationToken);

            if (house is null)
            {
                house = new House
                {
                    StreetId = streetId,
                    DistrictId = model.DistrictId!.Value,
                    Number = normalizedNumber
                };

                context.Houses.Add(house);
            }

            house.DistrictId = model.DistrictId!.Value;
            house.TotalFloors = model.TotalFloors;
            house.HasParking = model.HasParking;
            house.HasElevator = model.HasElevator;
            house.BuildingYear = model.BuildingYear;
            house.Latitude = model.Latitude;
            house.Longitude = model.Longitude;

            if (model.Floor > model.TotalFloors)
            {
                throw new InvalidOperationException("Этаж не может превышать количество этажей в доме");
            }

            var oldStatus = entity.Status?.Name ?? string.Empty;
            var oldStatusId = entity.StatusId;

            entity.TypeId = model.TypeId!.Value;
            entity.StatusId = requestedStatusId;
            entity.House = house;
            entity.Description = model.Description;
            entity.Price = model.Price;
            entity.Rooms = model.Rooms;
            entity.Area = model.Area;
            entity.Floor = model.Floor;
            entity.HasBalcony = model.HasBalcony;

            if (isAdmin && model.AgentId is > 0 && model.AgentId != entity.AgentId)
            {
                var agentExists = await context.Users
                    .Include(u => u.Role)
                    .AnyAsync(u => u.Id == model.AgentId && u.DeletedAt == null && u.Role != null && u.Role.Name == "agent", cancellationToken);
                if (!agentExists)
                {
                    throw new InvalidOperationException("Выбранный риелтор не найден.");
                }

                entity.AgentId = model.AgentId.Value;
            }

            await context.SaveChangesAsync(cancellationToken);

            await _auditLogService.WriteAsync(
                "RealEstate",
                "update",
                entity.Id,
                requesterId,
                oldStatusId == requestedStatusId ? null : oldStatus,
                oldStatusId == requestedStatusId ? $"Обновлён объект #{entity.Id}" : requestedStatus.Name,
                cancellationToken);

            if (oldStatusId != requestedStatusId)
            {
                await _notificationService.CreateAsync(entity.AgentId, "Статус объекта изменён", $"Объект #{entity.Id}: {oldStatus} → {requestedStatus.Name}.", cancellationToken);
            }
        }
        catch (DbException ex)
        {
            var message = DatabaseErrorMessages.Resolve(ex);
            _logger.LogError(ex, message);
            throw new InvalidOperationException(message, ex);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Не удалось обновить объект недвижимости");
            throw;
        }
    }

    /// <summary>
    /// Сохраняет фотографии к объекту недвижимости с валидацией расширений и размеров файлов.
    /// </summary>
    public async Task<IReadOnlyList<RealEstatePhoto>> SavePhotosAsync(
        int realEstateId,
        IEnumerable<IBrowserFile> files,
        int uploaderId,
        bool isAdmin,
        CancellationToken cancellationToken = default)
    {
        var fileList = files?.ToList() ?? new List<IBrowserFile>();
        if (fileList.Count == 0)
        {
            return Array.Empty<RealEstatePhoto>();
        }

        await using var context = await _contextFactory.CreateDbContextAsync(cancellationToken);

        var entity = await context.RealEstates
            .Include(r => r.Photos)
            .FirstOrDefaultAsync(r => r.Id == realEstateId && r.DeletedAt == null, cancellationToken);

        if (entity is null)
        {
            throw new InvalidOperationException("Объект недвижимости не найден");
        }

        if (!isAdmin && entity.AgentId != uploaderId)
        {
            throw new InvalidOperationException("Недостаточно прав для загрузки фотографий");
        }

        if (string.IsNullOrWhiteSpace(_environment.WebRootPath))
        {
            throw new InvalidOperationException("Не удалось определить каталог wwwroot для сохранения фотографий");
        }

        var directory = Path.Combine(_environment.WebRootPath, "Images", "RealEstates", entity.Id.ToString());
        Directory.CreateDirectory(directory);

        var hasPrimary = entity.Photos.Any(p => p.DeletedAt == null && p.IsPrimary);
        var savedPhotos = new List<RealEstatePhoto>();

        foreach (var file in fileList)
        {
            var extension = Path.GetExtension(file.Name).ToLowerInvariant();
            if (!AllowedPhotoExtensions.Contains(extension))
            {
                throw new InvalidOperationException("Поддерживаются только изображения JPG, PNG и GIF");
            }

            if (file.Size > MaxPhotoSize)
            {
                throw new InvalidOperationException($"Файл {file.Name} превышает допустимый размер 5 МБ");
            }

            var fileName = $"{Guid.NewGuid():N}{extension}";
            var physicalPath = Path.Combine(directory, fileName);
            var relativePath = $"/Images/RealEstates/{entity.Id}/{fileName}";

            try
            {
                await using var stream = File.Create(physicalPath);
                await file.OpenReadStream(MaxPhotoSize).CopyToAsync(stream, cancellationToken);
            }
            catch
            {
                if (File.Exists(physicalPath))
                {
                    File.Delete(physicalPath);
                }

                throw;
            }

            var photo = new RealEstatePhoto
            {
                RealEstateId = entity.Id,
                FileName = NormalizePhotoFileName(file.Name),
                FilePath = relativePath,
                IsPrimary = !hasPrimary && savedPhotos.Count == 0,
                DeletedAt = null
            };

            entity.Photos.Add(photo);
            savedPhotos.Add(photo);
        }

        await context.SaveChangesAsync(cancellationToken);

        return savedPhotos;
    }

    public async Task<bool> IsUnavailableAsync(int realEstateId, CancellationToken cancellationToken = default)
    {
        await using var context = await _contextFactory.CreateDbContextAsync(cancellationToken);

        return await context.RealEstates
            .AsNoTracking()
            .AnyAsync(r => r.Id == realEstateId
                && r.DeletedAt == null
                && r.Status != null
                && r.Status.Code != "active", cancellationToken);
    }

    private static IQueryable<RealEstate> ExcludeCompletedDeals(IQueryable<RealEstate> query)
    {
        return query.Where(r => !r.Interactions.Any(i =>
            i.DeletedAt == null
            && i.Status != null
            && i.Status.Name.Contains("заверш")));
    }

    private static async Task<int> GetStatusIdByCodeAsync(ArhReestrContext context, string code, CancellationToken cancellationToken)
    {
        var statusId = await context.RealEstateStatuses
            .Where(s => s.Code == code)
            .Select(s => (int?)s.Id)
            .FirstOrDefaultAsync(cancellationToken);

        return statusId ?? throw new InvalidOperationException("Не найден базовый статус объекта.");
    }

    public async Task DeletePhotoAsync(int photoId, int requesterId, bool isAdmin, CancellationToken cancellationToken = default)
    {
        if (requesterId <= 0)
        {
            throw new InvalidOperationException("Не удалось определить пользователя");
        }

        await using var context = await _contextFactory.CreateDbContextAsync(cancellationToken);

        var photo = await context.RealEstatePhotos
            .Include(p => p.RealEstate)
            .FirstOrDefaultAsync(p => p.Id == photoId && p.DeletedAt == null, cancellationToken);

        if (photo?.RealEstate is null || photo.RealEstate.DeletedAt != null)
        {
            throw new InvalidOperationException("Фото не найдено");
        }

        if (!isAdmin && photo.RealEstate.AgentId != requesterId)
        {
            throw new InvalidOperationException("Недостаточно прав для удаления фото");
        }

        var now = _timeProvider.GetUtcNow().UtcDateTime;
        var wasPrimary = photo.IsPrimary;
        photo.DeletedAt = now;
        photo.IsPrimary = false;

        if (wasPrimary)
        {
            var nextPrimary = await context.RealEstatePhotos
                .Where(p => p.RealEstateId == photo.RealEstateId && p.Id != photo.Id && p.DeletedAt == null)
                .OrderBy(p => p.Id)
                .FirstOrDefaultAsync(cancellationToken);

            if (nextPrimary is not null)
            {
                nextPrimary.IsPrimary = true;
            }
        }

        await context.SaveChangesAsync(cancellationToken);

        await _auditLogService.WriteAsync(
            "RealEstatePhoto",
            "delete",
            photo.Id,
            requesterId,
            null,
            $"Удалено фото объекта #{photo.RealEstateId}",
            cancellationToken);
    }

    public async Task DeleteAsync(int realEstateId, int requesterId, bool isAdmin, CancellationToken cancellationToken = default)
    {
        if (requesterId <= 0)
        {
            throw new InvalidOperationException("Не удалось определить пользователя");
        }

        await using var context = await _contextFactory.CreateDbContextAsync(cancellationToken);

        var entity = await context.RealEstates
            .Include(r => r.Photos)
            .FirstOrDefaultAsync(r => r.Id == realEstateId && r.DeletedAt == null, cancellationToken);

        if (entity is null)
        {
            throw new InvalidOperationException("Объект недвижимости не найден");
        }

        if (!isAdmin && entity.AgentId != requesterId)
        {
            throw new InvalidOperationException("Недостаточно прав для удаления объекта");
        }

        var now = _timeProvider.GetUtcNow().UtcDateTime;
        entity.DeletedAt = now;

        foreach (var photo in entity.Photos.Where(p => p.DeletedAt == null))
        {
            photo.DeletedAt = now;
            photo.IsPrimary = false;
        }

        await context.SaveChangesAsync(cancellationToken);

        await _auditLogService.WriteAsync(
            "RealEstate",
            "delete",
            entity.Id,
            requesterId,
            null,
            $"Удалён объект #{entity.Id}",
            cancellationToken);
    }

    private static string NormalizePhotoFileName(string fileName)
    {
        var normalized = Path.GetFileName(fileName);
        return normalized.Length <= MaxPhotoFileNameLength
            ? normalized
            : normalized[..MaxPhotoFileNameLength];
    }

    /// <summary>
    /// Унифицированная точка сортировки, чтобы не размножать OrderBy в основной логике поиска.
    /// </summary>
    private static IQueryable<RealEstate> ApplySorting(IQueryable<RealEstate> query, RealEstateFilterModel filter)
    {
        var sortKey = filter.SortBy?.ToLowerInvariant();

        query = sortKey switch
        {
            "area" => filter.SortDescending ? query.OrderByDescending(r => r.Area) : query.OrderBy(r => r.Area),
            "rooms" => filter.SortDescending ? query.OrderByDescending(r => r.Rooms) : query.OrderBy(r => r.Rooms),
            "district" => filter.SortDescending
                ? query.OrderByDescending(r => r.House != null && r.House.District != null ? r.House.District.Name : string.Empty)
                : query.OrderBy(r => r.House != null && r.House.District != null ? r.House.District.Name : string.Empty),
            _ => filter.SortDescending ? query.OrderByDescending(r => r.Price) : query.OrderBy(r => r.Price)
        };

        return query;
    }

    private static RealEstateSummary MapToSummary(RealEstate r)
    {
        var activePhotos = r.Photos.Where(p => p.DeletedAt == null).ToList();
        var isSold = string.Equals(r.Status?.Code, "sold", StringComparison.OrdinalIgnoreCase);
            return new RealEstateSummary(
                r.Id,
                AddressFormatter.Format(r.House),
            r.House?.District?.Name ?? string.Empty,
            r.Type?.Name ?? string.Empty,
            r.Status?.Name ?? string.Empty,
            r.StatusId,
            r.Price,
            r.Rooms,
            r.Area,
            r.Floor,
            r.House?.TotalFloors ?? 0,
            r.Agent?.GetFullName() ?? string.Empty,
            r.AgentId,
            r.HasBalcony,
            r.House?.HasParking ?? false,
            r.House?.HasElevator ?? false,
            activePhotos.FirstOrDefault(p => p.IsPrimary)?.FilePath ?? activePhotos.FirstOrDefault()?.FilePath,
            r.House?.Latitude,
            r.House?.Longitude,
            isSold);
    }

    /// <summary>
    /// Проверяем корректность модели создания с помощью DataAnnotations.
    /// </summary>
    private static void ValidateCreateModel(RealEstateCreateModel model)
    {
        var results = new List<ValidationResult>();
        var context = new ValidationContext(model);

        if (!Validator.TryValidateObject(model, context, results, true))
        {
            var message = string.Join(" ", results.Select(r => r.ErrorMessage));
            throw new InvalidOperationException(message);
        }

        if (!model.StreetId.HasValue && string.IsNullOrWhiteSpace(model.NewStreetName))
        {
            throw new InvalidOperationException("Выберите улицу или добавьте новую");
        }

        if (model.NewStreetName is { Length: > 150 })
        {
            throw new InvalidOperationException("Название улицы слишком длинное");
        }
    }

    /// <summary>
    /// Дополнительная валидация модели обновления.
    /// </summary>
    private static void ValidateUpdateModel(RealEstateUpdateModel model)
    {
        ValidateCreateModel(model);

        if (model.Id <= 0)
        {
            throw new InvalidOperationException("Некорректный идентификатор объекта");
        }
    }

    private static async Task<int> EnsureStreetAsync(ArhReestrContext context, RealEstateCreateModel model, CancellationToken cancellationToken)
    {
        if (model.StreetId.HasValue)
        {
            var exists = await context.Streets.AnyAsync(s => s.Id == model.StreetId, cancellationToken);
            if (!exists)
            {
                throw new InvalidOperationException("Улица не найдена");
            }

            return model.StreetId.Value;
        }

        var streetName = model.NewStreetName?.Trim();
        if (string.IsNullOrWhiteSpace(streetName))
        {
            throw new InvalidOperationException("Укажите название улицы");
        }

        var existing = await context.Streets
            .AsTracking()
            .FirstOrDefaultAsync(s => s.Name == streetName, cancellationToken);

        if (existing is null)
        {
            existing = new Street { Name = streetName };
            context.Streets.Add(existing);
            await context.SaveChangesAsync(cancellationToken);
        }

        model.StreetId = existing.Id;
        model.NewStreetName = null;
        return existing.Id;
    }
}

