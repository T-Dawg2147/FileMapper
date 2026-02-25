using FileMapper.Core.Models;

namespace FileMapper.Core.Writers;

/// <summary>
/// Defines a contract for writing converted records to a target file.
/// Implement this interface for each supported target file format.
/// </summary>
public interface IFileWriter
{
    /// <summary>
    /// Writes the provided records to the specified output file using the given mapping definition.
    /// </summary>
    /// <param name="filePath">Absolute path to the output file (will be created or overwritten).</param>
    /// <param name="records">The converted records to write, each as a dictionary of target field name to value.</param>
    /// <param name="mapping">The mapping definition containing target schema information.</param>
    /// <returns>A task that completes when writing is done.</returns>
    Task WriteAsync(string filePath,
        IReadOnlyList<IReadOnlyDictionary<string, string?>> records,
        MappingDefinition mapping);
}
