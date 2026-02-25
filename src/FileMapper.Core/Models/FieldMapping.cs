namespace FileMapper.Core.Models;

/// <summary>Represents a single source-to-target field mapping within a <see cref="MappingDefinition"/>.</summary>
public class FieldMapping
{
    /// <summary>Gets or sets the source field path (e.g., <c>PurchaseOrder/PurchaseOrderHeader/Supplier/Name</c>).</summary>
    public string SourcePath { get; set; } = string.Empty;

    /// <summary>Gets or sets the target field name or path (e.g., a CSV column header or XML element path).</summary>
    public string TargetName { get; set; } = string.Empty;

    /// <summary>Gets or sets the optional data-type hint for the source field (e.g., "string", "int", "decimal", "DateTime").</summary>
    public string? SourceDataType { get; set; }

    /// <summary>Gets or sets the optional data-type hint for the target field.</summary>
    public string? TargetDataType { get; set; }

    /// <summary>Gets or sets an optional transformation rule applied when copying the source value to the target.</summary>
    public TransformationRule? Transformation { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether the user has acknowledged a type-compatibility warning for this mapping.
    /// When <see langword="true"/>, the converter will attempt a best-effort conversion and log a warning.
    /// </summary>
    public bool TypeWarningAcknowledged { get; set; }
}
