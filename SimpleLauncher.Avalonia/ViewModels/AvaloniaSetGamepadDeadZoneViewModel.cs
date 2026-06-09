using System.Globalization;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using SimpleLauncher.Core.Interfaces;
using SimpleLauncher.Core.Services.SettingsManager;

namespace SimpleLauncher.Avalonia.ViewModels;

public partial class AvaloniaSetGamepadDeadZoneViewModel : ObservableObject
{
    private readonly SettingsManager _settingsManager;
    private readonly IMessageBoxLibraryService _messageBox;

    private double _deadZoneX;
    private double _deadZoneY;

    public AvaloniaSetGamepadDeadZoneViewModel(SettingsManager settingsManager, IMessageBoxLibraryService messageBox)
    {
        _settingsManager = settingsManager;
        _messageBox = messageBox;

        _deadZoneX = _settingsManager.DeadZoneX;
        _deadZoneY = _settingsManager.DeadZoneY;
    }

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

    public string DeadZoneXText => _deadZoneX.ToString("F2", CultureInfo.InvariantCulture);
    public string DeadZoneYText => _deadZoneY.ToString("F2", CultureInfo.InvariantCulture);

    public event Action? SaveCompleted;
    public event Action? CloseRequested;

    [RelayCommand]
    private async Task SaveAsync()
    {
        _settingsManager.DeadZoneX = (float)DeadZoneX;
        _settingsManager.DeadZoneY = (float)DeadZoneY;
        await _settingsManager.SaveAsync();

        await _messageBox.DeadZonesSavedMessageBox();
        SaveCompleted?.Invoke();
    }

    [RelayCommand]
    private async Task RevertAsync()
    {
        _settingsManager.DeadZoneX = SettingsManager.DefaultDeadZoneX;
        _settingsManager.DeadZoneY = SettingsManager.DefaultDeadZoneY;
        await _settingsManager.SaveAsync();

        DeadZoneX = SettingsManager.DefaultDeadZoneX;
        DeadZoneY = SettingsManager.DefaultDeadZoneY;

        CloseRequested?.Invoke();
    }
}
