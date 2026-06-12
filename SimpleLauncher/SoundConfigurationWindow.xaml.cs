#nullable enable

using System.Windows;
using Microsoft.Win32;
using SimpleLauncher.ViewModels;

namespace SimpleLauncher;

/// <summary>
/// Window for configuring sound and notification settings.
/// </summary>
public partial class SoundConfigurationWindow
{
    private readonly Action _saveCompletedHandler;

    /// <summary>
    /// Initializes a new instance of the <see cref="SoundConfigurationWindow"/> class.
    /// </summary>
    /// <param name="viewModel">The view model providing sound configuration logic.</param>
    public SoundConfigurationWindow(SoundConfigurationViewModel viewModel)
    {
        InitializeComponent();
        App.ApplyThemeToWindow(this);

        Owner = Application.Current.MainWindow;

        _saveCompletedHandler = () =>
        {
            if (IsLoaded) DialogResult = true;
            Close();
        };

        viewModel.SaveCompleted += _saveCompletedHandler;
        viewModel.CloseRequested += Close;
        viewModel.RequestSoundFilePath += OnRequestSoundFilePath;

        Closing += (_, _) =>
        {
            viewModel.SaveCompleted -= _saveCompletedHandler;
            viewModel.CloseRequested -= Close;
            viewModel.RequestSoundFilePath -= OnRequestSoundFilePath;
        };

        DataContext = viewModel;
    }

    private static string? OnRequestSoundFilePath()
    {
        var openFileDialog = new OpenFileDialog
        {
            Filter = "MP3 files (*.mp3)|*.mp3|All files (*.*)|*.*",
            Title = (string)Application.Current.TryFindResource("SelectNotificationSoundFile") ?? "Select Notification Sound File"
        };

        return openFileDialog.ShowDialog() == true ? openFileDialog.FileName : null;
    }
}
