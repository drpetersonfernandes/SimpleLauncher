using System.Windows;
using SimpleLauncher.Services;

namespace SimpleLauncher;

public partial class MainWindow
{
    private void CreateBatchFilesForXbox360XBLAGames_Click(object sender, RoutedEventArgs e)
    {
        LaunchTools.CreateBatchFilesForXbox360XBLAGames_Click();
    }

    private void BatchConvertIsoToXiso_Click(object sender, RoutedEventArgs e)
    {
        LaunchTools.BatchConvertIsoToXiso_Click();
    }

    private void BatchConvertToCHD_Click(object sender, RoutedEventArgs e)
    {
        LaunchTools.BatchConvertToCHD_Click();
    }

    private void BatchConvertToCompressedFile_Click(object sender, RoutedEventArgs e)
    {
        LaunchTools.BatchConvertToCompressedFile_Click();
    }

    private void BatchConvertToRVZ_Click(object sender, RoutedEventArgs e)
    {
        LaunchTools.BatchConvertToRVZ_Click();
    }

    private void BatchVerifyCompressedFiles_Click(object sender, RoutedEventArgs e)
    {
        LaunchTools.BatchVerifyCompressedFiles_Click();
    }

    private void CreateBatchFilesForPS3Games_Click(object sender, RoutedEventArgs e)
    {
        LaunchTools.CreateBatchFilesForPS3Games_Click();
    }

    private void CreateBatchFilesForScummVMGames_Click(object sender, RoutedEventArgs e)
    {
        LaunchTools.CreateBatchFilesForScummVMGames_Click();
    }

    private void CreateBatchFilesForSegaModel3Games_Click(object sender, RoutedEventArgs e)
    {
        LaunchTools.CreateBatchFilesForSegaModel3Games_Click();
    }

    private void CreateBatchFilesForWindowsGames_Click(object sender, RoutedEventArgs e)
    {
        LaunchTools.CreateBatchFilesForWindowsGames_Click();
    }

    private void FindRomCover_Click(object sender, RoutedEventArgs e)
    {
        ResetUi();
        LaunchTools.FindRomCoverLaunch_Click(_selectedImageFolder, _selectedRomFolder);
    }
}