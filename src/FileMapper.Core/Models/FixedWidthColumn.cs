namespace FileMapper.Core.Models;

/// <summary>Defines a fixed-width column for reading or writing fixed-width text files.</summary>
public class FixedWidthColumn
{
    /// <summary>Gets or sets the column name.</summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>Gets or sets the zero-based start position within the record line.</summary>
    public int StartPosition { get; set; }

    /// <summary>Gets or sets the column width in characters.</summary>
    public int Length { get; set; }
}
