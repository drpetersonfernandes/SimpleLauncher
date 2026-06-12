using System.Globalization;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using SimpleLauncher.Interfaces;
using SimpleLauncher.Services.SettingsManager;
using Application = System.Windows.Application;

namespace SimpleLauncher.ViewModels;

/// <summary>
/// ViewModel for the gamepad dead zone configuration window.
/// </summary>
public partial class SetGamepadDeadZoneViewModel : ObservableObject
{
    private readonly SettingsManager _settingsManager;
    private readonly IMessageBoxLibraryService _messageBox;
    private readonly IResourceProvider _resourceProvider;
    private readonly ILogErrors _logErrors;

    private double _deadZoneX;
    private double _deadZoneY;

    public SetGamepadDeadZoneViewModel(SettingsManager settingsManager, IMessageBoxLibraryService messageBox, IResourceProvider resourceProvider, ILogErrors logErrors)
    {
        _settingsManager = settingsManager ?? throw new ArgumentNullException(nameof(settingsManager));
        _messageBox = messageBox;
        _resourceProvider = resourceProvider;
        _logErrors = logErrors;

        _deadZoneX = _settingsManager.DeadZoneX;
        _deadZoneY = _settingsManager.DeadZoneY;
    }

    /// <summary>Gets or sets the X-axis dead zone value.</summary>
    public double DeadZoneX
    {
        get => _deadZoneX;
        set
        {
            if (SetProperty(ref _deadZoneX, value))
            {
                OnPropertyChanged(nameof(DeadZoneXText));
            }
        }
    }

    /// <summary>Gets or sets the Y-axis dead zone value.</summary>
    public double DeadZoneY
    {
        get => _deadZoneY;
        set
        {
            if (SetProperty(ref _deadZoneY, value))
            {
                OnPropertyChanged(nameof(DeadZoneYText));
            }
        }
    }

    /// <summary>Gets the X-axis dead zone formatted for display.</summary>
    public string DeadZoneXText => _deadZoneX.ToString("F2", CultureInfo.InvariantCulture);

    /// <summary>Gets the Y-axis dead zone formatted for display.</summary>
    public string DeadZoneYText => _deadZoneY.ToString("F2", CultureInfo.InvariantCulture);

    /// <summary>Event raised when settings have been saved.</summary>
    public event Action SaveCompleted;

    /// <summary>Event raised when the window should be closed.</summary>
    public event Action CloseRequested;

    [RelayCommand]
    private async Task SaveAsync()
    {
        try
        {
            _settingsManager.DeadZoneX = (float)DeadZoneX;
            _settingsManager.DeadZoneY = (float)DeadZoneY;
            await _settingsManager.SaveAsync();

            (Application.Current.MainWindow as MainWindow)?.UpdateStatusBarService.UpdateContent(
                _resourceProvider.GetString("SavingGamepadDeadZoneSettings", "Saving gamepad dead zone settings..."));

            await _messageBox.DeadZonesSavedMessageBoxAsync();

            SaveCompleted?.Invoke();
        }
        catch (Exception ex)
        {
            _logErrors.LogAndForget(ex, "Error saving gamepad dead zone settings.");
            await _messageBox.FailedToSaveSettingsMessageBoxAsync();
        }
    }

    [RelayCommand]
    private void Revert()
    {
        _settingsManager.DeadZoneX = SettingsManager.DefaultDeadZoneX;
        _settingsManager.DeadZoneY = SettingsManager.DefaultDeadZoneY;
        _settingsManager.SaveAsync();

        DeadZoneX = SettingsManager.DefaultDeadZoneX;
        DeadZoneY = SettingsManager.DefaultDeadZoneY;

        (Application.Current.MainWindow as MainWindow)?.UpdateStatusBarService.UpdateContent(
            _resourceProvider.GetString("RevertingGamepadDeadZoneSettings", "Reverting gamepad dead zone settings..."));

        CloseRequested?.Invoke();
    }
}
