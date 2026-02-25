namespace FileMapper.Core.Parsers;

/// <summary>
/// Defines a contract for reading a sample file and extracting all available field names/paths.
/// Implement this interface for each supported source file format.
/// </summary>
public interface IFileParser
{
    /// <summary>
    /// Parses the given file and returns a flat list of all field paths found in it.
    /// For JSON/XML these are slash-separated paths (arrays use index notation).
    /// For CSV/XLSX these are column header names.
    /// For Fixed-Width these are the user-defined column names.
    /// </summary>
    /// <param name="filePath">Absolute path to the sample file.</param>
    /// <param name="fixedWidthColumns">
    /// Optional fixed-width column definitions required when parsing a fixed-width text file.
    /// Pass <see langword="null"/> for all other formats.
    /// </param>
    /// <returns>An ordered list of distinct field names/paths.</returns>
    Task<IReadOnlyList<string>> ParseFieldsAsync(string filePath,
        IReadOnlyList<FileMapper.Core.Models.FixedWidthColumn>? fixedWidthColumns = null);

    /// <summary>
    /// Reads the file and returns all records as a list of dictionaries,
    /// where each dictionary maps a field path to its raw string value.
    /// </summary>
    /// <param name="filePath">Absolute path to the source file.</param>
    /// <param name="fixedWidthColumns">
    /// Optional fixed-width column definitions.
    /// </param>
    /// <returns>A list of records.</returns>
    Task<IReadOnlyList<IReadOnlyDictionary<string, string?>>> ReadRecordsAsync(string filePath,
        IReadOnlyList<FileMapper.Core.Models.FixedWidthColumn>? fixedWidthColumns = null);
}
