﻿using System.Windows;

namespace SimpleLauncher;

public partial class EditSystemEasyModeWindow
{
    private readonly SettingsManager _settings;

    public EditSystemEasyModeWindow(SettingsManager settings)
    {
        InitializeComponent();

        // Load Settings
        _settings = settings;

        App.ApplyThemeToWindow(this);
    }

    private void AddSystemButton_Click(object sender, RoutedEventArgs routedEventArgs)
    {
        EditSystemEasyModeAddSystemWindow editSystemEasyModeAdd = new();
        Close();
        editSystemEasyModeAdd.ShowDialog();
    }

    private void EditSystemButton_Click(object sender, RoutedEventArgs routedEventArgs)
    {
        EditSystemWindow editSystem = new(_settings);
        Close();
        editSystem.ShowDialog();
    }

    private void DeleteSystemButton_Click(object sender, RoutedEventArgs routedEventArgs)
    {
        EditSystemWindow editSystem = new(_settings);
        Close();
        editSystem.ShowDialog();
    }

    private void DownloadImagePackButton_Click(object sender, RoutedEventArgs e)
    {
        DownloadImagePackWindow downloadImagePack = new();
        Close();
        downloadImagePack.ShowDialog();
    }
}