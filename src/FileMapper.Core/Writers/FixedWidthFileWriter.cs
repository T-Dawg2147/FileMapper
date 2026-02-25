using FileMapper.Core.Models;
using System.Text;

namespace FileMapper.Core.Writers;

/// <summary>Writes records to a fixed-width text file.</summary>
public class FixedWidthFileWriter : IFileWriter
{
    /// <inheritdoc/>
    public async Task WriteAsync(string filePath,
        IReadOnlyList<IReadOnlyDictionary<string, string?>> records,
        MappingDefinition mapping)
    {
        var columns = mapping.TargetFixedWidthColumns;
        if (columns is null || columns.Count == 0)
            throw new InvalidOperationException("TargetFixedWidthColumns must be defined when writing fixed-width files.");

        // Determine total line length
        int lineLength = columns.Max(c => c.StartPosition + c.Length);

        var lines = new List<string>(records.Count);
        foreach (var record in records)
        {
            var line = new char[lineLength];
            Array.Fill(line, ' ');

            foreach (var col in columns)
            {
                record.TryGetValue(col.Name, out var value);
                var text = value ?? string.Empty;
                int copyLen = Math.Min(col.Length, text.Length);
                for (int i = 0; i < copyLen; i++)
                    line[col.StartPosition + i] = text[i];
            }

            lines.Add(new string(line));
        }

        await File.WriteAllLinesAsync(filePath, lines, Encoding.UTF8);
    }
}
