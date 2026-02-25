using FileMapper.Core.Flattening;
using FileMapper.Core.Models;
using FileMapper.Core.Parsers;
using FileMapper.Core.Serialization;
using FileMapper.Core.Validation;
using Microsoft.Win32;
using System.Collections.ObjectModel;
using System.IO;
using System.Text.Json;
using System.Windows;

namespace FileMapper.UI.ViewModels;

/// <summary>Main ViewModel for the mapping designer window.</summary>
public class MainViewModel : ViewModelBase
{
    private readonly MappingSerializer _serializer = new();
    private readonly TypeCompatibilityValidator _validator = new();

    private static readonly string UiSettingsPath =
        Path.Combine(AppContext.BaseDirectory, "ui-settings.json");

    private string _mappingName = "New Mapping";
    private FileType _sourceType = FileType.Json;
    private FileType _targetType = FileType.Csv;
    private string _sourceFilePath = string.Empty;
    private string _targetFilePath = string.Empty;
    private string? _currentMapFilePath;
    private string _statusMessage = "Ready.";
    private bool _flattenEnabled;
    private string? _selectedSourceField;
    private string? _selectedTargetField;
    private string _expectedFileName = string.Empty;
    private bool _fileNameIsPrefix;

    private UiSettings _uiSettings = new();

    /// <summary>Gets or sets the mapping name.</summary>
    public string MappingName
    {
        get => _mappingName;
        set => SetProperty(ref _mappingName, value);
    }

    /// <summary>Gets or sets the source file type.</summary>
    public FileType SourceType
    {
        get => _sourceType;
        set => SetProperty(ref _sourceType, value);
    }

    /// <summary>Gets or sets the target file type.</summary>
    public FileType TargetType
    {
        get => _targetType;
        set => SetProperty(ref _targetType, value);
    }

    /// <summary>Gets or sets the path to the source sample file.</summary>
    public string SourceFilePath
    {
        get => _sourceFilePath;
        set => SetProperty(ref _sourceFilePath, value);
    }

    /// <summary>Gets or sets the path to the target sample file.</summary>
    public string TargetFilePath
    {
        get => _targetFilePath;
        set => SetProperty(ref _targetFilePath, value);
    }

    /// <summary>Gets or sets the status bar message.</summary>
    public string StatusMessage
    {
        get => _statusMessage;
        set => SetProperty(ref _statusMessage, value);
    }

    /// <summary>Gets or sets whether flattening is enabled for hierarchical sources.</summary>
    public bool FlattenEnabled
    {
        get => _flattenEnabled;
        set => SetProperty(ref _flattenEnabled, value);
    }

    /// <summary>Gets or sets the currently selected source field for mapping.</summary>
    public string? SelectedSourceField
    {
        get => _selectedSourceField;
        set => SetProperty(ref _selectedSourceField, value);
    }

    /// <summary>Gets or sets the currently selected target field for mapping.</summary>
    public string? SelectedTargetField
    {
        get => _selectedTargetField;
        set => SetProperty(ref _selectedTargetField, value);
    }

    /// <summary>
    /// Gets or sets the expected source file name (or static prefix) used by the Converter
    /// to match incoming files to this mapping.
    /// </summary>
    public string ExpectedFileName
    {
        get => _expectedFileName;
        set => SetProperty(ref _expectedFileName, value);
    }

    /// <summary>
    /// When <see langword="true"/>, <see cref="ExpectedFileName"/> is treated as a prefix;
    /// any file whose name starts with that value will use this mapping.
    /// </summary>
    public bool FileNameIsPrefix
    {
        get => _fileNameIsPrefix;
        set => SetProperty(ref _fileNameIsPrefix, value);
    }

    /// <summary>Gets the list of source field paths loaded from the sample file.</summary>
    public ObservableCollection<string> SourceFields { get; } = new();

    /// <summary>Gets the list of target field paths loaded from the sample file.</summary>
    public ObservableCollection<string> TargetFields { get; } = new();

    /// <summary>Gets the current list of field mappings.</summary>
    public ObservableCollection<FieldMappingViewModel> FieldMappings { get; } = new();

    /// <summary>Gets the available file types for the combo boxes.</summary>
    public IEnumerable<FileType> FileTypes { get; } = Enum.GetValues<FileType>();

    // Commands
    /// <summary>Opens a source sample file.</summary>
    public RelayCommand BrowseSourceCommand { get; }

    /// <summary>Opens a target sample file.</summary>
    public RelayCommand BrowseTargetCommand { get; }

    /// <summary>Loads field names from the source file.</summary>
    public RelayCommand LoadSourceFieldsCommand { get; }

    /// <summary>Loads field names from the target file.</summary>
    public RelayCommand LoadTargetFieldsCommand { get; }

    /// <summary>Adds a new mapping from the selected source and target fields.</summary>
    public RelayCommand AddMappingCommand { get; }

    /// <summary>Removes a selected mapping from the grid.</summary>
    public RelayCommand RemoveMappingCommand { get; }

    /// <summary>Saves the current mapping to a <c>.map.json</c> file.</summary>
    public RelayCommand SaveMappingCommand { get; }

    /// <summary>Loads an existing <c>.map.json</c> file.</summary>
    public RelayCommand LoadMappingCommand { get; }

    /// <summary>Opens the settings dialog to configure default mapping output folder.</summary>
    public RelayCommand OpenSettingsCommand { get; }

    /// <summary>Initialises a new <see cref="MainViewModel"/>.</summary>
    public MainViewModel()
    {
        BrowseSourceCommand = new RelayCommand(_ => BrowseFile(isSource: true));
        BrowseTargetCommand = new RelayCommand(_ => BrowseFile(isSource: false));
        LoadSourceFieldsCommand = new RelayCommand(async _ => await LoadFieldsAsync(isSource: true));
        LoadTargetFieldsCommand = new RelayCommand(async _ => await LoadFieldsAsync(isSource: false));
        AddMappingCommand = new RelayCommand(_ => AddMapping(),
            _ => SelectedSourceField is not null && SelectedTargetField is not null);
        RemoveMappingCommand = new RelayCommand(p => RemoveMapping(p as FieldMappingViewModel),
            p => p is FieldMappingViewModel);
        SaveMappingCommand = new RelayCommand(async _ => await SaveMappingAsync());
        LoadMappingCommand = new RelayCommand(async _ => await LoadMappingAsync());
        OpenSettingsCommand = new RelayCommand(_ => OpenSettings());

        LoadUiSettings();
    }

    private void BrowseFile(bool isSource)
    {
        var dialog = new OpenFileDialog
        {
            Filter = "All Supported Files|*.json;*.csv;*.xml;*.xlsx;*.txt|" +
                     "JSON|*.json|CSV|*.csv|XML|*.xml|Excel|*.xlsx|Fixed-Width|*.txt|All|*.*"
        };

        if (dialog.ShowDialog() != true) return;

        var path = dialog.FileName;
        var detected = FileParserFactory.DetectFromExtension(path);

        if (isSource)
        {
            SourceFilePath = path;
            if (detected.HasValue) SourceType = detected.Value;
        }
        else
        {
            TargetFilePath = path;
            if (detected.HasValue) TargetType = detected.Value;
        }
    }

    private async Task LoadFieldsAsync(bool isSource)
    {
        var filePath = isSource ? SourceFilePath : TargetFilePath;
        var fileType = isSource ? SourceType : TargetType;

        if (string.IsNullOrWhiteSpace(filePath) || !File.Exists(filePath))
        {
            StatusMessage = $"File not found: {filePath}";
            return;
        }

        try
        {
            var parser = FileParserFactory.GetParser(fileType);
            var fields = await parser.ParseFieldsAsync(filePath);

            var collection = isSource ? SourceFields : TargetFields;
            collection.Clear();

            if (isSource && FlattenEnabled &&
                (fileType == FileType.Json || fileType == FileType.Xml))
            {
                var flattened = PathFlattener.Flatten(fields.ToList());
                foreach (var (_, flat) in flattened)
                    collection.Add(flat);
            }
            else
            {
                foreach (var f in fields)
                    collection.Add(f);
            }

            // Ask about flattening for hierarchical sources
            if (isSource && (fileType == FileType.Json || fileType == FileType.Xml) && !FlattenEnabled)
            {
                var answer = MessageBox.Show(
                    "Would you like to flatten nested field names?",
                    "Flatten Fields",
                    MessageBoxButton.YesNo,
                    MessageBoxImage.Question);

                if (answer == MessageBoxResult.Yes)
                {
                    FlattenEnabled = true;
                    await LoadFieldsAsync(isSource: true);
                    return;
                }
            }

            StatusMessage = $"Loaded {collection.Count} field(s) from '{Path.GetFileName(filePath)}'.";
        }
        catch (Exception ex)
        {
            StatusMessage = $"Error loading fields: {ex.Message}";
            MessageBox.Show(ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    private void AddMapping()
    {
        if (SelectedSourceField is null || SelectedTargetField is null) return;

        var vm = new FieldMappingViewModel
        {
            SourcePath = SelectedSourceField,
            TargetName = SelectedTargetField
        };

        // Run type validation
        var result = _validator.Validate(vm.SourceDataType, vm.TargetDataType);
        vm.ValidationSeverity = result.Severity;
        vm.ValidationMessage = result.Message;

        if (result.Severity == ValidationSeverity.Error)
        {
            MessageBox.Show(result.Message, "Incompatible Types", MessageBoxButton.OK, MessageBoxImage.Error);
            return;
        }

        if (result.Severity == ValidationSeverity.Warning)
        {
            var answer = MessageBox.Show(
                $"{result.Message}\n\nDo you want to proceed anyway?",
                "Type Compatibility Warning",
                MessageBoxButton.OKCancel,
                MessageBoxImage.Warning);

            if (answer != MessageBoxResult.OK) return;
            vm.TypeWarningAcknowledged = true;
        }

        FieldMappings.Add(vm);
        StatusMessage = $"Added mapping: {vm.SourcePath} → {vm.TargetName}";
    }

    private void RemoveMapping(FieldMappingViewModel? vm)
    {
        if (vm is null) return;
        FieldMappings.Remove(vm);
        StatusMessage = $"Removed mapping: {vm.SourcePath} → {vm.TargetName}";
    }

    private async Task SaveMappingAsync()
    {
        var dialog = new SaveFileDialog
        {
            Filter = "Mapping Files (*.map.json)|*.map.json|All Files|*.*",
            DefaultExt = ".map.json",
            FileName = _currentMapFilePath ?? MappingName
        };

        if (!string.IsNullOrEmpty(_uiSettings.DefaultMappingOutputFolder) &&
            Directory.Exists(_uiSettings.DefaultMappingOutputFolder))
        {
            dialog.InitialDirectory = _uiSettings.DefaultMappingOutputFolder;
        }

        if (dialog.ShowDialog() != true) return;

        _currentMapFilePath = dialog.FileName;

        var definition = new MappingDefinition
        {
            Name = MappingName,
            SourceType = SourceType,
            TargetType = TargetType,
            FieldMappings = FieldMappings.Select(vm => vm.ToModel()).ToList(),
            Flattening = new FlatteningConfiguration { Enabled = FlattenEnabled },
            ExpectedFileName = string.IsNullOrWhiteSpace(ExpectedFileName) ? null : ExpectedFileName,
            FileNameIsPrefix = FileNameIsPrefix
        };

        try
        {
            await _serializer.SaveAsync(definition, _currentMapFilePath);
            StatusMessage = $"Mapping saved to '{_currentMapFilePath}'.";
        }
        catch (Exception ex)
        {
            MessageBox.Show(ex.Message, "Save Error", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    private async Task LoadMappingAsync()
    {
        var dialog = new OpenFileDialog
        {
            Filter = "Mapping Files (*.map.json)|*.map.json|All Files|*.*"
        };

        if (dialog.ShowDialog() != true) return;

        try
        {
            var definition = await _serializer.LoadAsync(dialog.FileName);
            _currentMapFilePath = dialog.FileName;

            MappingName = definition.Name;
            SourceType = definition.SourceType;
            TargetType = definition.TargetType;
            FlattenEnabled = definition.Flattening?.Enabled ?? false;
            ExpectedFileName = definition.ExpectedFileName ?? string.Empty;
            FileNameIsPrefix = definition.FileNameIsPrefix;

            FieldMappings.Clear();
            foreach (var fm in definition.FieldMappings)
                FieldMappings.Add(FieldMappingViewModel.FromModel(fm));

            StatusMessage = $"Loaded mapping '{definition.Name}' with {definition.FieldMappings.Count} mapping(s).";
        }
        catch (Exception ex)
        {
            MessageBox.Show(ex.Message, "Load Error", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    private void OpenSettings()
    {
        var dialog = new OpenFolderDialog
        {
            Title = "Select default mapping output folder"
        };

        if (!string.IsNullOrEmpty(_uiSettings.DefaultMappingOutputFolder) &&
            Directory.Exists(_uiSettings.DefaultMappingOutputFolder))
        {
            dialog.InitialDirectory = _uiSettings.DefaultMappingOutputFolder;
        }

        if (dialog.ShowDialog() == true)
        {
            _uiSettings.DefaultMappingOutputFolder = dialog.FolderName;
            SaveUiSettings();
            StatusMessage = $"Default mapping output folder set to '{dialog.FolderName}'.";
        }
    }

    private void LoadUiSettings()
    {
        try
        {
            if (File.Exists(UiSettingsPath))
            {
                var json = File.ReadAllText(UiSettingsPath);
                _uiSettings = JsonSerializer.Deserialize<UiSettings>(json,
                    new JsonSerializerOptions { PropertyNameCaseInsensitive = true }) ?? new UiSettings();
            }
        }
        catch
        {
            _uiSettings = new UiSettings();
        }
    }

    private void SaveUiSettings()
    {
        try
        {
            var json = JsonSerializer.Serialize(_uiSettings, new JsonSerializerOptions { WriteIndented = true });
            File.WriteAllText(UiSettingsPath, json);
        }
        catch (Exception ex)
        {
            StatusMessage = $"Could not save UI settings: {ex.Message}";
        }
    }
}
