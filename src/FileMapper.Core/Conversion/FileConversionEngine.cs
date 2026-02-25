using FileMapper.Core.Models;
using FileMapper.Core.Parsers;
using FileMapper.Core.Writers;

namespace FileMapper.Core.Conversion;

/// <summary>
/// Converts a source file to a target file according to a <see cref="MappingDefinition"/>.
/// Used by both the UI (preview) and the Converter console application.
/// </summary>
public class FileConversionEngine
{
    /// <summary>
    /// Converts <paramref name="sourceFilePath"/> to <paramref name="targetFilePath"/> using <paramref name="mapping"/>.
    /// </summary>
    /// <param name="sourceFilePath">Path to the input file.</param>
    /// <param name="targetFilePath">Path where the output file will be written.</param>
    /// <param name="mapping">The mapping definition describing how fields map from source to target.</param>
    /// <param name="onWarning">
    /// Optional callback invoked for each per-record conversion warning.
    /// Parameters: record index (0-based), field target name, warning message.
    /// </param>
    /// <returns>The number of records successfully converted.</returns>
    public async Task<int> ConvertAsync(
        string sourceFilePath,
        string targetFilePath,
        MappingDefinition mapping,
        Action<int, string, string>? onWarning = null)
    {
        var parser = FileParserFactory.GetParser(mapping.SourceType);
        var records = await parser.ReadRecordsAsync(sourceFilePath, mapping.SourceFixedWidthColumns);

        var converted = new List<IReadOnlyDictionary<string, string?>>(records.Count);
        int recordIndex = 0;

        foreach (var sourceRecord in records)
        {
            var targetRecord = new Dictionary<string, string?>();

            foreach (var fieldMapping in mapping.FieldMappings)
            {
                sourceRecord.TryGetValue(fieldMapping.SourcePath, out var rawValue);

                string? transformedValue;
                try
                {
                    transformedValue = TransformationEngine.Apply(rawValue, fieldMapping.Transformation, sourceRecord);
                }
                catch (Exception ex)
                {
                    onWarning?.Invoke(recordIndex, fieldMapping.TargetName,
                        $"Transformation failed: {ex.Message}. Using raw value.");
                    transformedValue = rawValue;
                }

                // Warn about type coercion if the user previously acknowledged
                if (fieldMapping.TypeWarningAcknowledged &&
                    !string.IsNullOrWhiteSpace(fieldMapping.SourceDataType) &&
                    !string.IsNullOrWhiteSpace(fieldMapping.TargetDataType))
                {
                    onWarning?.Invoke(recordIndex, fieldMapping.TargetName,
                        $"Best-effort conversion from '{fieldMapping.SourceDataType}' to '{fieldMapping.TargetDataType}'.");
                }

                targetRecord[fieldMapping.TargetName] = transformedValue;
            }

            converted.Add(targetRecord);
            recordIndex++;
        }

        var writer = FileWriterFactory.GetWriter(mapping.TargetType);
        await writer.WriteAsync(targetFilePath, converted, mapping);

        return converted.Count;
    }
}
