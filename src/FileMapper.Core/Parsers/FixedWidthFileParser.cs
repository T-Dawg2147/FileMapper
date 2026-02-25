using FileMapper.Core.Models;

namespace FileMapper.Core.Parsers;

/// <summary>Parses fixed-width text files using user-defined column definitions.</summary>
public class FixedWidthFileParser : IFileParser
{
    /// <inheritdoc/>
    public Task<IReadOnlyList<string>> ParseFieldsAsync(string filePath,
        IReadOnlyList<FixedWidthColumn>? fixedWidthColumns = null)
    {
        if (fixedWidthColumns is null || fixedWidthColumns.Count == 0)
            return Task.FromResult<IReadOnlyList<string>>(Array.Empty<string>());

        return Task.FromResult<IReadOnlyList<string>>(
            fixedWidthColumns.Select(c => c.Name).ToList());
    }

    /// <inheritdoc/>
    public async Task<IReadOnlyList<IReadOnlyDictionary<string, string?>>> ReadRecordsAsync(string filePath,
        IReadOnlyList<FixedWidthColumn>? fixedWidthColumns = null)
    {
        if (fixedWidthColumns is null || fixedWidthColumns.Count == 0)
            return Array.Empty<IReadOnlyDictionary<string, string?>>();

        var records = new List<IReadOnlyDictionary<string, string?>>();
        foreach (var line in await File.ReadAllLinesAsync(filePath))
        {
            if (string.IsNullOrWhiteSpace(line)) continue;
            var record = new Dictionary<string, string?>();
            foreach (var col in fixedWidthColumns)
            {
                if (col.StartPosition < line.Length)
                {
                    int available = Math.Min(col.Length, line.Length - col.StartPosition);
                    record[col.Name] = line.Substring(col.StartPosition, available).TrimEnd();
                }
                else
                {
                    record[col.Name] = null;
                }
            }
            records.Add(record);
        }
        return records;
    }
}
