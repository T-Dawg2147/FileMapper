using FileMapper.Core.Models;
using System.Xml.Linq;

namespace FileMapper.Core.Parsers;

/// <summary>Parses XML files to extract element and attribute paths.</summary>
public class XmlFileParser : IFileParser
{
    /// <inheritdoc/>
    public Task<IReadOnlyList<string>> ParseFieldsAsync(string filePath,
        IReadOnlyList<FixedWidthColumn>? fixedWidthColumns = null)
    {
        var doc = XDocument.Load(filePath);
        var paths = new List<string>();
        if (doc.Root is not null)
            CollectPaths(doc.Root, string.Empty, paths);
        return Task.FromResult<IReadOnlyList<string>>(paths.Distinct().ToList());
    }

    /// <inheritdoc/>
    public Task<IReadOnlyList<IReadOnlyDictionary<string, string?>>> ReadRecordsAsync(string filePath,
        IReadOnlyList<FixedWidthColumn>? fixedWidthColumns = null)
    {
        var doc = XDocument.Load(filePath);
        var records = new List<IReadOnlyDictionary<string, string?>>();

        if (doc.Root is null)
            return Task.FromResult<IReadOnlyList<IReadOnlyDictionary<string, string?>>>(records);

        // If root has repeating child elements, treat each child as a record; otherwise treat the whole document as one record.
        var children = doc.Root.Elements().ToList();
        var firstChildName = children.FirstOrDefault()?.Name.LocalName;
        bool hasRepeatingChildren = firstChildName is not null
            && children.Count > 1
            && children.All(c => c.Name.LocalName == firstChildName);

        if (hasRepeatingChildren)
        {
            foreach (var child in children)
            {
                var record = new Dictionary<string, string?>();
                CollectValues(child, string.Empty, record);
                records.Add(record);
            }
        }
        else
        {
            var record = new Dictionary<string, string?>();
            CollectValues(doc.Root, string.Empty, record);
            records.Add(record);
        }

        return Task.FromResult<IReadOnlyList<IReadOnlyDictionary<string, string?>>>(records);
    }

    private static void CollectPaths(XElement element, string prefix, List<string> paths)
    {
        var elementPath = string.IsNullOrEmpty(prefix)
            ? element.Name.LocalName
            : $"{prefix}/{element.Name.LocalName}";

        foreach (var attr in element.Attributes())
        {
            paths.Add($"{elementPath}/@{attr.Name.LocalName}");
        }

        var childElements = element.Elements().ToList();
        if (childElements.Count == 0)
        {
            paths.Add(elementPath);
        }
        else
        {
            foreach (var child in childElements)
                CollectPaths(child, elementPath, paths);
        }
    }

    private static void CollectValues(XElement element, string prefix, Dictionary<string, string?> record)
    {
        var elementPath = string.IsNullOrEmpty(prefix)
            ? element.Name.LocalName
            : $"{prefix}/{element.Name.LocalName}";

        foreach (var attr in element.Attributes())
        {
            record[$"{elementPath}/@{attr.Name.LocalName}"] = attr.Value;
        }

        var childElements = element.Elements().ToList();
        if (childElements.Count == 0)
        {
            record[elementPath] = element.Value;
        }
        else
        {
            foreach (var child in childElements)
                CollectValues(child, elementPath, record);
        }
    }
}
