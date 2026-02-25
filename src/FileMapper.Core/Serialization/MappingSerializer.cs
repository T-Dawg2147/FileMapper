using FileMapper.Core.Models;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace FileMapper.Core.Serialization;

/// <summary>
/// Serializes and deserializes <see cref="MappingDefinition"/> objects to and from <c>.map.json</c> files.
/// </summary>
public class MappingSerializer
{
    private static readonly JsonSerializerOptions Options = new()
    {
        WriteIndented = true,
        Converters = { new JsonStringEnumConverter() },
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
    };

    /// <summary>Serializes <paramref name="definition"/> and writes it to <paramref name="filePath"/>.</summary>
    /// <param name="definition">The mapping definition to save.</param>
    /// <param name="filePath">The target <c>.map.json</c> file path.</param>
    public async Task SaveAsync(MappingDefinition definition, string filePath)
    {
        var json = JsonSerializer.Serialize(definition, Options);
        await File.WriteAllTextAsync(filePath, json);
    }

    /// <summary>Reads and deserializes a <see cref="MappingDefinition"/> from <paramref name="filePath"/>.</summary>
    /// <param name="filePath">Path to an existing <c>.map.json</c> file.</param>
    /// <returns>The deserialized <see cref="MappingDefinition"/>.</returns>
    /// <exception cref="InvalidOperationException">Thrown if the file cannot be deserialized.</exception>
    public async Task<MappingDefinition> LoadAsync(string filePath)
    {
        var json = await File.ReadAllTextAsync(filePath);
        return JsonSerializer.Deserialize<MappingDefinition>(json, Options)
               ?? throw new InvalidOperationException($"Could not deserialize mapping file: {filePath}");
    }
}
