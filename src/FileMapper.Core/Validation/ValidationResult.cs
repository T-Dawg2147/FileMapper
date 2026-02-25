namespace FileMapper.Core.Validation;

/// <summary>Describes the severity of a validation result.</summary>
public enum ValidationSeverity
{
    /// <summary>Informational message only.</summary>
    Info,

    /// <summary>The mapping may produce unexpected results but is allowed after acknowledgement.</summary>
    Warning,

    /// <summary>The mapping is impossible and must be blocked.</summary>
    Error
}

/// <summary>Represents the outcome of a type-compatibility check between a source and target field.</summary>
public class ValidationResult
{
    /// <summary>Gets the severity of this result.</summary>
    public ValidationSeverity Severity { get; init; }

    /// <summary>Gets the human-readable message explaining the issue.</summary>
    public string Message { get; init; } = string.Empty;

    /// <summary>Gets the source data-type string that was checked (may be <see langword="null"/>).</summary>
    public string? SourceDataType { get; init; }

    /// <summary>Gets the target data-type string that was checked (may be <see langword="null"/>).</summary>
    public string? TargetDataType { get; init; }

    /// <summary>
    /// Creates a successful (no-warning) validation result.
    /// </summary>
    public static ValidationResult Ok => new()
    {
        Severity = ValidationSeverity.Info,
        Message = "Types are compatible."
    };
}
