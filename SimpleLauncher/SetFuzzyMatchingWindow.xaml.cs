using System;
using System.Globalization;
using System.Windows;
using Microsoft.Extensions.DependencyInjection;
using SimpleLauncher.Services.DebugAndBugReport;
using SimpleLauncher.Services.MessageBox;
using SimpleLauncher.Services.SettingsManager;
using SimpleLauncher.Services.UpdateStatusBar;

namespace SimpleLauncher;

public partial class SetFuzzyMatchingWindow
{
    private readonly SettingsManager _settings;

    public SetFuzzyMatchingWindow(SettingsManager settings)
    {
        InitializeComponent();
        _settings = settings ?? throw new ArgumentNullException(nameof(settings));

        App.ApplyThemeToWindow(this);

        // Display the current threshold as percentage
        CurrentThresholdLabel.Content = _settings.FuzzyMatchingThreshold.ToString("P0", CultureInfo.InvariantCulture);

        // Set the slider's initial value
        // Ensure the initial value is within the slider's min/max range (0.7 to 0.95)
        ThresholdSlider.Value = Math.Max(ThresholdSlider.Minimum, Math.Min(ThresholdSlider.Maximum, _settings.FuzzyMatchingThreshold));
    }

    private void OkButton_Click(object sender, RoutedEventArgs e)
    {
        try
        {
            // Get the value directly from the slider.
            // The slider's Minimum and Maximum properties, combined with IsSnapToTickEnabled,
            // are designed to constrain the value within the desired range.
            // Math.Clamp is used here for explicit robustness against any potential
            // floating-point precision issues that might cause the slider's internal
            // value to slightly exceed its declared maximum/minimum.
            var newThreshold = Math.Clamp(ThresholdSlider.Value, ThresholdSlider.Minimum, ThresholdSlider.Maximum);

            _settings.FuzzyMatchingThreshold = newThreshold;
            _settings.Save();
            UpdateStatusBar.UpdateContent((string)Application.Current.TryFindResource("SavingFuzzyMatchingSettings") ?? "Saving fuzzy matching settings...", Application.Current.MainWindow as MainWindow);
            DialogResult = true;
            Close();
        }
        catch (Exception ex)
        {
            // Notify developer
            const string contextMessage = "Error setting fuzzy matching threshold from slider.";
            _ = App.ServiceProvider.GetRequiredService<ILogErrors>().LogErrorAsync(ex, contextMessage);

            // Notify the user
            MessageBoxLibrary.FuzzyMatchingErrorFailToSetThresholdMessageBox();
            // Do not close the window on error
        }
    }

    // No explicit CancelButton_Click needed because IsCancel="True" handles it
}