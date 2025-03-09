﻿using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace SimpleLauncher;

public class LetterNumberMenu
{
    public StackPanel LetterPanel { get; } = new() { Orientation = Orientation.Horizontal };
    private Button _selectedButton;

    public event Action<string> OnLetterSelected;
    public event Action OnFavoritesSelected;
    public event Action OnFeelingLuckySelected;

    public LetterNumberMenu()
    {
        InitializeAllButton();
        InitializeNumberButton();
        InitializeLetterButtons();
        InitializeFavoritesButton();
        InitializeDiceButton();
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
        foreach (var c in Enumerable.Range('A', 26).Select(x => (char)x))
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

    private void InitializeFavoritesButton()
    {
        var favoritesButton = new Button
        {
            Width = 32,
            Height = 32,
            ToolTip = "Favorites"
        };

        var starImage = new Image
        {
            Source = new BitmapImage(new Uri("pack://application:,,,/images/star.png")),
            Width = 18,
            Height = 18
        };
        favoritesButton.Content = starImage;

        // Attach event for Favorites button
        favoritesButton.Click += (_, _) =>
        {
            UpdateSelectedButton(favoritesButton);
            OnFavoritesSelected?.Invoke(); // Trigger favorites event
        };

        LetterPanel.Children.Add(favoritesButton);
    }

    private void InitializeDiceButton()
    {
        var diceButton = new Button
        {
            Width = 32,
            Height = 32,
            ToolTip = "Feeling Lucky"
        };

        var diceImage = new Image
        {
            Source = new BitmapImage(new Uri("pack://application:,,,/images/dice.png")),
            Width = 18,
            Height = 18
        };
        diceButton.Content = diceImage;

        // Attach event for Dice button
        diceButton.Click += (_, _) =>
        {
            UpdateSelectedButton(diceButton);
            OnFeelingLuckySelected?.Invoke(); // Trigger feeling lucky event
        };

        LetterPanel.Children.Add(diceButton);
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
}