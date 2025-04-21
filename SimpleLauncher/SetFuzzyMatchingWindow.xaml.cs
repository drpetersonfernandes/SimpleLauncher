using System;
using System.Globalization;
using System.Windows;
using ControlzEx.Theming;
using SimpleLauncher.Services;

namespace SimpleLauncher;

public partial class SetFuzzyMatchingWindow
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

        // Display the current threshold
        CurrentThresholdLabel.Content = _settings.FuzzyMatchingThreshold.ToString("F2", CultureInfo.InvariantCulture);
        NewThresholdTextBox.Text = _settings.FuzzyMatchingThreshold.ToString("F2", CultureInfo.InvariantCulture); // Pre-fill with current value
    }

    private void OkButton_Click(object sender, RoutedEventArgs e)
    {
        try
        {
            // Get the input text
            var inputText = NewThresholdTextBox.Text.Trim();

            // Attempt to parse the input as a double using invariant culture
            if (double.TryParse(inputText, NumberStyles.Any, CultureInfo.InvariantCulture, out var newThreshold))
            {
                // Validate the parsed value is within the acceptable range [0.0, 1.0]
                if (newThreshold is >= 0.0 and <= 1.0)
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
                    // Value is outside the valid range
                    var invalidInputMessageTitle = (string)Application.Current.TryFindResource("InvalidInputMessageTitle") ?? "Invalid Input";
                    var invalidInputMessageText = (string)Application.Current.TryFindResource("InvalidInputMessageText") ?? "Please enter a valid number between 0.0 and 1.0 for the threshold.";
                    MessageBoxLibrary.ShowErrorMessageBox(invalidInputMessageTitle, invalidInputMessageText);
                }
            }
            else
            {
                // Parsing failed
                var invalidInputMessageTitle = (string)Application.Current.TryFindResource("InvalidInputMessageTitle") ?? "Invalid Input";
                var invalidInputMessageText = (string)Application.Current.TryFindResource("InvalidInputMessageText") ?? "Please enter a valid number between 0.0 and 1.0 for the threshold.";
                MessageBoxLibrary.ShowErrorMessageBox(invalidInputMessageTitle, invalidInputMessageText);
            }
        }
        catch (Exception ex)
        {
            // Log the error
            const string contextMessage = "Error setting fuzzy matching threshold.";
            _ = LogErrors.LogErrorAsync(ex, contextMessage);

            // Notify the user
            var errorTitle = (string)Application.Current.TryFindResource("SetFuzzyMatchingThresholdFailureMessageBoxTitle") ?? "Error";
            var errorMessage = (string)Application.Current.TryFindResource("SetFuzzyMatchingThresholdFailureMessageBoxText") ?? "Failed to set fuzzy matching threshold.";
            MessageBoxLibrary.ShowErrorMessageBox(errorTitle, errorMessage);
        }
    }

    // No explicit CancelButton_Click needed because IsCancel="True" handles it
}