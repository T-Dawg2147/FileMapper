namespace FileMapper.Core.Models;

/// <summary>Configuration that controls how hierarchical field paths are flattened to simple names.</summary>
public class FlatteningConfiguration
{
    /// <summary>Gets or sets a value indicating whether flattening is enabled.</summary>
    public bool Enabled { get; set; }

    /// <summary>
    /// Gets or sets the common ancestor prefix that is stripped from all field paths during flattening.
    /// Determined automatically as the longest common path shared by all source fields, but may be overridden.
    /// </summary>
    public string? CommonPrefix { get; set; }
}
