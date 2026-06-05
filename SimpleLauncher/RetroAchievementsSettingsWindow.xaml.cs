using System.Windows;
using Microsoft.Win32;
using SimpleLauncher.ViewModels;

namespace SimpleLauncher;

public partial class RetroAchievementsSettingsWindow
{
    private readonly RetroAchievementsSettingsViewModel _viewModel;

    public RetroAchievementsSettingsWindow(RetroAchievementsSettingsViewModel viewModel)
    {
        InitializeComponent();
        App.ApplyThemeToWindow(this);
        Owner = Application.Current.MainWindow;

        _viewModel = viewModel;

        _viewModel.SaveCompleted += () =>
        {
            DialogResult = true;
            Close();
        };
        _viewModel.CloseRequested += () =>
        {
            DialogResult = false;
            Close();
        };
        _viewModel.RequestExePath += OnRequestExePath;

        ApiKeyPasswordBox.PasswordChanged += (_, _) => { _viewModel.ApiKey = ApiKeyPasswordBox.Password; };
        RaPasswordPasswordBox.PasswordChanged += (_, _) => { _viewModel.Password = RaPasswordPasswordBox.Password; };

        Loaded += (_, _) =>
        {
            ApiKeyPasswordBox.Password = _viewModel.ApiKey;
            RaPasswordPasswordBox.Password = _viewModel.Password;
        };

        DataContext = _viewModel;
    }

    private static string OnRequestExePath()
    {
        var openFileDialog = new OpenFileDialog
        {
            Title = "Select Emulator Executable",
            Filter = "Executable files (*.exe)|*.exe"
        };

        return openFileDialog.ShowDialog() == true ? openFileDialog.FileName : null;
    }
}