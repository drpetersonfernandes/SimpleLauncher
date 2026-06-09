using System.Globalization;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using SimpleLauncher.Core.Interfaces;
using SimpleLauncher.Core.Services.DebugAndBugReport;
using SimpleLauncher.Core.Services.SettingsManager;

namespace SimpleLauncher.Avalonia.ViewModels;

public partial class AvaloniaSetFuzzyMatchingViewModel : ObservableObject
{
    private readonly SettingsManager _settings;
    private readonly ILogErrors _logErrors;
    private readonly IMessageBoxLibraryService _messageBox;

    private double _thresholdValue;

    public const double MinimumThreshold = 0.7;
    public const double MaximumThreshold = 0.95;
    public const double TickFrequency = 0.05;

    public AvaloniaSetFuzzyMatchingViewModel(SettingsManager settings, ILogErrors logErrors, IMessageBoxLibraryService messageBox)
    {
        _settings = settings ?? throw new ArgumentNullException(nameof(settings));
        _logErrors = logErrors;
        _messageBox = messageBox;

        _thresholdValue = Math.Max(MinimumThreshold, Math.Min(MaximumThreshold, _settings.FuzzyMatchingThreshold));
        CurrentThresholdText = _settings.FuzzyMatchingThreshold.ToString("P0", CultureInfo.InvariantCulture);
    }

    public double Minimum => MinimumThreshold;
    public double Maximum => MaximumThreshold;
    public double TickFrequencyValue => TickFrequency;

    public double ThresholdValue
    {
        get => _thresholdValue;
        set
        {
            if (SetProperty(ref _thresholdValue, value))
            {
                OnPropertyChanged(nameof(ThresholdPercentage));
            }
        }
    }

    public string ThresholdPercentage => _thresholdValue.ToString("P0", CultureInfo.InvariantCulture);

    public string CurrentThresholdText { get; }

    public bool CanSave => true;

    public event Action? SaveCompleted;
    public event Action? CancelRequested;

    [RelayCommand(CanExecute = nameof(CanSave))]
    private async Task SaveAsync()
    {
        try
        {
            var newThreshold = Math.Clamp(ThresholdValue, MinimumThreshold, MaximumThreshold);

            _settings.FuzzyMatchingThreshold = newThreshold;
            await _settings.SaveAsync();

            SaveCompleted?.Invoke();
        }
        catch (Exception ex)
        {
            const string contextMessage = "Error setting fuzzy matching threshold from slider.";
            _logErrors.LogAndForget(ex, contextMessage);

            await _messageBox.FuzzyMatchingErrorFailToSetThresholdMessageBox();
        }
    }

    [RelayCommand]
    private void Cancel()
    {
        CancelRequested?.Invoke();
    }
}
