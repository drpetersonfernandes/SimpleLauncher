using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Navigation;
using Application = System.Windows.Application;
using MessageBox = System.Windows.MessageBox;

namespace SimpleLauncher;

public partial class DownloadImagePack
{
    private EasyModeConfig _config;
    private CancellationTokenSource _cancellationTokenSource;
    private readonly HttpClient _httpClient = new();
    private bool _isDownloadCompleted;
    private readonly string _tempFolder = Path.Combine(Path.GetTempPath(), "SimpleLauncher");
        
    public DownloadImagePack()
    {
        InitializeComponent();

        // Apply Theme
        App.ApplyThemeToWindow(this);
        
        // Load Config
        _config = EasyModeConfig.Load();
        
        PopulateSystemDropdown();
            
        // Subscribe to the Closed event
        Closed += EditSystemEasyModeAddSystem_Closed;

    }

    private void PopulateSystemDropdown()
    {
        if (_config?.Systems != null)
        {
            // Filter systems that have a valid ExtrasDownloadLink
            var systemsWithImagePacks = _config.Systems
                .Where(system => !string.IsNullOrEmpty(system.Emulators.Emulator.ExtrasDownloadLink))
                .Select(system => system.SystemName)
                .OrderBy(name => name) // Order by system name
                .ToList();

            SystemNameDropdown.ItemsSource = systemsWithImagePacks;
        }
    }

    private void SystemNameDropdown_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        if (SystemNameDropdown.SelectedItem != null)
        {
            var selectedSystem = _config.Systems.FirstOrDefault(system => system.SystemName == SystemNameDropdown.SelectedItem.ToString());
            if (selectedSystem != null)
            {
                DownloadExtrasButton.IsEnabled = !string.IsNullOrEmpty(selectedSystem.Emulators.Emulator.ExtrasDownloadLink);
            }
        }
    }

    private async void DownloadImagePackButton_Click(object sender, RoutedEventArgs e)
    {
        try
        {
            // Reset the flag at the start of the download
            _isDownloadCompleted = false;

            var selectedSystem = _config.Systems.FirstOrDefault(system => system.SystemName == SystemNameDropdown.SelectedItem.ToString());
            if (selectedSystem != null)
            {
                string extrasDownloadUrl = selectedSystem.Emulators.Emulator.ExtrasDownloadLink;
                
                // Determine the extraction folder
                string extractionFolder = !string.IsNullOrWhiteSpace(ExtractionFolderTextBox.Text)
                    ? ExtractionFolderTextBox.Text
                    : selectedSystem.Emulators.Emulator.ExtrasDownloadExtractPath;
                
                string downloadFilePath = Path.Combine(_tempFolder, Path.GetFileName(extrasDownloadUrl) ?? throw new InvalidOperationException("'Simple Launcher' could not get extrasDownloadUrl"));
                Directory.CreateDirectory(_tempFolder);

                try
                {
                    // Display progress bar
                    DownloadProgressBar.Visibility = Visibility.Visible;
                    DownloadProgressBar.Value = 0;
                    StopDownloadButton.IsEnabled = true;

                    // Initialize cancellation token source
                    _cancellationTokenSource = new CancellationTokenSource();

                    await DownloadWithProgressAsync(extrasDownloadUrl, downloadFilePath, _cancellationTokenSource.Token);

                    // Only proceed with extraction if the download completed successfully
                    if (_isDownloadCompleted)
                    {
                        // Show the PleaseWaitExtraction window
                        PleaseWaitExtraction pleaseWaitWindow = new PleaseWaitExtraction();
                        pleaseWaitWindow.Show();

                        bool extractionSuccess = await ExtractCompressedFile.Instance2.ExtractDownloadFilesAsync2(downloadFilePath, extractionFolder);
                        
                        // Close the PleaseWaitExtraction window
                        pleaseWaitWindow.Close();

                        if (extractionSuccess)
                        {
                            DownloadExtractionSuccessfullyMessageBox();

                            DeleteDownloadedFile(downloadFilePath);
               
                            // Mark as downloaded and disable button
                            DownloadExtrasButton.IsEnabled = false;
                        }
                        else // Extraction fail
                        {
                            // Notify developer
                            string formattedException = $"Image Pack extraction failed.\n\n" +
                                                        $"File: {extrasDownloadUrl}";
                            Exception ex = new Exception(formattedException);
                            await LogErrors.LogErrorAsync(ex, formattedException);

                            // Notify user
                            ImagePackDownloadExtractionFailedMessageBox();
                        }
                    }
                    else // Download fail
                    {
                        // Notify developer
                        string formattedException = $"Image Pack download failed.\n\n" +
                                                    $"File: {extrasDownloadUrl}";
                        Exception ex = new Exception(formattedException);
                        await LogErrors.LogErrorAsync(ex, formattedException);

                        // Notify user
                        ImagePackDownloadExtractionFailedMessageBox();
                    }
                }
                catch (TaskCanceledException)
                {
                    DeleteDownloadedFile(downloadFilePath);

                    // Notify user
                    DownloadCanceledMessageBox();
                }
                catch (Exception ex)
                {
                    // Notify developer
                    string formattedException = $"Error downloading the Image Pack.\n\n" +
                                                $"File: {extrasDownloadUrl}\n" +
                                                $"Exception type: {ex.GetType().Name}\n" +
                                                $"Exception details: {ex.Message}";
                    await LogErrors.LogErrorAsync(ex, formattedException);

                    // Notify user
                    DownloadErrorOfferRedirectMessageBox(selectedSystem);
                }
                finally
                {
                    StopDownloadButton.IsEnabled = false;
                    DeleteDownloadedFile(downloadFilePath);
                }
            }
        }
        catch (Exception ex)
        {
            // Notify developer
            string formattedException = $"Error downloading the Image Pack.\n\n" +
                                        $"Exception type: {ex.GetType().Name}\n" +
                                        $"Exception details: {ex.Message}";
            await LogErrors.LogErrorAsync(ex, formattedException);

            // Notify user
            ImagePackDownloadExtractionFailedMessageBox();
        }

        void DeleteDownloadedFile(string downloadFilePath)
        {
            if (File.Exists(downloadFilePath))
            {
                try
                {
                    File.Delete(downloadFilePath);
                }
                catch (Exception)
                {
                    // ignore
                }
            }
        }

        void ImagePackDownloadExtractionFailedMessageBox()
        {
            string imagePackdownloadorextraction2 = (string)Application.Current.TryFindResource("ImagePackdownloadorextraction") ?? "Image Pack download or extraction failed!";
            string grantSimpleLauncheradministrative2 = (string)Application.Current.TryFindResource("GrantSimpleLauncheradministrative") ?? "Grant 'Simple Launcher' administrative access and try again.";
            string ensuretheSimpleLauncher2 = (string)Application.Current.TryFindResource("EnsuretheSimpleLauncher") ?? "Ensure the 'Simple Launcher' folder is a writable directory.";
            string temporarilydisableyourantivirussoftware2 = (string)Application.Current.TryFindResource("Temporarilydisableyourantivirussoftware") ?? "Temporarily disable your antivirus software and try again.";
            string downloadorExtractionFailed2 = (string)Application.Current.TryFindResource("DownloadorExtractionFailed") ?? "Download or extraction failed.";
            MessageBox.Show($"{imagePackdownloadorextraction2}\n\n" +
                            $"{grantSimpleLauncheradministrative2}\n\n" +
                            $"{ensuretheSimpleLauncher2}\n\n" +
                            $"{temporarilydisableyourantivirussoftware2}",
                downloadorExtractionFailed2, MessageBoxButton.OK, MessageBoxImage.Information);
        }

        void DownloadExtractionSuccessfullyMessageBox()
        {
            string thedownloadandextractionweresuccessful2 = (string)Application.Current.TryFindResource("Thedownloadandextractionweresuccessful") ?? "The download and extraction were successful.";
            string success2 = (string)Application.Current.TryFindResource("Success") ?? "Success";
            MessageBox.Show(thedownloadandextractionweresuccessful2,
                success2, MessageBoxButton.OK, MessageBoxImage.Information);
        }

        void DownloadCanceledMessageBox()
        {
            string downloadwascanceled2 = (string)Application.Current.TryFindResource("Downloadwascanceled") ?? "Download was canceled.";
            string downloadCanceled2 = (string)Application.Current.TryFindResource("DownloadCanceled") ?? "Download Canceled";
            MessageBox.Show(downloadwascanceled2,
                downloadCanceled2, MessageBoxButton.OK, MessageBoxImage.Information);
        }

        void DownloadErrorOfferRedirectMessageBox(EasyModeSystemConfig selectedSystem)
        {
            string downloaderror2 = (string)Application.Current.TryFindResource("Downloaderror") ?? "Download error.";
            string wouldyouliketoberedirected2 = (string)Application.Current.TryFindResource("Wouldyouliketoberedirected") ?? "Would you like to be redirected to the download page?";
            string error2 = (string)Application.Current.TryFindResource("Error") ?? "Error";
            MessageBoxResult result = MessageBox.Show($"{downloaderror2}\n\n" +
                                                      $"{wouldyouliketoberedirected2}",
                error2, MessageBoxButton.YesNo, MessageBoxImage.Error);
            if (result == MessageBoxResult.Yes)
            {
                Process.Start(new ProcessStartInfo
                {
                    FileName = selectedSystem.Emulators.Emulator.ExtrasDownloadLink,
                    UseShellExecute = true
                });
            }
        }
    }

    private async Task DownloadWithProgressAsync(string downloadUrl, string destinationPath, CancellationToken cancellationToken)
    {
        try
        {
            using var response = await _httpClient.GetAsync(downloadUrl, HttpCompletionOption.ResponseHeadersRead, cancellationToken);
            response.EnsureSuccessStatusCode();

            long? totalBytes = response.Content.Headers.ContentLength;
            await using var contentStream = await response.Content.ReadAsStreamAsync(cancellationToken);
            await using var fileStream = new FileStream(destinationPath, FileMode.Create, FileAccess.Write, FileShare.None, 8192, true);
            var buffer = new byte[8192];
            long totalBytesRead = 0;
            int bytesRead;

            while ((bytesRead = await contentStream.ReadAsync(buffer, 0, buffer.Length, cancellationToken)) > 0)
            {
                await fileStream.WriteAsync(buffer, 0, bytesRead, cancellationToken);
                totalBytesRead += bytesRead;

                if (totalBytes.HasValue)
                {
                    DownloadProgressBar.Value = (double)totalBytesRead / totalBytes.Value * 100;
                }
            }

            // Check if the file was fully downloaded
            if (totalBytes.HasValue && totalBytesRead == totalBytes.Value)
            {
                _isDownloadCompleted = true;
            }
            else
            {
                _isDownloadCompleted = false;
                throw new IOException("Download incomplete. Bytes downloaded do not match the expected file size.");
            }

        }
        catch (HttpRequestException ex) when (ex.StatusCode == HttpStatusCode.NotFound)
        {
            // Notify developer
            string formattedException = $"The requested file was not available on the server.\n\n" +
                                        $"URL: {downloadUrl}\n" +
                                        $"Exception type: {ex.GetType().Name}\n" +
                                        $"Exception details: {ex.Message}";
            await LogErrors.LogErrorAsync(ex, formattedException);

            // Notify user
            DownloadErrorMessageBox();
        }
        catch (HttpRequestException ex)
        {
            // Notify developer
            string formattedException = $"Network error during file download.\n\n" +
                                        $"URL: {downloadUrl}\n" +
                                        $"Exception type: {ex.GetType().Name}\n" +
                                        $"Exception details: {ex.Message}";
            await LogErrors.LogErrorAsync(ex, formattedException);

            // Notify user
            DownloadErrorMessageBox();
        }
        catch (IOException ex)
        {
            // Notify developer
            string formattedException = $"File read/write error after file download.\n\n" +
                                        $"URL: {downloadUrl}\n" +
                                        $"Exception type: {ex.GetType().Name}\n" +
                                        $"Exception details: {ex.Message}";
            await LogErrors.LogErrorAsync(ex, formattedException);

            // Notify user
            IoExceptionMessageBox();
        }
        catch (TaskCanceledException ex)
        {
            if (cancellationToken.IsCancellationRequested)
            {
                // Notify developer
                string formattedException = $"Download was canceled by the user. User was not notified.\n\n" +
                                            $"URL: {downloadUrl}\n" +
                                            $"Exception type: {ex.GetType().Name}\n" +
                                            $"Exception details: {ex.Message}";
                await LogErrors.LogErrorAsync(ex, formattedException);
                
                DeleteDownloadedFile();
            }
            else
            {
                // Notify developer
                string formattedException = $"Download timed out or was canceled unexpectedly.\n\n" +
                                            $"URL: {downloadUrl}\n" +
                                            $"Exception type: {ex.GetType().Name}\n" +
                                            $"Exception details: {ex.Message}";
                await LogErrors.LogErrorAsync(ex, formattedException);
                
                DeleteDownloadedFile();
                DownloadErrorMessageBox();
            }
        }

        void DeleteDownloadedFile()
        {
            // If user canceled, delete the partially downloaded file
            if (File.Exists(destinationPath))
            {
                try
                {
                    File.Delete(destinationPath);
                }
                catch (Exception)
                {
                    // ignore
                }
            }
        }

        void IoExceptionMessageBox()
        {
            string afilereadwriteerroroccurred2 = (string)Application.Current.TryFindResource("Afilereadwriteerroroccurred") ?? "A file read/write error occurred after the file was downloaded.";
            string thiserrormayoccurifanantivirus2 = (string)Application.Current.TryFindResource("Thiserrormayoccurifanantivirus") ?? "This error may occur if an antivirus program is locking or scanning the newly downloaded files, causing access issues. Try temporarily disabling real-time protection.";
            string additionallygrantSimpleLauncher2 = (string)Application.Current.TryFindResource("AdditionallygrantSimpleLauncher") ?? "Additionally, grant 'Simple Launcher' administrative access to enable file writing.";
            string makesuretheSimpleLauncher2 = (string)Application.Current.TryFindResource("MakesuretheSimpleLauncher") ?? "Make sure the 'Simple Launcher' folder is located in a writable directory.";
            string wouldyouliketoopenthetemp2 = (string)Application.Current.TryFindResource("Wouldyouliketoopenthetemp") ?? "Would you like to open the 'temp' folder to view the downloaded file?";
            string error2 = (string)Application.Current.TryFindResource("Error") ?? "Error";            
            var result = MessageBox.Show($"{afilereadwriteerroroccurred2}\n\n" +
                                         $"{thiserrormayoccurifanantivirus2}\n\n" +
                                         $"{additionallygrantSimpleLauncher2}\n\n" +
                                         $"{makesuretheSimpleLauncher2}\n\n" +
                                         $"{wouldyouliketoopenthetemp2}",
                error2, MessageBoxButton.YesNo, MessageBoxImage.Error);
            if (result == MessageBoxResult.Yes)
            {
                try
                {
                    Process.Start(new ProcessStartInfo
                    {
                        FileName = _tempFolder,
                        UseShellExecute = true
                    });
                }
                catch (Exception)
                {
                    string simpleLauncherwasunabletoopenthe2 = (string)Application.Current.TryFindResource("SimpleLauncherwasunabletoopenthe") ?? "'Simple Launcher' was unable to open the 'temp' folder due to access issues.";
                    MessageBox.Show($"{simpleLauncherwasunabletoopenthe2}\n\n" +
                                    $"{_tempFolder}",
                        error2, MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        void DownloadErrorMessageBox()
        {
            string downloaderror2 = (string)Application.Current.TryFindResource("Downloaderror") ?? "Download error.";
            string theerrorwasreportedtothedeveloper2 = (string)Application.Current.TryFindResource("Theerrorwasreportedtothedeveloper") ?? "The error was reported to the developer that will try to fix the issue.";
            string error2 = (string)Application.Current.TryFindResource("Error") ?? "Error";
            MessageBox.Show($"{downloaderror2}\n\n" +
                            $"{theerrorwasreportedtothedeveloper2}",
                error2, MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    private void StopDownloadButton_Click(object sender, RoutedEventArgs e)
    {
        if (_cancellationTokenSource != null)
        {
            // Cancel the ongoing download
            _cancellationTokenSource.Cancel();
            
            // Disable the stop button once the download is canceled
            StopDownloadButton.IsEnabled = false; 

            // Reset completion flag and progress
            _isDownloadCompleted = false; 
            DownloadProgressBar.Value = 0;
                
            // Reinitialize the cancellation token source for the next download
            _cancellationTokenSource = new CancellationTokenSource();
        }
    }

    private void EditSystemEasyModeAddSystem_Closed(object sender, EventArgs e)
    {
        // Empty EasyMode Config
        _config = null;
    }

    private void Hyperlink_RequestNavigate(object sender, RequestNavigateEventArgs e)
    {
        Process.Start(new ProcessStartInfo(e.Uri.AbsoluteUri) { UseShellExecute = true });
        e.Handled = true;
    }

    private void ChooseExtractionFolderButton_Click(object sender, RoutedEventArgs e)
    {
        string selectafoldertoextracttheImagePack2 = (string)Application.Current.TryFindResource("SelectafoldertoextracttheImagePack") ?? "Select a folder to extract the Image Pack";
        using var dialog = new System.Windows.Forms.FolderBrowserDialog();
        dialog.Description = selectafoldertoextracttheImagePack2;
        dialog.UseDescriptionForTitle = true;

        if (dialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
        {
            ExtractionFolderTextBox.Text = dialog.SelectedPath;
        }
    }
}