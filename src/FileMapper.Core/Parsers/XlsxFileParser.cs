using ClosedXML.Excel;
using FileMapper.Core.Models;

namespace FileMapper.Core.Parsers;

/// <summary>Parses XLSX files using ClosedXML to extract column headers and records.</summary>
public class XlsxFileParser : IFileParser
{
    /// <inheritdoc/>
    public Task<IReadOnlyList<string>> ParseFieldsAsync(string filePath,
        IReadOnlyList<FixedWidthColumn>? fixedWidthColumns = null)
    {
        using var workbook = new XLWorkbook(filePath);
        var sheet = workbook.Worksheets.First();
        var headers = GetHeaders(sheet);
        return Task.FromResult<IReadOnlyList<string>>(headers);
    }

    /// <inheritdoc/>
    public Task<IReadOnlyList<IReadOnlyDictionary<string, string?>>> ReadRecordsAsync(string filePath,
        IReadOnlyList<FixedWidthColumn>? fixedWidthColumns = null)
    {
        using var workbook = new XLWorkbook(filePath);
        var sheet = workbook.Worksheets.First();
        var headers = GetHeaders(sheet);
        var records = new List<IReadOnlyDictionary<string, string?>>();

        int lastRow = sheet.LastRowUsed()?.RowNumber() ?? 1;
        for (int row = 2; row <= lastRow; row++)
        {
            var record = new Dictionary<string, string?>();
            for (int col = 1; col <= headers.Count; col++)
            {
                var cell = sheet.Cell(row, col);
                record[headers[col - 1]] = cell.IsEmpty() ? null : cell.GetString();
            }
            records.Add(record);
        }

        return Task.FromResult<IReadOnlyList<IReadOnlyDictionary<string, string?>>>(records);
    }

    private static List<string> GetHeaders(IXLWorksheet sheet)
    {
        var headers = new List<string>();
        int lastCol = sheet.LastColumnUsed()?.ColumnNumber() ?? 0;
        for (int col = 1; col <= lastCol; col++)
        {
            var cell = sheet.Cell(1, col);
            headers.Add(cell.IsEmpty() ? $"Column{col}" : cell.GetString());
        }
        return headers;
    }
}
