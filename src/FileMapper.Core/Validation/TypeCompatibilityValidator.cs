namespace FileMapper.Core.Validation;

/// <summary>
/// Checks type compatibility between source and target fields and returns warnings or errors.
/// </summary>
public class TypeCompatibilityValidator
{
    // Numeric types that can be freely converted between each other (possibly with precision loss).
    private static readonly HashSet<string> NumericTypes = new(StringComparer.OrdinalIgnoreCase)
    {
        "int", "integer", "long", "short", "byte",
        "float", "double", "decimal", "number", "numeric"
    };

    // String-like types are compatible with everything (best-effort ToString / Parse).
    private static readonly HashSet<string> StringTypes = new(StringComparer.OrdinalIgnoreCase)
    {
        "string", "text", "varchar", "nvarchar", "char"
    };

    // Boolean types
    private static readonly HashSet<string> BoolTypes = new(StringComparer.OrdinalIgnoreCase)
    {
        "bool", "boolean", "bit"
    };

    // Date/time types
    private static readonly HashSet<string> DateTypes = new(StringComparer.OrdinalIgnoreCase)
    {
        "datetime", "date", "time", "timestamp"
    };

    /// <summary>
    /// Validates the compatibility between <paramref name="sourceType"/> and <paramref name="targetType"/>.
    /// </summary>
    /// <param name="sourceType">The source data-type hint string (case-insensitive). May be <see langword="null"/>.</param>
    /// <param name="targetType">The target data-type hint string (case-insensitive). May be <see langword="null"/>.</param>
    /// <returns>A <see cref="ValidationResult"/> describing the compatibility.</returns>
    public ValidationResult Validate(string? sourceType, string? targetType)
    {
        if (string.IsNullOrWhiteSpace(sourceType) || string.IsNullOrWhiteSpace(targetType))
            return ValidationResult.Ok; // No type info — cannot check

        if (sourceType.Equals(targetType, StringComparison.OrdinalIgnoreCase))
            return ValidationResult.Ok;

        // Source or target is string — always a warning (not error) as ToString/Parse is a best-effort conversion
        if (StringTypes.Contains(sourceType) || StringTypes.Contains(targetType))
        {
            return new ValidationResult
            {
                Severity = ValidationSeverity.Warning,
                Message = $"Mapping from '{sourceType}' to '{targetType}' requires string conversion. " +
                          "Data may be lost or incorrectly formatted.",
                SourceDataType = sourceType,
                TargetDataType = targetType
            };
        }

        // Both numeric — warning about potential precision loss
        if (NumericTypes.Contains(sourceType) && NumericTypes.Contains(targetType))
        {
            return new ValidationResult
            {
                Severity = ValidationSeverity.Warning,
                Message = $"Mapping from '{sourceType}' to '{targetType}' may result in precision loss.",
                SourceDataType = sourceType,
                TargetDataType = targetType
            };
        }

        // Numeric <-> bool — warn
        if ((NumericTypes.Contains(sourceType) && BoolTypes.Contains(targetType)) ||
            (BoolTypes.Contains(sourceType) && NumericTypes.Contains(targetType)))
        {
            return new ValidationResult
            {
                Severity = ValidationSeverity.Warning,
                Message = $"Mapping from '{sourceType}' to '{targetType}' will treat 0 as false and non-zero as true (or vice versa).",
                SourceDataType = sourceType,
                TargetDataType = targetType
            };
        }

        // Date <-> numeric or Date <-> bool — block
        if ((DateTypes.Contains(sourceType) && NumericTypes.Contains(targetType)) ||
            (NumericTypes.Contains(sourceType) && DateTypes.Contains(targetType)) ||
            (DateTypes.Contains(sourceType) && BoolTypes.Contains(targetType)) ||
            (BoolTypes.Contains(sourceType) && DateTypes.Contains(targetType)))
        {
            return new ValidationResult
            {
                Severity = ValidationSeverity.Error,
                Message = $"Conversion from '{sourceType}' to '{targetType}' is not supported.",
                SourceDataType = sourceType,
                TargetDataType = targetType
            };
        }

        // Unknown type combination — issue a warning
        return new ValidationResult
        {
            Severity = ValidationSeverity.Warning,
            Message = $"Unknown compatibility between '{sourceType}' and '{targetType}'. Proceed with caution.",
            SourceDataType = sourceType,
            TargetDataType = targetType
        };
    }
}
