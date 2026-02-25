using FileMapper.Core.Models;

namespace FileMapper.Core.Conversion;

/// <summary>
/// Applies transformation rules defined in a <see cref="FieldMapping"/> to raw source values.
/// </summary>
public static class TransformationEngine
{
    /// <summary>
    /// Applies the <paramref name="rule"/> to <paramref name="sourceValue"/> and returns the transformed string.
    /// </summary>
    /// <param name="sourceValue">The raw value read from the source file.</param>
    /// <param name="rule">The transformation rule to apply, or <see langword="null"/> for no transformation.</param>
    /// <param name="record">The full source record (needed for Concatenate referencing another field).</param>
    /// <returns>The transformed value string.</returns>
    public static string? Apply(string? sourceValue, TransformationRule? rule,
        IReadOnlyDictionary<string, string?> record)
    {
        if (rule is null || rule.Type == TransformationType.None)
            return sourceValue;

        return rule.Type switch
        {
            TransformationType.Trim => sourceValue?.Trim(),

            TransformationType.StaticValue => rule.Parameter1,

            TransformationType.Concatenate => ApplyConcatenate(sourceValue, rule, record),

            TransformationType.DateFormat => ApplyDateFormat(sourceValue, rule),

            TransformationType.Substring => ApplySubstring(sourceValue, rule),

            _ => sourceValue
        };
    }

    private static string? ApplyConcatenate(string? sourceValue, TransformationRule rule,
        IReadOnlyDictionary<string, string?> record)
    {
        var additional = rule.Parameter1 ?? string.Empty;
        // Check if Parameter1 is a field path reference in the record
        if (record.TryGetValue(additional, out var fieldValue))
            additional = fieldValue ?? string.Empty;

        return (sourceValue ?? string.Empty) + additional;
    }

    private static string? ApplyDateFormat(string? sourceValue, TransformationRule rule)
    {
        if (string.IsNullOrWhiteSpace(sourceValue)) return sourceValue;
        var sourceFormat = rule.Parameter1;
        var targetFormat = rule.Parameter2;

        if (string.IsNullOrWhiteSpace(sourceFormat) || string.IsNullOrWhiteSpace(targetFormat))
            return sourceValue;

        if (DateTimeOffset.TryParseExact(sourceValue, sourceFormat,
            System.Globalization.CultureInfo.InvariantCulture,
            System.Globalization.DateTimeStyles.None, out var dt))
        {
            return dt.ToString(targetFormat, System.Globalization.CultureInfo.InvariantCulture);
        }

        return sourceValue; // best-effort fallback
    }

    private static string? ApplySubstring(string? sourceValue, TransformationRule rule)
    {
        if (string.IsNullOrEmpty(sourceValue)) return sourceValue;

        if (!int.TryParse(rule.Parameter1, out int start)) return sourceValue;
        if (start < 0 || start >= sourceValue.Length) return string.Empty;

        if (!string.IsNullOrWhiteSpace(rule.Parameter2) && int.TryParse(rule.Parameter2, out int length))
        {
            length = Math.Min(length, sourceValue.Length - start);
            return sourceValue.Substring(start, length);
        }

        return sourceValue[start..];
    }
}
