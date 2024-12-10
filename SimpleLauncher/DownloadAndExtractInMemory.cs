using System;
using System.IO;
using System.IO.Compression;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

namespace SimpleLauncher;

public static class DownloadAndExtractInMemory
{
    private static readonly HttpClient HttpClient = new();

    public static async Task<bool> DownloadAndExtractInMemoryAsync(string downloadUrl, string destinationPath,
        CancellationToken cancellationToken, ProgressBar progressBar)
    {
        try
        {
            // Step 1: Download the file into memory
            using var memoryStream = await DownloadToMemoryStream(downloadUrl, cancellationToken, progressBar);
            
            if (!IsValidZip(memoryStream))
            {
                throw new InvalidDataException("The downloaded file is not a valid ZIP archive.");
            }

            // Step 2: Extract the ZIP file directly from the memory stream
            Directory.CreateDirectory(destinationPath); // Ensure destination directory exists
            
            // Show the PleaseWaitExtraction window
            PleaseWaitExtraction pleaseWaitWindow = new PleaseWaitExtraction();
            pleaseWaitWindow.Show();

            await ExtractFromMemoryStream(destinationPath, cancellationToken, memoryStream);

            // Close the PleaseWaitExtraction window
            pleaseWaitWindow.Close();

            // Extraction successful
            return true;
        }
        catch (HttpRequestException ex) when (ex.StatusCode == HttpStatusCode.NotFound)
        {
            string formattedException = $"The requested file was not available on the server.\n\n" +
                                        $"URL: {downloadUrl}\n" +
                                        $"Exception type: {ex.GetType().Name}\n" +
                                        $"Exception details: {ex.Message}";
       
            throw new Exception(formattedException);
        }
        catch (IOException ex)
        {
            string formattedException = $"Error during in-memory extraction.\n\n" +
                                        $"URL: {downloadUrl}\n" +
                                        $"Exception type: {ex.GetType().Name}\n" +
                                        $"Exception details: {ex.Message}";
        
            throw new Exception(formattedException);
        }
        catch (TaskCanceledException ex)
        {
            MessageBox.Show("The operation was canceled by the user.",
                "Operation Canceled", MessageBoxButton.OK, MessageBoxImage.Information);

            string formattedException = "The operation was canceled by the user.\n\n" +
                                        $"URL: {downloadUrl}\n" +
                                        $"Exception type: {ex.GetType().Name}\n" +
                                        $"Exception details: {ex.Message}";
            throw new Exception(formattedException);
        }
    }
    
    private static async Task<MemoryStream> DownloadToMemoryStream(string downloadUrl, CancellationToken cancellationToken, ProgressBar progressBar)
    {
        MemoryStream memoryStream = null;
        try
        {
            progressBar.Value = 0;

            using var response = await HttpClient.GetAsync(downloadUrl, HttpCompletionOption.ResponseHeadersRead, cancellationToken);
            response.EnsureSuccessStatusCode();
            
            if (response.Content.Headers.ContentType?.MediaType != "application/zip")
            {
                throw new InvalidDataException("Unexpected file type. The server may have returned an error page.");
            }

            long? totalBytes = response.Content.Headers.ContentLength;
            await using var contentStream = await response.Content.ReadAsStreamAsync(cancellationToken);
            memoryStream = new MemoryStream();

            var buffer = new byte[8192];
            long totalBytesRead = 0;
            int bytesRead;

            while ((bytesRead = await contentStream.ReadAsync(buffer, 0, buffer.Length, cancellationToken)) > 0)
            {
                await memoryStream.WriteAsync(buffer, 0, bytesRead, cancellationToken);
                totalBytesRead += bytesRead;

                // Update progress bar
                if (totalBytes.HasValue)
                {
                    progressBar.Value = (double)totalBytesRead / totalBytes.Value * 100;
                }
            }

            // Verify if the file was fully downloaded
            if (totalBytes.HasValue && totalBytesRead != totalBytes.Value)
            {
                throw new IOException("Download incomplete. Bytes downloaded do not match the expected file size.");
            }

            memoryStream.Seek(0, SeekOrigin.Begin); // Reset memory stream for reading
            return memoryStream;
        }
        catch (InvalidDataException)
        {
            MessageBox.Show("The downloaded file is not a valid ZIP archive.\n\n" +
                            "This may be due to a network issue or server error.\n\n" +
                            "Please try again later.", "Extraction Error", MessageBoxButton.OK, MessageBoxImage.Error);
            throw;
        }
        catch
        {
            if (memoryStream != null)
            {
                await memoryStream.DisposeAsync();
            }
            throw;
        }
    }

    private static async Task ExtractFromMemoryStream(string destinationPath, CancellationToken cancellationToken,
        MemoryStream memoryStream)
    {
        using var zipArchive = new ZipArchive(memoryStream, ZipArchiveMode.Read, leaveOpen: false);
        foreach (var entry in zipArchive.Entries)
        {
            if (string.IsNullOrEmpty(entry.FullName) || entry.FullName.EndsWith("/"))
            {
                // Skip directories
                continue;
            }

            string extractedFilePath = Path.Combine(destinationPath, entry.FullName);
            Directory.CreateDirectory(Path.GetDirectoryName(extractedFilePath) ?? string.Empty);

            // Extract the file
            await using var entryStream = entry.Open(); // Open the ZIP entry
            await using var fileStream = File.Create(extractedFilePath); // Create the output file
            await entryStream.CopyToAsync(fileStream, cancellationToken); // Stream the content
        }
    }
    
    private static bool IsValidZip(MemoryStream memoryStream)
    {
        try
        {
            using var zipArchive = new ZipArchive(memoryStream, ZipArchiveMode.Read, leaveOpen: true);
            return true;
        }
        catch (InvalidDataException)
        {
            return false;
        }
    }
}