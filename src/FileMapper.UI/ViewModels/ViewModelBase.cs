using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace FileMapper.UI.ViewModels;

/// <summary>Base class providing <see cref="INotifyPropertyChanged"/> support for all ViewModels.</summary>
public abstract class ViewModelBase : INotifyPropertyChanged
{
    /// <inheritdoc/>
    public event PropertyChangedEventHandler? PropertyChanged;

    /// <summary>Raises <see cref="PropertyChanged"/> for the given property name.</summary>
    protected void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));

    /// <summary>Sets a backing field and raises <see cref="PropertyChanged"/> if the value changed.</summary>
    protected bool SetProperty<T>(ref T field, T value, [CallerMemberName] string? propertyName = null)
    {
        if (EqualityComparer<T>.Default.Equals(field, value)) return false;
        field = value;
        OnPropertyChanged(propertyName);
        return true;
    }
}
