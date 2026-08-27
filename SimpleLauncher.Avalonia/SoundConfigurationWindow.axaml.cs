using Avalonia.Controls;
using Microsoft.Extensions.DependencyInjection;
using SimpleLauncher.Avalonia.ViewModels;
using SimpleLauncher.Core.Interfaces;

namespace SimpleLauncher.Avalonia;

/// <summary>
/// Window for configuring sound and notification settings.
/// </summary>
public partial class SoundConfigurationWindow : Window
{
    private readonly EventHandler _saveCompletedHandler;

    /// <summary>
    /// Initializes a new instance of the <see cref="SoundConfigurationWindow"/> class.
    /// </summary>
    /// <param name="viewModel">The view model providing sound configuration logic.</param>
    public SoundConfigurationWindow(SoundConfigurationViewModel viewModel)
    {
        InitializeComponent();

        _saveCompletedHandler = (_, _) => { Close(); };

        viewModel.SaveCompleted += _saveCompletedHandler;
        viewModel.CloseRequested += OnCloseRequested;
        viewModel.RequestSoundFilePath += OnRequestSoundFilePath;

        Closing += (_, _) =>
        {
            viewModel.SaveCompleted -= _saveCompletedHandler;
            viewModel.CloseRequested -= OnCloseRequested;
            viewModel.RequestSoundFilePath -= OnRequestSoundFilePath;
        };

        DataContext = viewModel;
    }

    private void OnCloseRequested(object? sender, EventArgs e)
    {
        Close();
    }

    private static async Task<string?> OnRequestSoundFilePath()
    {
        var filePicker = App.ServiceProvider.GetRequiredService<IFilePickerService>();
        return await filePicker.OpenFileAsync("Select Notification Sound File",
            "MP3 files (*.mp3)|*.mp3|All files (*.*)|*.*");
    }
}