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
        LaunchTools.CreateBatchFilesForXbox360XBLAGames_Click();
    }

    private void BatchConvertIsoToXiso_Click(object sender, RoutedEventArgs e)
    {
        UpdateStatusBar.UpdateContent((string)Application.Current.TryFindResource("LaunchingTool") ?? "Launching tool...", this);
        LaunchTools.BatchConvertIsoToXiso_Click();
    }

    private void BatchConvertToCHD_Click(object sender, RoutedEventArgs e)
    {
        UpdateStatusBar.UpdateContent((string)Application.Current.TryFindResource("LaunchingTool") ?? "Launching tool...", this);
        LaunchTools.BatchConvertToCHD_Click();
    }

    private void BatchConvertToCompressedFile_Click(object sender, RoutedEventArgs e)
    {
        UpdateStatusBar.UpdateContent((string)Application.Current.TryFindResource("LaunchingTool") ?? "Launching tool...", this);
        LaunchTools.BatchConvertToCompressedFile_Click();
    }

    private void BatchConvertToRVZ_Click(object sender, RoutedEventArgs e)
    {
        UpdateStatusBar.UpdateContent((string)Application.Current.TryFindResource("LaunchingTool") ?? "Launching tool...", this);
        LaunchTools.BatchConvertToRVZ_Click();
    }

    private void CreateBatchFilesForPS3Games_Click(object sender, RoutedEventArgs e)
    {
        UpdateStatusBar.UpdateContent((string)Application.Current.TryFindResource("LaunchingTool") ?? "Launching tool...", this);
        LaunchTools.CreateBatchFilesForPS3Games_Click();
    }

    private void CreateBatchFilesForScummVMGames_Click(object sender, RoutedEventArgs e)
    {
        UpdateStatusBar.UpdateContent((string)Application.Current.TryFindResource("LaunchingTool") ?? "Launching tool...", this);
        LaunchTools.CreateBatchFilesForScummVMGames_Click();
    }

    private void CreateBatchFilesForSegaModel3Games_Click(object sender, RoutedEventArgs e)
    {
        UpdateStatusBar.UpdateContent((string)Application.Current.TryFindResource("LaunchingTool") ?? "Launching tool...", this);
        LaunchTools.CreateBatchFilesForSegaModel3Games_Click();
    }

    private void CreateBatchFilesForWindowsGames_Click(object sender, RoutedEventArgs e)
    {
        UpdateStatusBar.UpdateContent((string)Application.Current.TryFindResource("LaunchingTool") ?? "Launching tool...", this);
        LaunchTools.CreateBatchFilesForWindowsGames_Click();
    }

    private void FindRomCover_Click(object sender, RoutedEventArgs e)
    {
        UpdateStatusBar.UpdateContent((string)Application.Current.TryFindResource("LaunchingTool") ?? "Launching tool...", this);
        ResetUi();
        LaunchTools.FindRomCoverLaunch_Click(_selectedImageFolder, _selectedRomFolders?.FirstOrDefault());
    }

    private void RomValidator_Click(object sender, RoutedEventArgs e)
    {
        UpdateStatusBar.UpdateContent((string)Application.Current.TryFindResource("LaunchingTool") ?? "Launching tool...", this);
        ResetUi();
        LaunchTools.RomValidator_Click();
    }

    private void GameCoverScraper_Click(object sender, RoutedEventArgs e)
    {
        UpdateStatusBar.UpdateContent((string)Application.Current.TryFindResource("LaunchingTool") ?? "Launching tool...", this);
        ResetUi();
        LaunchTools.GameCoverScraper_Click(_selectedImageFolder, _selectedRomFolders?.FirstOrDefault());
    }
}