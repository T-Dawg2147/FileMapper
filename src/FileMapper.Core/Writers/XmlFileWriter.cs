using FileMapper.Core.Models;
using System.Text;
using System.Xml.Linq;

namespace FileMapper.Core.Writers;

/// <summary>Writes records to an XML file.</summary>
public class XmlFileWriter : IFileWriter
{
    /// <inheritdoc/>
    public async Task WriteAsync(string filePath,
        IReadOnlyList<IReadOnlyDictionary<string, string?>> records,
        MappingDefinition mapping)
    {
        var root = new XElement("Records");

        foreach (var record in records)
        {
            var recordElement = new XElement("Record");
            foreach (var (key, value) in record)
            {
                // Sanitise key to a valid XML element name
                var elementName = SanitiseElementName(key);
                recordElement.Add(new XElement(elementName, value ?? string.Empty));
            }
            root.Add(recordElement);
        }

        var doc = new XDocument(new XDeclaration("1.0", "utf-8", null), root);
        await using var stream = new FileStream(filePath, FileMode.Create, FileAccess.Write);
        await Task.Run(() => doc.Save(stream));
    }

    private static string SanitiseElementName(string name)
    {
        // Replace path separators and spaces with underscores
        var sb = new StringBuilder();
        foreach (var c in name)
        {
            sb.Append(char.IsLetterOrDigit(c) || c == '_' || c == '-' || c == '.' ? c : '_');
        }
        var result = sb.ToString();
        // XML element names cannot start with a digit
        if (result.Length > 0 && char.IsDigit(result[0]))
            result = "_" + result;
        return string.IsNullOrEmpty(result) ? "Field" : result;
    }
}
