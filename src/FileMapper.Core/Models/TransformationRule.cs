namespace FileMapper.Core.Models;

/// <summary>Transformation rule types that can be applied when converting a field value.</summary>
public enum TransformationType
{
    /// <summary>No transformation; value is copied as-is.</summary>
    None,

    /// <summary>Concatenate this field with another field or a static string.</summary>
    Concatenate,

    /// <summary>Convert the value from one date/time format to another.</summary>
    DateFormat,

    /// <summary>Replace the source value with a static/default string.</summary>
    StaticValue,

    /// <summary>Extract a substring (start index and optional length).</summary>
    Substring,

    /// <summary>Trim leading and trailing whitespace.</summary>
    Trim
}

/// <summary>
/// Describes an optional transformation to apply to a mapped source field before writing to the target.
/// </summary>
public class TransformationRule
{
    /// <summary>Gets or sets the type of transformation.</summary>
    public TransformationType Type { get; set; } = TransformationType.None;

    /// <summary>
    /// Gets or sets the primary parameter for the transformation.
    /// <list type="bullet">
    ///   <item><description>For <see cref="TransformationType.Concatenate"/>: the additional field path or literal to append.</description></item>
    ///   <item><description>For <see cref="TransformationType.DateFormat"/>: the source date format string (e.g., <c>yyyy-MM-dd</c>).</description></item>
    ///   <item><description>For <see cref="TransformationType.StaticValue"/>: the static value to use.</description></item>
    ///   <item><description>For <see cref="TransformationType.Substring"/>: the zero-based start index (as a string).</description></item>
    /// </list>
    /// </summary>
    public string? Parameter1 { get; set; }

    /// <summary>
    /// Gets or sets the secondary parameter for the transformation.
    /// <list type="bullet">
    ///   <item><description>For <see cref="TransformationType.DateFormat"/>: the target date format string.</description></item>
    ///   <item><description>For <see cref="TransformationType.Substring"/>: the optional length (as a string).</description></item>
    /// </list>
    /// </summary>
    public string? Parameter2 { get; set; }
}
