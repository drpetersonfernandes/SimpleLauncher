#nullable enable
using System.Windows;
using Microsoft.Win32;
using SimpleLauncher.ViewModels;

namespace SimpleLauncher;

public partial class SoundConfigurationWindow
{
    public SoundConfigurationWindow(SoundConfigurationViewModel viewModel)
    {
        InitializeComponent();
        App.ApplyThemeToWindow(this);

        Owner = Application.Current.MainWindow;

        viewModel.SaveCompleted += () =>
        {
            DialogResult = true;
            Close();
        };
        viewModel.CloseRequested += Close;
        viewModel.RequestSoundFilePath += OnRequestSoundFilePath;

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