using FileMapper.Core.Models;

namespace FileMapper.Core.Writers;

/// <summary>
/// Factory that resolves the correct <see cref="IFileWriter"/> implementation for a given <see cref="FileType"/>.
/// </summary>
public static class FileWriterFactory
{
    /// <summary>Returns the <see cref="IFileWriter"/> for the specified <paramref name="fileType"/>.</summary>
    /// <param name="fileType">The target file format.</param>
    /// <returns>An <see cref="IFileWriter"/> implementation.</returns>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when the file type is not supported.</exception>
    public static IFileWriter GetWriter(FileType fileType) => fileType switch
    {
        FileType.Json => new JsonFileWriter(),
        FileType.Csv => new CsvFileWriter(),
        FileType.Xml => new XmlFileWriter(),
        FileType.Xlsx => new XlsxFileWriter(),
        FileType.FixedWidth => new FixedWidthFileWriter(),
        _ => throw new ArgumentOutOfRangeException(nameof(fileType), fileType, "Unsupported file type.")
    };
}
