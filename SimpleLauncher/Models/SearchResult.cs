using System.ComponentModel;
using System.Runtime.CompilerServices;
using SimpleLauncher.Services.SystemManager;

namespace SimpleLauncher.Models;

public class SearchResult : INotifyPropertyChanged
{
    public string FileName { get; init; } = null!;
    public string FileNameWithExtension { get; init; } = null!;
    public string MachineName { get; init; } = null!;
    public string FolderName { get; init; } = null!;
    public string FilePath { get; init; } = null!;
    public string SystemName { get; init; } = null!;
    public Emulator EmulatorManager { get; init; } = null!;

    public int Score
    {
        get;
        set
        {
            if (field == value) return;

            field = value;
            OnPropertyChanged();
        }
    }

    public string CoverImage { get; init; } = null!;

    public string DefaultEmulator
    {
        get
        {
            if (EmulatorManager?.EmulatorName != null)
                return EmulatorManager.EmulatorName;

            // Use Dispatcher to safely access Application.Current resources
            if (System.Windows.Application.Current?.TryFindResource("NoDefaultEmulator") is string localized)
                return localized;

            return "No Default Emulator";
        }
    }

    public event PropertyChangedEventHandler? PropertyChanged;

    protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}
