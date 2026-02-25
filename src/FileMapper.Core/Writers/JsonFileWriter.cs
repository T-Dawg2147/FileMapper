using FileMapper.Core.Models;
using System.Text;
using System.Text.Json;

namespace FileMapper.Core.Writers;

/// <summary>Writes records to a JSON file.</summary>
public class JsonFileWriter : IFileWriter
{
    /// <inheritdoc/>
    public async Task WriteAsync(string filePath,
        IReadOnlyList<IReadOnlyDictionary<string, string?>> records,
        MappingDefinition mapping)
    {
        var options = new JsonSerializerOptions { WriteIndented = true };
        string json;

        if (records.Count == 1)
        {
            // Single record — write as an object
            json = JsonSerializer.Serialize(records[0], options);
        }
        else
        {
            json = JsonSerializer.Serialize(records, options);
        }

        await File.WriteAllTextAsync(filePath, json, Encoding.UTF8);
    }
}
