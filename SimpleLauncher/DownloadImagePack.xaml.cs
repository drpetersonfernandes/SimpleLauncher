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
    
    // Unique temp folder within the Windows temp directory
    private readonly string _tempFolder = Path.Combine(Path.GetTempPath(), "SimpleLauncher");
        
    public DownloadImagePack()
    {
        InitializeComponent();
            
        ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12 | SecurityProtocolType.Tls13;
            
        App.ApplyThemeToWindow(this);
            
        LoadConfig();
        PopulateSystemDropdown();
            
        // Subscribe to the Closed event
        Closed += EditSystemEasyModeAddSystem_Closed;

        MessageBox.Show("Some antivirus programs may lock or prevent the extraction of newly downloaded files, causing access issues during installation.\n\n" +
                        "If you encounter errors, try temporarily disabling real-time protection and run 'Simple Launcher' with administrative privileges.",
            "Info", MessageBoxButton.OK, MessageBoxImage.Information);
    }

    private void LoadConfig()
    {
        _config = EasyModeConfig.Load();
    }

    private void PopulateSystemDropdown()
    {
        if (_config?.Systems != null)
        {
            // Filter systems that have a valid ExtrasDownloadLink
            var systemsWithImagePacks = _config.Systems
                .Where(system => !string.IsNullOrEmpty(system.Emulators.Emulator.ExtrasDownloadLink))
                .Select(system => system.SystemName)
                .OrderBy(name => name)
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

    private async void DownloadExtrasButton_Click(object sender, RoutedEventArgs e)
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

                        bool extractionSuccess = await ExtractCompressedFile.Instance2.ExtractDownloadFilesAsync(downloadFilePath, extractionFolder);
                        pleaseWaitWindow.Close();

                        if (extractionSuccess)
                        {
                            ExtrasExtractionSuccess(selectedSystem, downloadFilePath);
                        }
                        else
                        {
                            MessageBox.Show($"My first attempt to download and extract the file failed.\n\n" +
                                            $"I will try again using in memory download and extraction",
                                "Extraction Error", MessageBoxButton.OK, MessageBoxImage.Warning);
                            
                            /////////////////////////////////////////////////
                            //// In Memory Download and Extract - Start /////
                            /////////////////////////////////////////////////
                            try
                            {
                                bool extractionSuccess2 = await DownloadAndExtractInMemory.DownloadAndExtractInMemoryAsync(extrasDownloadUrl, extractionFolder, _cancellationTokenSource.Token, DownloadProgressBar);
                                
                                if (extractionSuccess2)
                                {
                                    ExtrasExtractionSuccess(selectedSystem, downloadFilePath);
                                    
                                    // Notify Developer
                                    string notifyDeveloper = "User used DownloadAndExtractInMemory and the result was successful.\n";
                                    Exception ex = new Exception(notifyDeveloper);
                                    await LogErrors.LogErrorAsync(ex, notifyDeveloper);
                                }
                                else
                                {
                                    ExtrasDownloadExtractFailure(selectedSystem);
                                }
                            }
                            catch (Exception ex)
                            {
                                // Notify Developer
                                string formattedException = $"Error in DownloadAndExtractInMemoryAsync method.\n\n" +
                                                            $"Exception type: {ex.GetType().Name}\nException details: {ex.Message}";
                                await LogErrors.LogErrorAsync(ex, formattedException);
                                
                                ExtrasDownloadExtractFailure(selectedSystem);
                            }
                            /////////////////////////////////////////////////
                            //// In Memory Download and Extract - End  //////
                            /////////////////////////////////////////////////
                            
                        }
                    }
                    else
                    {
                        // Download was incomplete
                        MessageBoxResult result = MessageBox.Show($"Download was incomplete and will not be extracted.\n\n" +
                                                                  $"Would you like to be redirected to the download page?",
                            "Download Incomplete", MessageBoxButton.YesNo, MessageBoxImage.Question);
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
                catch (TaskCanceledException)
                {
                    // Delete a partially downloaded file
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
                    
                    MessageBox.Show("Image Pack download was canceled.", "Download Canceled", MessageBoxButton.OK, MessageBoxImage.Information);
                }
                catch (Exception ex)
                {
                    string formattedException = $"Error downloading the Image Pack.\n\n" +
                                                $"Exception type: {ex.GetType().Name}\nException details: {ex.Message}";
                    await LogErrors.LogErrorAsync(ex, formattedException);

                    MessageBoxResult result = MessageBox.Show($"Error downloading the Image Pack.\n\n" +
                                                              $"Would you like to be redirected to the download page?",
                        "Download Error", MessageBoxButton.YesNo, MessageBoxImage.Error);
                    if (result == MessageBoxResult.Yes)
                    {
                        Process.Start(new ProcessStartInfo
                        {
                            FileName = selectedSystem.Emulators.Emulator.ExtrasDownloadLink,
                            UseShellExecute = true
                        });
                    }
                }
                finally
                {
                    StopDownloadButton.IsEnabled = false;
                    
                    // Delete temp download file
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
            }
        }
        catch (Exception ex)
        {
            string formattedException = $"Error downloading the Image Pack.\n\n" +
                                        $"Exception type: {ex.GetType().Name}\n" +
                                        $"Exception details: {ex.Message}";
            await LogErrors.LogErrorAsync(ex, formattedException);
        }
    }

    private static void ExtrasDownloadExtractFailure(EasyModeSystemConfig selectedSystem)
    {
        // Download and Extraction failed - offer redirect option
        MessageBoxResult result = MessageBox.Show($"Download and Extraction failed for {selectedSystem.SystemName} Image Pack.\n\n" +
                                                  $"Would you like to be redirected to the download page?",
            "Download and Extraction failed", MessageBoxButton.YesNo, MessageBoxImage.Error);
        if (result == MessageBoxResult.Yes)
        {
            Process.Start(new ProcessStartInfo
            {
                FileName = selectedSystem.Emulators.Emulator.ExtrasDownloadLink,
                UseShellExecute = true
            });
        }
    }

    private void ExtrasExtractionSuccess(EasyModeSystemConfig selectedSystem, string downloadFilePath)
    {
        MessageBox.Show($"Image pack for {selectedSystem.SystemName} downloaded and extracted successfully.",
            "Download Complete", MessageBoxButton.OK, MessageBoxImage.Information);
                                
        // Clean up the downloaded file only if extraction is successful
        try
        {
            File.Delete(downloadFilePath);
        }
        catch (Exception)
        {
            // ignore
        }
               
        // Mark as downloaded and disable button
        DownloadExtrasButton.IsEnabled = false;
                            
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
            string formattedException = $"The requested file was not available on the server.\n\n" +
                                        $"URL: {downloadUrl}\nException type: {ex.GetType().Name}\nException details: {ex.Message}";
            await LogErrors.LogErrorAsync(ex, formattedException);

            MessageBox.Show("The requested file is not available on the server.\n\n" +
                            "The error was reported to the developer that will try to fix the issue.",
                "Download Error", MessageBoxButton.OK, MessageBoxImage.Error);
        }
        catch (HttpRequestException ex)
        {
            string formattedException = $"Network error during file download.\n\n" +
                                        $"URL: {downloadUrl}\nException type: {ex.GetType().Name}\nException details: {ex.Message}";
            await LogErrors.LogErrorAsync(ex, formattedException);

            MessageBox.Show("There was a network error either with your internet access or the server.\n\n" +
                            "Please try again later.", "Download Error", MessageBoxButton.OK, MessageBoxImage.Error);
        }
        catch (IOException ex)
        {
            string formattedException = $"File read/write error after file download.\n\n" +
                                        $"URL: {downloadUrl}\nException type: {ex.GetType().Name}\nException details: {ex.Message}";
            await LogErrors.LogErrorAsync(ex, formattedException);

            MessageBox.Show("There was a file read/write error after the file download.\n\n" +
                            "Some antivirus programs may lock or scan newly downloaded files, causing access issues.\n" +
                            "Try temporarily disabling real-time protection.",
                "Read/Write Error", MessageBoxButton.OK, MessageBoxImage.Error);
        }
        catch (TaskCanceledException ex)
        {
            if (cancellationToken.IsCancellationRequested)
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
                    
                string formattedException = $"Download was canceled by the user.\n\n" +
                                            $"URL: {downloadUrl}\n" +
                                            $"Exception type: {ex.GetType().Name}\n" +
                                            $"Exception details: {ex.Message}";
                await LogErrors.LogErrorAsync(ex, formattedException);
            }
            else
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
                    
                string formattedException = $"Download timed out or was canceled unexpectedly.\n\n" +
                                            $"URL: {downloadUrl}\n" +
                                            $"Exception type: {ex.GetType().Name}\n" +
                                            $"Exception details: {ex.Message}";
                await LogErrors.LogErrorAsync(ex, formattedException);
                    
                MessageBox.Show("Download timed out or was canceled unexpectedly.\n\n" +
                                "You can try again later.",
                    "Download Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }

    private void StopDownloadButton_Click(object sender, RoutedEventArgs e)
    {
        if (_cancellationTokenSource != null)
        {
            _cancellationTokenSource.Cancel(); // Cancel the ongoing download
            StopDownloadButton.IsEnabled = false; // Disable the stop button once the download is canceled

            // Reset completion flag and progress
            _isDownloadCompleted = false; 
            DownloadProgressBar.Value = 0;
                
            // Reinitialize the cancellation token source for the next download
            _cancellationTokenSource = new CancellationTokenSource();
        }
    }

    private void EditSystemEasyModeAddSystem_Closed(object sender, EventArgs e)
    {
        _config = null;
            
        // Prepare the process start info
        var processModule = Process.GetCurrentProcess().MainModule;
        if (processModule != null)
        {
            var startInfo = new ProcessStartInfo
            {
                FileName = processModule.FileName,
                UseShellExecute = true
            };

            // Start the new application instance
            Process.Start(startInfo);

            // Shutdown the current application instance
            Application.Current.Shutdown();
            Environment.Exit(0);
        }
    }

    private void Hyperlink_RequestNavigate(object sender, RequestNavigateEventArgs e)
    {
        Process.Start(new ProcessStartInfo(e.Uri.AbsoluteUri) { UseShellExecute = true });
        e.Handled = true;
    }

    private void ChooseExtractionFolderButton_Click(object sender, RoutedEventArgs e)
    {
        using var dialog = new System.Windows.Forms.FolderBrowserDialog();
        dialog.Description = @"Select a folder to extract the Image Pack";
        dialog.UseDescriptionForTitle = true;

        if (dialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
        {
            ExtractionFolderTextBox.Text = dialog.SelectedPath;
        }
    }
}