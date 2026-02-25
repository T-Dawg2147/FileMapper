using FileMapper.Core.Models;
using FileMapper.Core.Validation;

namespace FileMapper.UI.ViewModels;

/// <summary>ViewModel representing a single field mapping row in the mapping grid.</summary>
public class FieldMappingViewModel : ViewModelBase
{
    private string _sourcePath = string.Empty;
    private string _targetName = string.Empty;
    private string? _sourceDataType;
    private string? _targetDataType;
    private TransformationRule? _transformation;
    private bool _typeWarningAcknowledged;
    private ValidationSeverity _validationSeverity = ValidationSeverity.Info;
    private string _validationMessage = string.Empty;

    /// <summary>Gets or sets the source field path.</summary>
    public string SourcePath
    {
        get => _sourcePath;
        set => SetProperty(ref _sourcePath, value);
    }

    /// <summary>Gets or sets the target field name.</summary>
    public string TargetName
    {
        get => _targetName;
        set => SetProperty(ref _targetName, value);
    }

    /// <summary>Gets or sets the source data type hint.</summary>
    public string? SourceDataType
    {
        get => _sourceDataType;
        set => SetProperty(ref _sourceDataType, value);
    }

    /// <summary>Gets or sets the target data type hint.</summary>
    public string? TargetDataType
    {
        get => _targetDataType;
        set => SetProperty(ref _targetDataType, value);
    }

    /// <summary>Gets or sets the transformation rule applied to this mapping.</summary>
    public TransformationRule? Transformation
    {
        get => _transformation;
        set => SetProperty(ref _transformation, value);
    }

    /// <summary>Gets or sets whether the user has acknowledged a type-compatibility warning.</summary>
    public bool TypeWarningAcknowledged
    {
        get => _typeWarningAcknowledged;
        set => SetProperty(ref _typeWarningAcknowledged, value);
    }

    /// <summary>Gets or sets the validation severity for display in the UI.</summary>
    public ValidationSeverity ValidationSeverity
    {
        get => _validationSeverity;
        set => SetProperty(ref _validationSeverity, value);
    }

    /// <summary>Gets or sets the validation message for display in the UI.</summary>
    public string ValidationMessage
    {
        get => _validationMessage;
        set => SetProperty(ref _validationMessage, value);
    }

    /// <summary>Converts this ViewModel to a <see cref="FieldMapping"/> model.</summary>
    public FieldMapping ToModel() => new()
    {
        SourcePath = SourcePath,
        TargetName = TargetName,
        SourceDataType = SourceDataType,
        TargetDataType = TargetDataType,
        Transformation = Transformation,
        TypeWarningAcknowledged = TypeWarningAcknowledged
    };

    /// <summary>Creates a <see cref="FieldMappingViewModel"/> from a <see cref="FieldMapping"/> model.</summary>
    public static FieldMappingViewModel FromModel(FieldMapping model) => new()
    {
        SourcePath = model.SourcePath,
        TargetName = model.TargetName,
        SourceDataType = model.SourceDataType,
        TargetDataType = model.TargetDataType,
        Transformation = model.Transformation,
        TypeWarningAcknowledged = model.TypeWarningAcknowledged
    };
}
