using System.Text.Json.Serialization;

namespace FileMapper.Converter;

/// <summary>
/// Configuration model read from <c>converter-config.json</c> (or a path supplied on the command line).
/// </summary>
public class ConverterConfig
{
    /// <summary>Gets or sets the folder path that contains <c>.map.json</c> mapping files.</summary>
    public string MappingsFolderPath { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the path to a single source input file, or a folder path for batch processing.
    /// </summary>
    public string SourcePath { get; set; } = string.Empty;

    /// <summary>Gets or sets the output file path or folder for converted file(s).</summary>
    public string OutputPath { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the optional log file path.
    /// Defaults to a <c>logs/</c> folder next to the executable when not specified.
    /// </summary>
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? LogFilePath { get; set; }
}
