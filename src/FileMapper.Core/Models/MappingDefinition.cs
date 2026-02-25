namespace FileMapper.Core.Models;

/// <summary>
/// The top-level mapping definition that is serialised to a <c>.map.json</c> file.
/// Both the Mapper UI and the Converter application use this model.
/// </summary>
public class MappingDefinition
{
    /// <summary>Gets or sets a human-readable name for this mapping.</summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>Gets or sets the source file type.</summary>
    public FileType SourceType { get; set; }

    /// <summary>Gets or sets the target file type.</summary>
    public FileType TargetType { get; set; }

    /// <summary>Gets or sets the collection of field-level mappings.</summary>
    public List<FieldMapping> FieldMappings { get; set; } = new();

    /// <summary>
    /// Gets or sets the flattening configuration applied to hierarchical source files (JSON, XML).
    /// Ignored when the source type is CSV, XLSX, or FixedWidth.
    /// </summary>
    public FlatteningConfiguration Flattening { get; set; } = new();

    /// <summary>
    /// Gets or sets the fixed-width column definitions used when the source is <see cref="FileType.FixedWidth"/>.
    /// </summary>
    public List<FixedWidthColumn>? SourceFixedWidthColumns { get; set; }

    /// <summary>
    /// Gets or sets the fixed-width column definitions used when the target is <see cref="FileType.FixedWidth"/>.
    /// </summary>
    public List<FixedWidthColumn>? TargetFixedWidthColumns { get; set; }
}
