namespace FileMapper.Core.Flattening;

/// <summary>
/// Provides flattening logic for hierarchical field paths (from JSON or XML sources).
/// The algorithm strips the longest common ancestor prefix and ensures uniqueness of resulting names.
/// </summary>
public static class PathFlattener
{
    /// <summary>
    /// Determines the longest common ancestor path prefix shared by all supplied paths.
    /// </summary>
    /// <param name="paths">A collection of slash-separated field paths.</param>
    /// <returns>
    /// The common prefix string (including the trailing slash), or an empty string if there is no common prefix.
    /// </returns>
    public static string GetCommonPrefix(IReadOnlyList<string> paths)
    {
        if (paths is null || paths.Count == 0)
            return string.Empty;

        var segmentLists = paths
            .Select(p => p.Split('/').ToList())
            .ToList();

        // Walk segment by segment, taking segments that are identical across all paths
        int minSegments = segmentLists.Min(s => s.Count);
        var commonSegments = new List<string>();

        for (int i = 0; i < minSegments - 1; i++) // -1 so we always keep at least the leaf
        {
            var candidate = segmentLists[0][i];
            if (segmentLists.All(s => s[i] == candidate))
                commonSegments.Add(candidate);
            else
                break;
        }

        return commonSegments.Count > 0 ? string.Join("/", commonSegments) + "/" : string.Empty;
    }

    /// <summary>
    /// Flattens a list of hierarchical paths by stripping the longest common prefix and
    /// retaining enough parent segments to ensure uniqueness.
    /// </summary>
    /// <param name="paths">The original slash-separated field paths.</param>
    /// <returns>
    /// A dictionary mapping each original path to its flattened (unique) name.
    /// </returns>
    public static IReadOnlyDictionary<string, string> Flatten(IReadOnlyList<string> paths)
    {
        if (paths is null || paths.Count == 0)
            return new Dictionary<string, string>();

        var commonPrefix = GetCommonPrefix(paths);
        var stripped = paths
            .Select(p => p.StartsWith(commonPrefix, StringComparison.Ordinal)
                ? p[commonPrefix.Length..]
                : p)
            .ToList();

        // Attempt to use only the leaf (last segment). If duplicates arise, add parent levels.
        return ResolveUnique(paths, stripped);
    }

    private static IReadOnlyDictionary<string, string> ResolveUnique(
        IReadOnlyList<string> originals,
        IReadOnlyList<string> stripped)
    {
        // Split each stripped path into segments
        var segmentLists = stripped.Select(s => s.Split('/')).ToList();
        int maxDepth = segmentLists.Max(s => s.Length);

        // Start with 1 trailing segment and increase until all names are unique
        for (int depth = 1; depth <= maxDepth; depth++)
        {
            var names = segmentLists
                .Select(s => string.Join("/", s.Skip(Math.Max(0, s.Length - depth))))
                .ToList();

            if (names.Count == names.Distinct().Count())
            {
                // Unique — build result dictionary
                var result = new Dictionary<string, string>(originals.Count);
                for (int i = 0; i < originals.Count; i++)
                    result[originals[i]] = names[i];
                return result;
            }
        }

        // Fallback: return the full stripped path
        var fallback = new Dictionary<string, string>(originals.Count);
        for (int i = 0; i < originals.Count; i++)
            fallback[originals[i]] = stripped[i];
        return fallback;
    }
}
