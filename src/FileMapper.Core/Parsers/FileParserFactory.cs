using FileMapper.Core.Models;

namespace FileMapper.Core.Parsers;

/// <summary>
/// Factory that resolves the correct <see cref="IFileParser"/> implementation for a given <see cref="FileType"/>.
/// </summary>
public static class FileParserFactory
{
    /// <summary>Returns the <see cref="IFileParser"/> for the specified <paramref name="fileType"/>.</summary>
    /// <param name="fileType">The file format to parse.</param>
    /// <returns>An <see cref="IFileParser"/> implementation.</returns>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when the file type is not supported.</exception>
    public static IFileParser GetParser(FileType fileType) => fileType switch
    {
        FileType.Json => new JsonFileParser(),
        FileType.Csv => new CsvFileParser(),
        FileType.Xml => new XmlFileParser(),
        FileType.Xlsx => new XlsxFileParser(),
        FileType.FixedWidth => new FixedWidthFileParser(),
        _ => throw new ArgumentOutOfRangeException(nameof(fileType), fileType, "Unsupported file type.")
    };

    /// <summary>
    /// Determines the <see cref="FileType"/> from a file extension.
    /// Returns <see langword="null"/> if the extension cannot be mapped.
    /// </summary>
    /// <param name="filePath">Path to the file.</param>
    /// <returns>The detected <see cref="FileType"/>, or <see langword="null"/>.</returns>
    public static FileType? DetectFromExtension(string filePath)
    {
        var ext = Path.GetExtension(filePath).ToLowerInvariant();
        return ext switch
        {
            ".json" => FileType.Json,
            ".csv" => FileType.Csv,
            ".xml" => FileType.Xml,
            ".xlsx" => FileType.Xlsx,
            ".txt" => FileType.FixedWidth,
            _ => null
        };
    }
}
