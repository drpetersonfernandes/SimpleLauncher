using System;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input; // Added for KeyEventArgs
using System.Windows.Media;
using System.Windows.Automation; // Added for AutomationProperties
using SimpleLauncher.Services; // Added for PlaySoundEffects

namespace SimpleLauncher.UiHelpers;

public class FilterMenu
{
    public StackPanel LetterPanel { get; } = new() { Orientation = Orientation.Horizontal };
    private Button _selectedButton;

    public event Action<string> OnLetterSelected;

    public FilterMenu()
    {
        InitializeAllButton();
        InitializeNumberButton();
        InitializeLetterButtons();

        // // Set initial selected button to "All"
        // if (LetterPanel.Children.Count > 0 && LetterPanel.Children[0] is Button allButton)
        // {
        //     UpdateSelectedButton(allButton);
        // }
    }

    private void InitializeNumberButton()
    {
        Button numButton = new() { Content = "#", Width = 32, Height = 32 };
        // Set AutomationProperties.Name for screen readers
        AutomationProperties.SetName(numButton, (string)Application.Current.TryFindResource("FilterByNumber") ?? "Filter by Number");
        numButton.Click += (_, _) =>
        {
            PlaySoundEffects.PlayNotificationSound(); // Added sound effect
            UpdateSelectedButton(numButton);
            OnLetterSelected?.Invoke("#");
        };
        numButton.KeyDown += FilterButton_KeyDown; // Added KeyDown handler for keyboard navigation
        LetterPanel.Children.Add(numButton);
    }

    private void InitializeLetterButtons()
    {
        // Cache the resource lookup to avoid repeated calls
        var filterByLetterText = (string)Application.Current.TryFindResource("FilterByLetter") ?? "Filter by";

        foreach (var c in Enumerable.Range('A', 26).Select(static x => (char)x))
        {
            Button button = new() { Content = c.ToString(), Width = 32, Height = 32 };
            // Set AutomationProperties.Name for screen readers
            AutomationProperties.SetName(button, $"{filterByLetterText} {c}");
            button.Click += (_, _) =>
            {
                PlaySoundEffects.PlayNotificationSound(); // Added sound effect
                UpdateSelectedButton(button);
                OnLetterSelected?.Invoke(c.ToString());
            };
            button.KeyDown += FilterButton_KeyDown; // Added KeyDown handler for keyboard navigation
            LetterPanel.Children.Add(button);
        }
    }

    private void InitializeAllButton()
    {
        var allButton = new Button { Content = "All", Width = 50, Height = 32 };
        // Set AutomationProperties.Name for screen readers
        AutomationProperties.SetName(allButton, (string)Application.Current.TryFindResource("FilterByAll") ?? "Filter by All");
        allButton.Click += (_, _) =>
        {
            PlaySoundEffects.PlayNotificationSound(); // Added sound effect
            UpdateSelectedButton(allButton);
            OnLetterSelected?.Invoke(null);
        };
        allButton.KeyDown += FilterButton_KeyDown; // Added KeyDown handler for keyboard navigation
        LetterPanel.Children.Add(allButton);
    }

    [SuppressMessage("ReSharper", "SwitchStatementMissingSomeEnumCasesNoDefault")]
    private void FilterButton_KeyDown(object sender, KeyEventArgs e)
    {
        if (sender is not Button currentButton) return;

        var currentIndex = LetterPanel.Children.IndexOf(currentButton);
        var newIndex = -1;

        switch (e.Key)
        {
            case Key.Left:
                newIndex = currentIndex - 1;
                break;
            case Key.Right:
                newIndex = currentIndex + 1;
                break;
            case Key.Home:
                newIndex = 0;
                break;
            case Key.End:
                newIndex = LetterPanel.Children.Count - 1;
                break;
        }

        if (newIndex >= 0 && newIndex < LetterPanel.Children.Count)
        {
            if (LetterPanel.Children[newIndex] is Button targetButton)
            {
                targetButton.Focus(); // Move keyboard focus to the target button
                e.Handled = true; // Mark event as handled to prevent further processing
            }
        }
    }

    private void UpdateSelectedButton(Button button)
    {
        if (_selectedButton != null && _selectedButton != button)
        {
            _selectedButton.ClearValue(Control.BackgroundProperty);
        }

        button.Background = (Brush)Application.Current.Resources["AccentColorBrush"];
        _selectedButton = button;
    }

    public void DeselectLetter()
    {
        if (_selectedButton == null) return;

        _selectedButton.ClearValue(Control.BackgroundProperty);
        _selectedButton = null;
    }

    public void SetButtonsEnabled(bool isEnabled)
    {
        foreach (var child in LetterPanel.Children)
        {
            if (child is Button button)
            {
                button.IsEnabled = isEnabled;
            }
        }
    }
}