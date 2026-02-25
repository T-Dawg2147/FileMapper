namespace FileMapper.UI;

/// <summary>
/// Persisted settings for the FileMapper UI, stored in <c>ui-settings.json</c>.
/// </summary>
public class UiSettings
{
    /// <summary>Gets or sets the default folder path for saving mapping files.</summary>
    public string DefaultMappingOutputFolder { get; set; } = string.Empty;
}
