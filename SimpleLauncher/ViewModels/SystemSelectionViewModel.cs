using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using SimpleLauncher.Services.RetroAchievements;

namespace SimpleLauncher.ViewModels;

/// <summary>
/// ViewModel for the SystemSelectionWindow.
/// </summary>
public partial class SystemSelectionViewModel : ObservableObject
{
    private string _selectedSystem;

    public SystemSelectionViewModel(string currentGuess)
    {
        var systems = RetroAchievementsSystemMatcher.GetSupportedSystemNames();
        Systems = new ObservableCollection<string>(systems);

        // Try to pre-select the guess if it exists in the list
        SelectedSystem = systems.FirstOrDefault(s => s.Equals(currentGuess, StringComparison.OrdinalIgnoreCase));
    }

    /// <summary>
    /// Gets the collection of system names.
    /// </summary>
    public ObservableCollection<string> Systems { get; }

    /// <summary>
    /// Gets or sets the selected system name.
    /// </summary>
    public string SelectedSystem
    {
        get => _selectedSystem;
        set => SetProperty(ref _selectedSystem, value);
    }

    /// <summary>
    /// Event raised when the window should be closed with a dialog result.
    /// </summary>
    public event Action<bool?> DialogResultRequested;

    [RelayCommand]
    private void Confirm()
    {
        if (SelectedSystem != null)
        {
            DialogResultRequested?.Invoke(true);
        }
    }

    [RelayCommand]
    private void Cancel()
    {
        DialogResultRequested?.Invoke(false);
    }
}