using System.Linq;
using System.Windows;
using SimpleLauncher.Services;
using Application = System.Windows.Application;

namespace SimpleLauncher;

public partial class MainWindow
{
    private void CreateBatchFilesForXbox360XBLAGames_Click(object sender, RoutedEventArgs e)
    {
        UpdateStatusBar.UpdateContent((string)Application.Current.TryFindResource("LaunchingTool") ?? "Launching tool...", this);
        _launchTools.CreateBatchFilesForXbox360XblaGames();
    }

    private void BatchConvertIsoToXiso_Click(object sender, RoutedEventArgs e)
    {
        UpdateStatusBar.UpdateContent((string)Application.Current.TryFindResource("LaunchingTool") ?? "Launching tool...", this);
        _launchTools.BatchConvertIsoToXiso();
    }

    private void BatchConvertToCHD_Click(object sender, RoutedEventArgs e)
    {
        UpdateStatusBar.UpdateContent((string)Application.Current.TryFindResource("LaunchingTool") ?? "Launching tool...", this);
        _launchTools.BatchConvertToChd();
    }

    private void BatchConvertToCompressedFile_Click(object sender, RoutedEventArgs e)
    {
        UpdateStatusBar.UpdateContent((string)Application.Current.TryFindResource("LaunchingTool") ?? "Launching tool...", this);
        _launchTools.BatchConvertToCompressedFile();
    }

    private void BatchConvertToRVZ_Click(object sender, RoutedEventArgs e)
    {
        UpdateStatusBar.UpdateContent((string)Application.Current.TryFindResource("LaunchingTool") ?? "Launching tool...", this);
        _launchTools.BatchConvertToRvz();
    }

    private void CreateBatchFilesForPS3Games_Click(object sender, RoutedEventArgs e)
    {
        UpdateStatusBar.UpdateContent((string)Application.Current.TryFindResource("LaunchingTool") ?? "Launching tool...", this);
        _launchTools.CreateBatchFilesForPs3Games();
    }

    private void CreateBatchFilesForScummVMGames_Click(object sender, RoutedEventArgs e)
    {
        UpdateStatusBar.UpdateContent((string)Application.Current.TryFindResource("LaunchingTool") ?? "Launching tool...", this);
        _launchTools.CreateBatchFilesForScummVmGames();
    }

    private void CreateBatchFilesForSegaModel3Games_Click(object sender, RoutedEventArgs e)
    {
        UpdateStatusBar.UpdateContent((string)Application.Current.TryFindResource("LaunchingTool") ?? "Launching tool...", this);
        _launchTools.CreateBatchFilesForSegaModel3Games();
    }

    private void CreateBatchFilesForWindowsGames_Click(object sender, RoutedEventArgs e)
    {
        UpdateStatusBar.UpdateContent((string)Application.Current.TryFindResource("LaunchingTool") ?? "Launching tool...", this);
        _launchTools.CreateBatchFilesForWindowsGames();
    }

    private void FindRomCover_Click(object sender, RoutedEventArgs e)
    {
        UpdateStatusBar.UpdateContent((string)Application.Current.TryFindResource("LaunchingTool") ?? "Launching tool...", this);
        ResetUi();
        _launchTools.FindRomCoverLaunch(_selectedImageFolder, _selectedRomFolders?.FirstOrDefault());
    }

    private void RomValidator_Click(object sender, RoutedEventArgs e)
    {
        UpdateStatusBar.UpdateContent((string)Application.Current.TryFindResource("LaunchingTool") ?? "Launching tool...", this);
        ResetUi();
        _launchTools.RomValidator();
    }

    private void GameCoverScraper_Click(object sender, RoutedEventArgs e)
    {
        UpdateStatusBar.UpdateContent((string)Application.Current.TryFindResource("LaunchingTool") ?? "Launching tool...", this);
        ResetUi();
        _launchTools.GameCoverScraper(_selectedImageFolder, _selectedRomFolders?.FirstOrDefault());
    }
}