using ClosedXML.Excel;
using FileMapper.Core.Models;

namespace FileMapper.Core.Writers;

/// <summary>Writes records to an XLSX file using ClosedXML.</summary>
public class XlsxFileWriter : IFileWriter
{
    /// <inheritdoc/>
    public Task WriteAsync(string filePath,
        IReadOnlyList<IReadOnlyDictionary<string, string?>> records,
        MappingDefinition mapping)
    {
        var headers = mapping.FieldMappings.Count > 0
            ? mapping.FieldMappings.Select(m => m.TargetName).ToList()
            : (records.Count > 0 ? records[0].Keys.ToList() : new List<string>());

        using var workbook = new XLWorkbook();
        var sheet = workbook.Worksheets.Add("Data");

        // Write header row
        for (int col = 1; col <= headers.Count; col++)
            sheet.Cell(1, col).Value = headers[col - 1];

        // Write data rows
        for (int row = 0; row < records.Count; row++)
        {
            for (int col = 1; col <= headers.Count; col++)
            {
                records[row].TryGetValue(headers[col - 1], out var value);
                sheet.Cell(row + 2, col).Value = value ?? string.Empty;
            }
        }

        workbook.SaveAs(filePath);
        return Task.CompletedTask;
    }
}
