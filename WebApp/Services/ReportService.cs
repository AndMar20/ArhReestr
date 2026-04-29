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
    public async Task<(IReadOnlyList<AdminReportRow> Districts, IReadOnlyList<AdminReportRow> Agents, IReadOnlyList<AdminReportRow> Statuses)> BuildAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            var districtCounts = await _context.RealEstates
                .AsNoTracking()
                .Where(r => r.DeletedAt == null)
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

            var agentQuery = await _context.Interactions
                .AsNoTracking()
                .Where(i => i.DeletedAt == null)
                .GroupBy(i => new { i.Agent!.LastName, i.Agent!.FirstName, i.Agent!.MiddleName })
                .Select(g => new { g.Key.LastName, g.Key.FirstName, g.Key.MiddleName, Count = g.Count() })
                .OrderByDescending(r => r.Count)
                .ToListAsync(cancellationToken);

            var agentRows = agentQuery
                .Select(g => new AdminReportRow(FullNameFormatter.Combine(g.LastName, g.FirstName, g.MiddleName), g.Count))
                .ToList();

            var statusCounts = await _context.Interactions
                .AsNoTracking()
                .Where(i => i.DeletedAt == null)
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

    /// <summary>
    /// Собирает Excel-файл с тремя листами, используя данные из BuildAsync.
    /// </summary>
    public async Task<byte[]> BuildExcelAsync(CancellationToken cancellationToken = default)
    {
        var (districts, agents, statuses) = await BuildAsync(cancellationToken);

        using var workbook = new XLWorkbook();
        var districtSheet = workbook.AddWorksheet("Районы");
        FillWorksheet(districtSheet, districts, "Обращения по районам");

        var agentSheet = workbook.AddWorksheet("Риелторы");
        FillWorksheet(agentSheet, agents, "Активность риелторов");

        var statusSheet = workbook.AddWorksheet("Статусы");
        FillWorksheet(statusSheet, statuses, "Итоги взаимодействий");

        using var stream = new MemoryStream();
        workbook.SaveAs(stream);
        return stream.ToArray();
    }

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
}
