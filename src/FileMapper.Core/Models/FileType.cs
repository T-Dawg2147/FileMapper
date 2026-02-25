namespace FileMapper.Core.Models;

/// <summary>Supported file format types.</summary>
public enum FileType
{
    /// <summary>JavaScript Object Notation.</summary>
    Json,

    /// <summary>Comma-Separated Values.</summary>
    Csv,

    /// <summary>Extensible Markup Language.</summary>
    Xml,

    /// <summary>Excel Open XML Spreadsheet.</summary>
    Xlsx,

    /// <summary>Fixed-width text file.</summary>
    FixedWidth
}
