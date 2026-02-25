using System.Text.Json;
using FileMapper.Core.Models;

namespace FileMapper.Core.Parsers;

/// <summary>Parses JSON files to extract field paths and record data.</summary>
public class JsonFileParser : IFileParser
{
    /// <inheritdoc/>
    public Task<IReadOnlyList<string>> ParseFieldsAsync(string filePath,
        IReadOnlyList<FixedWidthColumn>? fixedWidthColumns = null)
    {
        var json = File.ReadAllText(filePath);
        using var doc = JsonDocument.Parse(json);
        var paths = new List<string>();
        CollectPaths(doc.RootElement, string.Empty, paths);
        return Task.FromResult<IReadOnlyList<string>>(paths.Distinct().ToList());
    }

    /// <inheritdoc/>
    public Task<IReadOnlyList<IReadOnlyDictionary<string, string?>>> ReadRecordsAsync(string filePath,
        IReadOnlyList<FixedWidthColumn>? fixedWidthColumns = null)
    {
        var json = File.ReadAllText(filePath);
        using var doc = JsonDocument.Parse(json);
        var records = new List<IReadOnlyDictionary<string, string?>>();

        if (doc.RootElement.ValueKind == JsonValueKind.Array)
        {
            foreach (var element in doc.RootElement.EnumerateArray())
            {
                var record = new Dictionary<string, string?>();
                CollectValues(element, string.Empty, record);
                records.Add(record);
            }
        }
        else
        {
            var record = new Dictionary<string, string?>();
            CollectValues(doc.RootElement, string.Empty, record);
            records.Add(record);
        }

        return Task.FromResult<IReadOnlyList<IReadOnlyDictionary<string, string?>>>(records);
    }

    private static void CollectPaths(JsonElement element, string prefix, List<string> paths)
    {
        switch (element.ValueKind)
        {
            case JsonValueKind.Object:
                foreach (var prop in element.EnumerateObject())
                {
                    var path = string.IsNullOrEmpty(prefix) ? prop.Name : $"{prefix}/{prop.Name}";
                    CollectPaths(prop.Value, path, paths);
                }
                break;

            case JsonValueKind.Array:
                int index = 0;
                foreach (var item in element.EnumerateArray())
                {
                    var path = $"{prefix}/{index}";
                    CollectPaths(item, path, paths);
                    index++;
                }
                break;

            default:
                if (!string.IsNullOrEmpty(prefix))
                    paths.Add(prefix);
                break;
        }
    }

    private static void CollectValues(JsonElement element, string prefix, Dictionary<string, string?> record)
    {
        switch (element.ValueKind)
        {
            case JsonValueKind.Object:
                foreach (var prop in element.EnumerateObject())
                {
                    var path = string.IsNullOrEmpty(prefix) ? prop.Name : $"{prefix}/{prop.Name}";
                    CollectValues(prop.Value, path, record);
                }
                break;

            case JsonValueKind.Array:
                int index = 0;
                foreach (var item in element.EnumerateArray())
                {
                    CollectValues(item, $"{prefix}/{index}", record);
                    index++;
                }
                break;

            default:
                if (!string.IsNullOrEmpty(prefix))
                    record[prefix] = element.ValueKind == JsonValueKind.Null ? null : element.ToString();
                break;
        }
    }
}
