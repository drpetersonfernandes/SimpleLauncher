using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows;

namespace SimpleLauncher.Services.GlobalSearch.Models;

public class SearchResult : INotifyPropertyChanged
{
    public string FileName { get; init; }
    public string FileNameWithExtension { get; init; }
    public string MachineName { get; init; }
    public string FolderName { get; init; }
    public string FilePath { get; init; }
    public string SystemName { get; init; }
    public SystemManager.Emulator EmulatorManager { get; init; }

    public int Score // If the Score can be updated after display and is bound, it should also notify.
    {
        get;
        set
        {
            if (field == value) return;

            field = value;
            OnPropertyChanged();
        }
    }

    public string CoverImage { get; init; }
    public string DefaultEmulator => EmulatorManager?.EmulatorName ?? (string)Application.Current.TryFindResource("NoDefaultEmulator") ?? "No Default Emulator";

    public event PropertyChangedEventHandler PropertyChanged;

    protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}