using System.Windows;
using SimpleLauncher.Services.DebugAndBugReport;
using Application = System.Windows.Application;

namespace SimpleLauncher;

public partial class MainWindow
{
    private async void CreateBatchFilesForXbox360XBLAGames_Click(object sender, RoutedEventArgs e)
    {
        try
        {
            UpdateStatusBarService.UpdateContent((string)Application.Current.TryFindResource("LaunchingTool") ?? "Launching tool...");
            _audioInput.PlayNotificationSound();
            await _launchTools.CreateBatchFilesForXbox360XblaGames();
        }
        catch (Exception ex)
        {
            _logErrors.LogAndForget(ex, "Error in the method CreateBatchFilesForXbox360XBLAGames_Click");
        }
    }

    private async void BatchConvertIsoToXiso_Click(object sender, RoutedEventArgs e)
    {
        try
        {
            UpdateStatusBarService.UpdateContent((string)Application.Current.TryFindResource("LaunchingTool") ?? "Launching tool...");
            _audioInput.PlayNotificationSound();
            await _launchTools.BatchConvertIsoToXiso();
        }
        catch (Exception ex)
        {
            _logErrors.LogAndForget(ex, "Error in the method BatchConvertIsoToXiso_Click");
        }
    }

    private async void BatchConvertToCHD_Click(object sender, RoutedEventArgs e)
    {
        try
        {
            UpdateStatusBarService.UpdateContent((string)Application.Current.TryFindResource("LaunchingTool") ?? "Launching tool...");
            _audioInput.PlayNotificationSound();
            await _launchTools.BatchConvertToChd(_selectedRomFolders?.FirstOrDefault());
            DebugLogger.Log($"Called BatchConvertToCHD with args: {_selectedRomFolders?.FirstOrDefault()}");
        }
        catch (Exception ex)
        {
            _logErrors.LogAndForget(ex, "Error in the method BatchConvertToCHD_Click");
        }
    }

    private async void BatchConvertToCompressedFile_Click(object sender, RoutedEventArgs e)
    {
        try
        {
            UpdateStatusBarService.UpdateContent((string)Application.Current.TryFindResource("LaunchingTool") ?? "Launching tool...");
            _audioInput.PlayNotificationSound();
            await _launchTools.BatchConvertToCompressedFile();
        }
        catch (Exception ex)
        {
            _logErrors.LogAndForget(ex, "Error in the method BatchConvertToCompressedFile_Click");
        }
    }

    private async void BatchConvertToRVZ_Click(object sender, RoutedEventArgs e)
    {
        try
        {
            UpdateStatusBarService.UpdateContent((string)Application.Current.TryFindResource("LaunchingTool") ?? "Launching tool...");
            _audioInput.PlayNotificationSound();
            await _launchTools.BatchConvertToRvz();
        }
        catch (Exception ex)
        {
            _logErrors.LogAndForget(ex, "Error in the method BatchConvertToRVZ_Click");
        }
    }

    private async void CreateBatchFilesForPS3Games_Click(object sender, RoutedEventArgs e)
    {
        try
        {
            UpdateStatusBarService.UpdateContent((string)Application.Current.TryFindResource("LaunchingTool") ?? "Launching tool...");
            _audioInput.PlayNotificationSound();
            await _launchTools.CreateBatchFilesForPs3Games();
        }
        catch (Exception ex)
        {
            _logErrors.LogAndForget(ex, "Error in the method CreateBatchFilesForPS3Games_Click");
        }
    }

    private async void CreateBatchFilesForScummVMGames_Click(object sender, RoutedEventArgs e)
    {
        try
        {
            UpdateStatusBarService.UpdateContent((string)Application.Current.TryFindResource("LaunchingTool") ?? "Launching tool...");
            _audioInput.PlayNotificationSound();
            await _launchTools.CreateBatchFilesForScummVmGames();
        }
        catch (Exception ex)
        {
            _logErrors.LogAndForget(ex, "Error in the method CreateBatchFilesForScummVMGames_Click");
        }
    }

    private async void CreateBatchFilesForWindowsGames_Click(object sender, RoutedEventArgs e)
    {
        try
        {
            UpdateStatusBarService.UpdateContent((string)Application.Current.TryFindResource("LaunchingTool") ?? "Launching tool...");
            _audioInput.PlayNotificationSound();
            await _launchTools.CreateBatchFilesForWindowsGames();
        }
        catch (Exception ex)
        {
            _logErrors.LogAndForget(ex, "Error in the method CreateBatchFilesForWindowsGames_Click");
        }
    }

    private async void FindRomCover_Click(object sender, RoutedEventArgs e)
    {
        try
        {
            UpdateStatusBarService.UpdateContent((string)Application.Current.TryFindResource("LaunchingTool") ?? "Launching tool...");
            _audioInput.PlayNotificationSound();
            await ResetUiAsync();
            await _launchTools.FindRomCoverLaunch(_selectedImageFolder, _selectedRomFolders?.FirstOrDefault());
        }
        catch (Exception ex)
        {
            _logErrors.LogAndForget(ex, "Error in the method FindRomCover_Click");
        }
    }

    private async void RomValidator_Click(object sender, RoutedEventArgs e)
    {
        try
        {
            UpdateStatusBarService.UpdateContent((string)Application.Current.TryFindResource("LaunchingTool") ?? "Launching tool...");
            _audioInput.PlayNotificationSound();
            await _launchTools.RomValidator();
        }
        catch (Exception ex)
        {
            _logErrors.LogAndForget(ex, "Error in the method RomValidator_Click");
        }
    }

    private async void GameCoverScraper_Click(object sender, RoutedEventArgs e)
    {
        try
        {
            UpdateStatusBarService.UpdateContent((string)Application.Current.TryFindResource("LaunchingTool") ?? "Launching tool...");
            _audioInput.PlayNotificationSound();
            await ResetUiAsync();
            await _launchTools.GameCoverScraper(_selectedImageFolder, _selectedRomFolders?.FirstOrDefault());
        }
        catch (Exception ex)
        {
            _logErrors.LogAndForget(ex, "Error in the method GameCoverScraper_Click");
        }
    }

    private async void RetroGameCoverDownloader_Click(object sender, RoutedEventArgs e)
    {
        try
        {
            UpdateStatusBarService.UpdateContent((string)Application.Current.TryFindResource("LaunchingTool") ?? "Launching tool...");
            _audioInput.PlayNotificationSound();
            await ResetUiAsync();
            await _launchTools.RetroGameCoverDownloader(_selectedImageFolder, _selectedRomFolders?.FirstOrDefault());
        }
        catch (Exception ex)
        {
            _logErrors.LogAndForget(ex, "Error in the method RetroGameCoverDownloader_Click");
        }
    }
}
