using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

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
    }

    private void InitializeNumberButton()
    {
        Button numButton = new() { Content = "#", Width = 32, Height = 32 };
        numButton.Click += (_, _) =>
        {
            UpdateSelectedButton(numButton);
            OnLetterSelected?.Invoke("#");
        };
        LetterPanel.Children.Add(numButton);
    }

    private void InitializeLetterButtons()
    {
        foreach (var c in Enumerable.Range('A', 26).Select(static x => (char)x))
        {
            Button button = new() { Content = c.ToString(), Width = 32, Height = 32 };

            button.Click += (_, _) =>
            {
                UpdateSelectedButton(button);
                OnLetterSelected?.Invoke(c.ToString());
            };
            LetterPanel.Children.Add(button);
        }
    }

    private void InitializeAllButton()
    {
        var allButton = new Button { Content = "All", Width = 50, Height = 32 };
        allButton.Click += (_, _) =>
        {
            UpdateSelectedButton(allButton);
            OnLetterSelected?.Invoke(null);
        };
        LetterPanel.Children.Add(allButton);
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