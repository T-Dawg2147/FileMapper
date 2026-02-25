using CsvHelper;
using CsvHelper.Configuration;
using FileMapper.Core.Models;
using System.Globalization;

namespace FileMapper.Core.Parsers;

/// <summary>Parses CSV files using CsvHelper to extract column headers and records.</summary>
public class CsvFileParser : IFileParser
{
    /// <inheritdoc/>
    public Task<IReadOnlyList<string>> ParseFieldsAsync(string filePath,
        IReadOnlyList<FixedWidthColumn>? fixedWidthColumns = null)
    {
        using var reader = new StreamReader(filePath);
        using var csv = new CsvReader(reader, new CsvConfiguration(CultureInfo.InvariantCulture)
        {
            HasHeaderRecord = true
        });
        csv.Read();
        csv.ReadHeader();
        var headers = csv.HeaderRecord ?? Array.Empty<string>();
        return Task.FromResult<IReadOnlyList<string>>(headers.ToList());
    }

    /// <inheritdoc/>
    public async Task<IReadOnlyList<IReadOnlyDictionary<string, string?>>> ReadRecordsAsync(string filePath,
        IReadOnlyList<FixedWidthColumn>? fixedWidthColumns = null)
    {
        var results = new List<IReadOnlyDictionary<string, string?>>();
        using var reader = new StreamReader(filePath);
        using var csv = new CsvReader(reader, new CsvConfiguration(CultureInfo.InvariantCulture)
        {
            HasHeaderRecord = true
        });

        await csv.ReadAsync();
        csv.ReadHeader();
        var headers = csv.HeaderRecord ?? Array.Empty<string>();

        while (await csv.ReadAsync())
        {
            var record = new Dictionary<string, string?>();
            foreach (var header in headers)
            {
                record[header] = csv.GetField(header);
            }
            results.Add(record);
        }

        return results;
    }
}
