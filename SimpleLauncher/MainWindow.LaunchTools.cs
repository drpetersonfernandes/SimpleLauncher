using System.Windows;
using Application = System.Windows.Application;

namespace SimpleLauncher;

using Interfaces;

/// <summary>
/// Partial MainWindow containing launch tool click handlers for batch file creation and emulator utilities.
/// </summary>
public partial class MainWindow
{
    private async void CreateBatchFilesForXbox360XBLAGames_ClickAsync(object sender, RoutedEventArgs e)
    {
        try
        {
            UpdateStatusBarService.UpdateContent((string)Application.Current.TryFindResource("LaunchingTool") ?? "Launching tool...");
            _audioInput.PlayNotificationSound();
            await _launchTools.CreateBatchFilesForXbox360XblaGamesAsync();
        }
        catch (Exception ex)
        {
            _logErrors.LogAndForget(ex, "Error in the method CreateBatchFilesForXbox360XBLAGames_ClickAsync");
        }
    }

    private async void BatchConvertIsoToXiso_ClickAsync(object sender, RoutedEventArgs e)
    {
        try
        {
            UpdateStatusBarService.UpdateContent((string)Application.Current.TryFindResource("LaunchingTool") ?? "Launching tool...");
            _audioInput.PlayNotificationSound();
            await _launchTools.BatchConvertIsoToXisoAsync();
        }
        catch (Exception ex)
        {
            _logErrors.LogAndForget(ex, "Error in the method BatchConvertIsoToXiso_ClickAsync");
        }
    }

    private async void BatchConvertToCHD_ClickAsync(object sender, RoutedEventArgs e)
    {
        try
        {
            UpdateStatusBarService.UpdateContent((string)Application.Current.TryFindResource("LaunchingTool") ?? "Launching tool...");
            _audioInput.PlayNotificationSound();
            await _launchTools.BatchConvertToChdAsync(_selectedRomFolders?.FirstOrDefault());
            _debugLogger.Log($"Called BatchConvertToCHD with args: {_selectedRomFolders?.FirstOrDefault()}");
        }
        catch (Exception ex)
        {
            _logErrors.LogAndForget(ex, "Error in the method BatchConvertToCHD_ClickAsync");
        }
    }

    private async void BatchConvertToCompressedFile_ClickAsync(object sender, RoutedEventArgs e)
    {
        try
        {
            UpdateStatusBarService.UpdateContent((string)Application.Current.TryFindResource("LaunchingTool") ?? "Launching tool...");
            _audioInput.PlayNotificationSound();
            await _launchTools.BatchConvertToCompressedFileAsync();
        }
        catch (Exception ex)
        {
            _logErrors.LogAndForget(ex, "Error in the method BatchConvertToCompressedFile_ClickAsync");
        }
    }

    private async void BatchConvertToRVZ_ClickAsync(object sender, RoutedEventArgs e)
    {
        try
        {
            UpdateStatusBarService.UpdateContent((string)Application.Current.TryFindResource("LaunchingTool") ?? "Launching tool...");
            _audioInput.PlayNotificationSound();
            await _launchTools.BatchConvertToRvzAsync();
        }
        catch (Exception ex)
        {
            _logErrors.LogAndForget(ex, "Error in the method BatchConvertToRVZ_ClickAsync");
        }
    }

    private async void CreateBatchFilesForPS3Games_ClickAsync(object sender, RoutedEventArgs e)
    {
        try
        {
            UpdateStatusBarService.UpdateContent((string)Application.Current.TryFindResource("LaunchingTool") ?? "Launching tool...");
            _audioInput.PlayNotificationSound();
            await _launchTools.CreateBatchFilesForPs3GamesAsync();
        }
        catch (Exception ex)
        {
            _logErrors.LogAndForget(ex, "Error in the method CreateBatchFilesForPS3Games_ClickAsync");
        }
    }

    private async void CreateBatchFilesForScummVMGames_ClickAsync(object sender, RoutedEventArgs e)
    {
        try
        {
            UpdateStatusBarService.UpdateContent((string)Application.Current.TryFindResource("LaunchingTool") ?? "Launching tool...");
            _audioInput.PlayNotificationSound();
            await _launchTools.CreateBatchFilesForScummVmGamesAsync();
        }
        catch (Exception ex)
        {
            _logErrors.LogAndForget(ex, "Error in the method CreateBatchFilesForScummVMGames_ClickAsync");
        }
    }

    private async void CreateBatchFilesForWindowsGames_ClickAsync(object sender, RoutedEventArgs e)
    {
        try
        {
            UpdateStatusBarService.UpdateContent((string)Application.Current.TryFindResource("LaunchingTool") ?? "Launching tool...");
            _audioInput.PlayNotificationSound();
            await _launchTools.CreateBatchFilesForWindowsGamesAsync();
        }
        catch (Exception ex)
        {
            _logErrors.LogAndForget(ex, "Error in the method CreateBatchFilesForWindowsGames_ClickAsync");
        }
    }

    private async void FindRomCover_ClickAsync(object sender, RoutedEventArgs e)
    {
        try
        {
            UpdateStatusBarService.UpdateContent((string)Application.Current.TryFindResource("LaunchingTool") ?? "Launching tool...");
            _audioInput.PlayNotificationSound();
            await ResetUiAsync();
            await _launchTools.FindRomCoverLaunchAsync(_selectedImageFolder, _selectedRomFolders?.FirstOrDefault());
        }
        catch (Exception ex)
        {
            _logErrors.LogAndForget(ex, "Error in the method FindRomCover_ClickAsync");
        }
    }

    private async void RomValidator_ClickAsync(object sender, RoutedEventArgs e)
    {
        try
        {
            UpdateStatusBarService.UpdateContent((string)Application.Current.TryFindResource("LaunchingTool") ?? "Launching tool...");
            _audioInput.PlayNotificationSound();
            await _launchTools.RomValidatorAsync();
        }
        catch (Exception ex)
        {
            _logErrors.LogAndForget(ex, "Error in the method RomValidator_ClickAsync");
        }
    }

    private async void GameCoverScraper_ClickAsync(object sender, RoutedEventArgs e)
    {
        try
        {
            UpdateStatusBarService.UpdateContent((string)Application.Current.TryFindResource("LaunchingTool") ?? "Launching tool...");
            _audioInput.PlayNotificationSound();
            await ResetUiAsync();
            await _launchTools.GameCoverScraperAsync(_selectedImageFolder, _selectedRomFolders?.FirstOrDefault());
        }
        catch (Exception ex)
        {
            _logErrors.LogAndForget(ex, "Error in the method GameCoverScraper_ClickAsync");
        }
    }

    private async void RetroGameCoverDownloader_ClickAsync(object sender, RoutedEventArgs e)
    {
        try
        {
            UpdateStatusBarService.UpdateContent((string)Application.Current.TryFindResource("LaunchingTool") ?? "Launching tool...");
            _audioInput.PlayNotificationSound();
            await ResetUiAsync();
            await _launchTools.RetroGameCoverDownloaderAsync(_selectedImageFolder, _selectedRomFolders?.FirstOrDefault());
        }
        catch (Exception ex)
        {
            _logErrors.LogAndForget(ex, "Error in the method RetroGameCoverDownloader_ClickAsync");
        }
    }
}
