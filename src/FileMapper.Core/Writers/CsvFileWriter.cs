using CsvHelper;
using CsvHelper.Configuration;
using FileMapper.Core.Models;
using System.Globalization;

namespace FileMapper.Core.Writers;

/// <summary>Writes records to a CSV file using CsvHelper.</summary>
public class CsvFileWriter : IFileWriter
{
    /// <inheritdoc/>
    public async Task WriteAsync(string filePath,
        IReadOnlyList<IReadOnlyDictionary<string, string?>> records,
        MappingDefinition mapping)
    {
        await using var writer = new StreamWriter(filePath);
        await using var csv = new CsvWriter(writer, new CsvConfiguration(CultureInfo.InvariantCulture));

        // Determine headers from mapping target names or first record
        var headers = mapping.FieldMappings.Count > 0
            ? mapping.FieldMappings.Select(m => m.TargetName).ToList()
            : (records.Count > 0 ? records[0].Keys.ToList() : new List<string>());

        foreach (var header in headers)
            csv.WriteField(header);
        await csv.NextRecordAsync();

        foreach (var record in records)
        {
            foreach (var header in headers)
            {
                record.TryGetValue(header, out var value);
                csv.WriteField(value);
            }
            await csv.NextRecordAsync();
        }
    }
}
