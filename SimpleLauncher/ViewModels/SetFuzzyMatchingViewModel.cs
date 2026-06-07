using System.Globalization;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using SimpleLauncher.Services.DebugAndBugReport;
using SimpleLauncher.Services.MessageBox;
using SimpleLauncher.Services.SettingsManager;
using Application = System.Windows.Application;

namespace SimpleLauncher.ViewModels;

/// <summary>
/// ViewModel for the SetFuzzyMatchingWindow.
/// </summary>
public partial class SetFuzzyMatchingViewModel : ObservableObject
{
    private readonly SettingsManager _settings;
    private readonly ILogErrors _logErrors;

    private double _thresholdValue;

    // Slider constraints
    public const double MinimumThreshold = 0.7;
    public const double MaximumThreshold = 0.95;
    public const double TickFrequency = 0.05;

    public SetFuzzyMatchingViewModel(SettingsManager settings, ILogErrors logErrors)
    {
        _settings = settings ?? throw new ArgumentNullException(nameof(settings));
        _logErrors = logErrors;

        // Initialize values from settings
        _thresholdValue = Math.Max(MinimumThreshold, Math.Min(MaximumThreshold, _settings.FuzzyMatchingThreshold));
        CurrentThresholdText = _settings.FuzzyMatchingThreshold.ToString("P0", CultureInfo.InvariantCulture);
    }

    /// <summary>
    /// Gets the minimum threshold value for the slider.
    /// </summary>
    public double Minimum => MinimumThreshold;

    /// <summary>
    /// Gets the maximum threshold value for the slider.
    /// </summary>
    public double Maximum => MaximumThreshold;

    /// <summary>
    /// Gets the tick frequency for the slider.
    /// </summary>
    public double TickFrequencyValue => TickFrequency;

    /// <summary>
    /// Gets or sets the current threshold value from the slider.
    /// </summary>
    public double ThresholdValue
    {
        get => _thresholdValue;
        set
        {
            if (SetProperty(ref _thresholdValue, value))
            {
                // Update the percentage display when value changes
                OnPropertyChanged(nameof(ThresholdPercentage));
            }
        }
    }

    /// <summary>
    /// Gets the threshold as a percentage string for display.
    /// </summary>
    public string ThresholdPercentage => _thresholdValue.ToString("P0", CultureInfo.InvariantCulture);

    /// <summary>
    /// Gets the current threshold setting as displayed text.
    /// </summary>
    public string CurrentThresholdText { get; }

    /// <summary>
    /// Gets whether the settings can be saved.
    /// </summary>
    public bool CanSave => _settings != null;

    /// <summary>
    /// Event raised when the window should be closed with a success result.
    /// </summary>
    public event Action SaveCompleted;

    /// <summary>
    /// Event raised when the window should be closed without saving.
    /// </summary>
    public event Action CancelRequested;

    [RelayCommand(CanExecute = nameof(CanSave))]
    private void Save()
    {
        try
        {
            // Get the value directly from the slider.
            // The slider's Minimum and Maximum properties, combined with IsSnapToTickEnabled,
            // are designed to constrain the value within the desired range.
            // Math.Clamp is used here for explicit robustness against any potential
            // floating-point precision issues that might cause the slider's internal
            // value to slightly exceed its declared maximum/minimum.
            var newThreshold = Math.Clamp(ThresholdValue, MinimumThreshold, MaximumThreshold);

            _settings.FuzzyMatchingThreshold = newThreshold;
            _settings.SaveAsync();
            (Application.Current.MainWindow as MainWindow)?.UpdateStatusBarService.UpdateContent(
                (string)Application.Current.TryFindResource("SavingFuzzyMatchingSettings") ?? "Saving fuzzy matching settings...",
                Application.Current.MainWindow as MainWindow);

            SaveCompleted?.Invoke();
        }
        catch (Exception ex)
        {
            // Notify developer
            const string contextMessage = "Error setting fuzzy matching threshold from slider.";
            _logErrors.LogAndForget(ex, contextMessage);

            // Notify the user
            MessageBoxLibrary.FuzzyMatchingErrorFailToSetThresholdMessageBox();
            // Do not close the window on error
        }
    }

    [RelayCommand]
    private void Cancel()
    {
        CancelRequested?.Invoke();
    }
}
