using System.Data.Common;
using System.Linq;
using ClosedXML.Excel;
using DataLayer;
using Microsoft.EntityFrameworkCore;
using WebApp.Infrastructure;
using WebApp.ViewModels;

namespace WebApp.Services;

/// <summary>
/// Строит агрегированные отчёты по районам, агентам и статусам обращений.
/// </summary>
public class ReportService
{
    private readonly ArhReestrContext _context;

    /// <summary>
    /// Хранит контекст БД, чтобы выполнять группировки и использовать в Export to Excel.
    /// </summary>
    public ReportService(ArhReestrContext context)
    {
        _context = context;
    }

    /// <summary>
    /// Возвращает агрегированные данные, которые дальше можно экспортировать или показать в UI.
    /// </summary>
    public async Task<(IReadOnlyList<AdminReportRow> Districts, IReadOnlyList<AdminReportRow> Agents, IReadOnlyList<AdminReportRow> Statuses)> BuildAsync(DateTime? from = null, DateTime? to = null, CancellationToken cancellationToken = default)
    {
        try
        {
            var fromDate = NormalizeFrom(from);
            var toDate = NormalizeToExclusive(to);

            var realEstateQuery = _context.RealEstates
                .AsNoTracking()
                .Where(r => r.DeletedAt == null);

            if (fromDate.HasValue)
            {
                realEstateQuery = realEstateQuery.Where(r => r.CreatedAt >= fromDate.Value);
            }

            if (toDate.HasValue)
            {
                realEstateQuery = realEstateQuery.Where(r => r.CreatedAt < toDate.Value);
            }

            var districtCounts = await realEstateQuery
                .Include(r => r.House)
                .GroupBy(r => r.House!.DistrictId)
                .Select(g => new { DistrictId = g.Key, Count = g.Count() })
                .OrderByDescending(r => r.Count)
                .ToListAsync(cancellationToken);

            var districtIds = districtCounts
                .Select(c => c.DistrictId)
                .ToList();

            var districtNames = await _context.Districts
                .AsNoTracking()
                .Where(d => districtIds.Contains(d.Id))
                .ToDictionaryAsync(d => d.Id, d => d.Name, cancellationToken);

            var districtRows = districtCounts
                .Select(d =>
                {
                    var name = districtNames.TryGetValue(d.DistrictId, out var districtName)
                        ? districtName
                        : $"Район #{d.DistrictId}";
                    return new AdminReportRow(name, d.Count);
                })
                .ToList();

            var interactionQuery = _context.Interactions
                .AsNoTracking()
                .Where(i => i.DeletedAt == null);

            if (fromDate.HasValue)
            {
                interactionQuery = interactionQuery.Where(i => i.ContactedAt >= fromDate.Value);
            }

            if (toDate.HasValue)
            {
                interactionQuery = interactionQuery.Where(i => i.ContactedAt < toDate.Value);
            }

            var agentQuery = await interactionQuery
                .GroupBy(i => new { i.Agent!.LastName, i.Agent!.FirstName, i.Agent!.MiddleName })
                .Select(g => new { g.Key.LastName, g.Key.FirstName, g.Key.MiddleName, Count = g.Count() })
                .OrderByDescending(r => r.Count)
                .ToListAsync(cancellationToken);

            var agentRows = agentQuery
                .Select(g => new AdminReportRow(FullNameFormatter.Combine(g.LastName, g.FirstName, g.MiddleName), g.Count))
                .ToList();

            var statusCounts = await interactionQuery
                .GroupBy(i => i.StatusId)
                .Select(g => new { StatusId = g.Key, Count = g.Count() })
                .OrderByDescending(r => r.Count)
                .ToListAsync(cancellationToken);

            var statusIds = statusCounts
                .Select(s => s.StatusId)
                .ToList();

            var statusNames = await _context.InteractionStatuses
                .AsNoTracking()
                .Where(s => statusIds.Contains(s.Id))
                .ToDictionaryAsync(s => s.Id, s => s.Name, cancellationToken);

            var statusRows = statusCounts
                .Select(s =>
                {
                    var name = statusNames.TryGetValue(s.StatusId, out var statusName)
                        ? statusName
                        : $"Статус #{s.StatusId}";
                    return new AdminReportRow(name, s.Count);
                })
                .ToList();

            return (districtRows, agentRows, statusRows);
        }
        catch (DbException ex)
        {
            var message = DatabaseErrorMessages.Resolve(ex);
            throw new InvalidOperationException(message, ex);
        }
    }

    public async Task<AdminDashboardStats> GetDashboardStatsAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            var totalObjects = await _context.RealEstates.AsNoTracking().CountAsync(cancellationToken);
            var activeObjects = await _context.RealEstates.AsNoTracking().CountAsync(r => r.DeletedAt == null && r.Status != null && r.Status.Code == "active", cancellationToken);
            var totalUsers = await _context.Users.AsNoTracking().CountAsync(u => u.DeletedAt == null, cancellationToken);
            var agents = await _context.Users.AsNoTracking().CountAsync(u => u.DeletedAt == null && u.Role != null && u.Role.Name == "agent", cancellationToken);
            var clients = await _context.Users.AsNoTracking().CountAsync(u => u.DeletedAt == null && u.Role != null && u.Role.Name == "client", cancellationToken);
            var totalInteractions = await _context.Interactions.AsNoTracking().CountAsync(i => i.DeletedAt == null, cancellationToken);
            var closedDeals = await _context.Deals.AsNoTracking().CountAsync(cancellationToken);

            return new AdminDashboardStats(totalObjects, activeObjects, totalUsers, agents, clients, totalInteractions, closedDeals);
        }
        catch (DbException ex)
        {
            var message = DatabaseErrorMessages.Resolve(ex);
            throw new InvalidOperationException(message, ex);
        }
    }

    /// <summary>
    /// Собирает Excel-файл с тремя листами, используя данные из BuildAsync.
    /// </summary>
    public async Task<byte[]> BuildExcelAsync(string? type = null, DateTime? from = null, DateTime? to = null, CancellationToken cancellationToken = default)
    {
        var reportType = string.IsNullOrWhiteSpace(type) ? "full" : type.Trim().ToLowerInvariant();
        var (districts, agents, statuses) = await BuildAsync(from, to, cancellationToken);
        var stats = await GetDashboardStatsAsync(cancellationToken);

        using var workbook = new XLWorkbook();

        if (reportType is "full" or "summary")
        {
            var summarySheet = workbook.AddWorksheet("Обзор");
            FillSummaryWorksheet(summarySheet, stats);
        }

        if (reportType is "full" or "districts")
        {
            var districtSheet = workbook.AddWorksheet("Районы");
            FillWorksheet(districtSheet, districts, "Объекты по районам");
        }

        if (reportType is "full" or "agents")
        {
            var agentSheet = workbook.AddWorksheet("Риелторы");
            FillWorksheet(agentSheet, agents, "Активность риелторов");
        }

        if (reportType is "full" or "statuses")
        {
            var statusSheet = workbook.AddWorksheet("Статусы");
            FillWorksheet(statusSheet, statuses, "Итоги взаимодействий");
        }

        if (reportType is "full" or "revenue")
        {
            var revenueRows = await BuildRevenueRowsAsync(from, to, cancellationToken);
            var revenueSheet = workbook.AddWorksheet("Выручка");
            FillCurrencyWorksheet(revenueSheet, revenueRows, "Выручка по завершённым сделкам");
        }

        if (reportType is "full" or "revenue-monthly")
        {
            var monthlyRows = await BuildMonthlyRevenueRowsAsync(from, to, cancellationToken);
            var monthlySheet = workbook.AddWorksheet("Выручка по месяцам");
            FillCurrencyWorksheet(monthlySheet, monthlyRows, "Выручка по месяцам");
        }

        if (reportType is "full" or "conversion")
        {
            var conversionRows = await BuildConversionRowsAsync(from, to, cancellationToken);
            var conversionSheet = workbook.AddWorksheet("Конверсия");
            FillWorksheet(conversionSheet, conversionRows, "Конверсия заявка → сделка");
        }

        if (reportType is "full" or "inactive")
        {
            var inactiveRows = await BuildInactiveObjectRowsAsync(from, to, cancellationToken);
            var inactiveSheet = workbook.AddWorksheet("Без активности");
            FillWorksheet(inactiveSheet, inactiveRows, "Объекты без активности по районам");
        }

        if (reportType is "full" or "deal-time")
        {
            var timeRows = await BuildAverageDealTimeRowsAsync(from, to, cancellationToken);
            var timeSheet = workbook.AddWorksheet("Срок сделки");
            FillWorksheet(timeSheet, timeRows, "Среднее время до сделки, дней");
        }

        if (!workbook.Worksheets.Any())
        {
            var summarySheet = workbook.AddWorksheet("Обзор");
            FillSummaryWorksheet(summarySheet, stats);
        }

        using var stream = new MemoryStream();
        workbook.SaveAs(stream);
        return stream.ToArray();
    }

    private async Task<IReadOnlyList<AdminReportRow>> BuildRevenueRowsAsync(DateTime? from, DateTime? to, CancellationToken cancellationToken)
    {
        var rows = await FilterDealsByPeriod(_context.Deals.AsNoTracking(), from, to)
            .AsNoTracking()
            .Where(d => d.Agent != null)
            .GroupBy(d => new { d.Agent!.LastName, d.Agent!.FirstName, d.Agent!.MiddleName })
            .Select(g => new
            {
                g.Key.LastName,
                g.Key.FirstName,
                g.Key.MiddleName,
                Total = g.Sum(d => d.Amount)
            })
            .OrderByDescending(r => r.Total)
            .ToListAsync(cancellationToken);

        return rows
            .Select(r => new AdminReportRow(FullNameFormatter.Combine(r.LastName, r.FirstName, r.MiddleName), (int)Math.Round(r.Total)))
            .ToList();
    }

    private async Task<IReadOnlyList<AdminReportRow>> BuildMonthlyRevenueRowsAsync(DateTime? from, DateTime? to, CancellationToken cancellationToken)
    {
        var rows = await FilterDealsByPeriod(_context.Deals.AsNoTracking(), from, to)
            .AsNoTracking()
            .GroupBy(d => new { d.ClosedAt.Year, d.ClosedAt.Month })
            .Select(g => new { g.Key.Year, g.Key.Month, Total = g.Sum(d => d.Amount) })
            .OrderBy(r => r.Year)
            .ThenBy(r => r.Month)
            .ToListAsync(cancellationToken);

        return rows
            .Select(r => new AdminReportRow($"{r.Month:00}.{r.Year}", (int)Math.Round(r.Total)))
            .ToList();
    }

    private async Task<IReadOnlyList<AdminReportRow>> BuildConversionRowsAsync(DateTime? from, DateTime? to, CancellationToken cancellationToken)
    {
        var interactionQuery = FilterInteractionsByPeriod(_context.Interactions.AsNoTracking().Where(i => i.DeletedAt == null), from, to);
        var dealQuery = FilterDealsByPeriod(_context.Deals.AsNoTracking(), from, to);
        var requests = await interactionQuery.CountAsync(cancellationToken);
        var deals = await dealQuery.CountAsync(cancellationToken);
        var percent = requests == 0 ? 0 : (int)Math.Round((double)deals / requests * 100);

        return new[]
        {
            new AdminReportRow("Заявок", requests),
            new AdminReportRow("Сделок", deals),
            new AdminReportRow("Конверсия, %", percent)
        };
    }

    private async Task<IReadOnlyList<AdminReportRow>> BuildInactiveObjectRowsAsync(DateTime? from, DateTime? to, CancellationToken cancellationToken)
    {
        var query = _context.RealEstates
            .AsNoTracking()
            .Where(r => r.DeletedAt == null && !r.Interactions.Any(i => i.DeletedAt == null));

        var fromDate = NormalizeFrom(from);
        var toDate = NormalizeToExclusive(to);
        if (fromDate.HasValue)
        {
            query = query.Where(r => r.CreatedAt >= fromDate.Value);
        }

        if (toDate.HasValue)
        {
            query = query.Where(r => r.CreatedAt < toDate.Value);
        }

        var rows = await query
            .GroupBy(r => r.House!.District!.Name)
            .Select(g => new AdminReportRow(g.Key ?? "Без района", g.Count()))
            .OrderByDescending(r => r.Value)
            .ToListAsync(cancellationToken);

        return rows;
    }

    private async Task<IReadOnlyList<AdminReportRow>> BuildAverageDealTimeRowsAsync(DateTime? from, DateTime? to, CancellationToken cancellationToken)
    {
        var rows = await FilterDealsByPeriod(_context.Deals.AsNoTracking(), from, to)
            .AsNoTracking()
            .Where(d => d.Interaction != null && d.Agent != null)
            .Select(d => new
            {
                Agent = FullNameFormatter.Combine(d.Agent!.LastName, d.Agent!.FirstName, d.Agent!.MiddleName),
                d.Interaction!.ContactedAt,
                d.ClosedAt
            })
            .ToListAsync(cancellationToken);

        return rows
            .GroupBy(r => r.Agent)
            .Select(g => new AdminReportRow(g.Key, (int)Math.Round(g.Average(x => (x.ClosedAt - x.ContactedAt).TotalDays))))
            .OrderBy(r => r.Value)
            .ToList();
    }

    private static void FillSummaryWorksheet(IXLWorksheet sheet, AdminDashboardStats stats)
    {
        sheet.Cell(1, 1).Value = "Сводка АрхРеестр";
        sheet.Range(1, 1, 1, 2)
            .Merge()
            .Style
            .Font.SetBold()
            .Font.SetFontSize(14)
            .Fill.SetBackgroundColor(XLColor.FromTheme(XLThemeColor.Accent1, 0.2));

        var rows = new (string Name, int Value)[]
        {
            ("Всего объектов", stats.TotalObjects),
            ("Активных объектов", stats.ActiveObjects),
            ("Пользователей", stats.TotalUsers),
            ("Риелторов", stats.Agents),
            ("Клиентов", stats.Clients),
            ("Заявок", stats.TotalInteractions),
            ("Завершённых сделок", stats.ClosedDeals)
        };

        sheet.Cell(3, 1).Value = "Показатель";
        sheet.Cell(3, 2).Value = "Значение";
        sheet.Range(3, 1, 3, 2).Style.Font.SetBold();

        for (var i = 0; i < rows.Length; i++)
        {
            sheet.Cell(4 + i, 1).Value = rows[i].Name;
            sheet.Cell(4 + i, 2).Value = rows[i].Value;
        }

        sheet.Range(3, 1, 3 + rows.Length, 2).CreateTable().Theme = XLTableTheme.TableStyleMedium2;
        sheet.Columns().AdjustToContents();
    }

    private static IQueryable<DataLayer.Models.Deal> FilterDealsByPeriod(IQueryable<DataLayer.Models.Deal> query, DateTime? from, DateTime? to)
    {
        var fromDate = NormalizeFrom(from);
        var toDate = NormalizeToExclusive(to);

        if (fromDate.HasValue)
        {
            query = query.Where(d => d.ClosedAt >= fromDate.Value);
        }

        if (toDate.HasValue)
        {
            query = query.Where(d => d.ClosedAt < toDate.Value);
        }

        return query;
    }

    private static IQueryable<DataLayer.Models.Interaction> FilterInteractionsByPeriod(IQueryable<DataLayer.Models.Interaction> query, DateTime? from, DateTime? to)
    {
        var fromDate = NormalizeFrom(from);
        var toDate = NormalizeToExclusive(to);

        if (fromDate.HasValue)
        {
            query = query.Where(i => i.ContactedAt >= fromDate.Value);
        }

        if (toDate.HasValue)
        {
            query = query.Where(i => i.ContactedAt < toDate.Value);
        }

        return query;
    }

    private static DateTime? NormalizeFrom(DateTime? value) => value?.Date;

    private static DateTime? NormalizeToExclusive(DateTime? value) => value?.Date.AddDays(1);

    /// <summary>
    /// Унифицированное заполнение листа: заголовки и строки значений.
    /// </summary>
    private static void FillWorksheet(IXLWorksheet sheet, IReadOnlyList<AdminReportRow> rows, string title)
    {
        var total = rows.Sum(r => r.Value);

        sheet.Cell(1, 1).Value = title;
        sheet.Range(1, 1, 1, 3)
            .Merge()
            .Style
            .Font.SetBold()
            .Font.SetFontSize(14)
            .Fill.SetBackgroundColor(XLColor.FromTheme(XLThemeColor.Accent1, 0.2));

        sheet.Cell(2, 1).Value = $"Всего записей: {total}";
        sheet.Range(2, 1, 2, 3).Merge();

        const int headerRow = 3;
        sheet.Cell(headerRow, 1).Value = "Категория";
        sheet.Cell(headerRow, 2).Value = "Количество";
        sheet.Cell(headerRow, 3).Value = "Доля";

        var headerRange = sheet.Range(headerRow, 1, headerRow, 3);
        headerRange.Style.Font.SetBold();
        headerRange.Style.Fill.SetBackgroundColor(XLColor.FromTheme(XLThemeColor.Accent1, 0.6));
        headerRange.Style.Font.SetFontColor(XLColor.White);

        var startRow = headerRow + 1;
        for (var index = 0; index < rows.Count; index++)
        {
            var rowNumber = startRow + index;
            var share = total == 0 ? 0 : (double)rows[index].Value / total;

            sheet.Cell(rowNumber, 1).Value = rows[index].Category;
            sheet.Cell(rowNumber, 2).Value = rows[index].Value;
            sheet.Cell(rowNumber, 3).Value = share;
            sheet.Cell(rowNumber, 3).Style.NumberFormat.Format = "0.00%";
        }

        var totalRow = startRow + rows.Count;
        sheet.Cell(totalRow, 1).Value = "Итого";
        sheet.Cell(totalRow, 2).Value = total;
        sheet.Cell(totalRow, 3).Value = total == 0 ? 0 : 1;
        sheet.Cell(totalRow, 3).Style.NumberFormat.Format = "0.00%";

        var tableRange = sheet.Range(headerRow, 1, totalRow, 3);
        var table = tableRange.CreateTable();
        table.Theme = XLTableTheme.TableStyleMedium2;
        table.ShowTotalsRow = false;

        sheet.SheetView.FreezeRows(headerRow);
        sheet.Columns().AdjustToContents();
    }

    private static void FillCurrencyWorksheet(IXLWorksheet sheet, IReadOnlyList<AdminReportRow> rows, string title)
    {
        var total = rows.Sum(r => r.Value);

        sheet.Cell(1, 1).Value = title;
        sheet.Range(1, 1, 1, 3)
            .Merge()
            .Style
            .Font.SetBold()
            .Font.SetFontSize(14)
            .Fill.SetBackgroundColor(XLColor.FromTheme(XLThemeColor.Accent1, 0.2));

        sheet.Cell(2, 1).Value = $"Итого: {total:N0} ₽";
        sheet.Range(2, 1, 2, 3).Merge();

        const int headerRow = 3;
        sheet.Cell(headerRow, 1).Value = "Риелтор";
        sheet.Cell(headerRow, 2).Value = "Выручка";
        sheet.Cell(headerRow, 3).Value = "Доля";

        var headerRange = sheet.Range(headerRow, 1, headerRow, 3);
        headerRange.Style.Font.SetBold();
        headerRange.Style.Fill.SetBackgroundColor(XLColor.FromTheme(XLThemeColor.Accent1, 0.6));
        headerRange.Style.Font.SetFontColor(XLColor.White);

        var startRow = headerRow + 1;
        for (var index = 0; index < rows.Count; index++)
        {
            var rowNumber = startRow + index;
            var share = total == 0 ? 0 : (double)rows[index].Value / total;

            sheet.Cell(rowNumber, 1).Value = rows[index].Category;
            sheet.Cell(rowNumber, 2).Value = rows[index].Value;
            sheet.Cell(rowNumber, 2).Style.NumberFormat.Format = "#,##0 ₽";
            sheet.Cell(rowNumber, 3).Value = share;
            sheet.Cell(rowNumber, 3).Style.NumberFormat.Format = "0.00%";
        }

        var totalRow = startRow + rows.Count;
        sheet.Cell(totalRow, 1).Value = "Итого";
        sheet.Cell(totalRow, 2).Value = total;
        sheet.Cell(totalRow, 2).Style.NumberFormat.Format = "#,##0 ₽";
        sheet.Cell(totalRow, 3).Value = total == 0 ? 0 : 1;
        sheet.Cell(totalRow, 3).Style.NumberFormat.Format = "0.00%";

        sheet.Range(headerRow, 1, totalRow, 3).CreateTable().Theme = XLTableTheme.TableStyleMedium2;
        sheet.SheetView.FreezeRows(headerRow);
        sheet.Columns().AdjustToContents();
    }
}
