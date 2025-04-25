using System;
using System.Globalization;
using System.Windows;
using ControlzEx.Theming;
using SimpleLauncher.Services;

namespace SimpleLauncher;

public partial class SetFuzzyMatchingWindow // Inherit from MetroWindow
{
    private readonly SettingsManager _settings;

    public SetFuzzyMatchingWindow(SettingsManager settings)
    {
        InitializeComponent();
        _settings = settings ?? throw new ArgumentNullException(nameof(settings));

        // Apply Theme
        var detectedTheme = ThemeManager.Current.DetectTheme(this);
        if (detectedTheme != null)
        {
            ThemeManager.Current.ChangeTheme(this, detectedTheme.BaseColorScheme, detectedTheme.ColorScheme);
        }

        // Display the current threshold as percentage
        CurrentThresholdLabel.Content = _settings.FuzzyMatchingThreshold.ToString("P0", CultureInfo.InvariantCulture);

        // Set the slider's initial value
        // Ensure the initial value is within the slider's min/max range (0.7 to 1.0)
        ThresholdSlider.Value = Math.Max(ThresholdSlider.Minimum, Math.Min(ThresholdSlider.Maximum, _settings.FuzzyMatchingThreshold));
    }

    private void OkButton_Click(object sender, RoutedEventArgs e)
    {
        try
        {
            // Get the value directly from the slider
            var newThreshold = ThresholdSlider.Value;

            // The slider already constrains the value between 0.7 and 1.0,
            // so explicit range validation here is mostly for robustness,
            // though technically redundant if the slider min/max are correct.
            // We'll keep a simple check.
            if (newThreshold is >= 0.7 and <= 0.95)
            {
                // Update the setting
                _settings.FuzzyMatchingThreshold = newThreshold;
                _settings.Save();

                // Set DialogResult to true and close the window
                DialogResult = true;
                Close();
            }
            else
            {
                // This case should ideally not be hit with the slider configuration.
                // If it is, something is wrong with the slider setup or binding.
                MessageBoxLibrary.FuzzyMatchingErrorMessageBox();
                // Do not close the window
            }
        }
        catch (Exception ex)
        {
            // Log the error
            const string contextMessage = "Error setting fuzzy matching threshold from slider.";
            _ = LogErrors.LogErrorAsync(ex, contextMessage);

            // Notify the user
            MessageBoxLibrary.FuzzyMatchingErrorMessageBox2();
            // Do not close the window on error
        }
    }

    // No explicit CancelButton_Click needed because IsCancel="True" handles it
}
