using System.ComponentModel;
using System.Globalization;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;
using System.Text.RegularExpressions;
using System.Windows;
using Microsoft.Win32;

namespace CreateBatchFilesForPS3Games;

public partial class MainWindow
{
    private readonly BugReportService _bugReportService;

    // Bug Report API configuration
    private const string BugReportApiUrl = "http://localhost:5116/api/send-bug-report";
    private const string BugReportApiKey = "hjh7yu6t56tyr540o9u8767676r5674534453235264c75b6t7ggghgg76trf564e";
    private const string ApplicationName = "CreateBatchFilesForPS3Games";

    public MainWindow()
    {
        InitializeComponent();

        // Initialize the bug report service
        _bugReportService = new BugReportService(BugReportApiUrl, BugReportApiKey, ApplicationName);

        LogMessage("Welcome to the Batch File Creator for PS3 Games.");
        LogMessage("");
        LogMessage("This program creates batch files to launch your PS3 games.");
        LogMessage("Please follow these steps:");
        LogMessage("1. Select the RPCS3 emulator executable file (rpcs3.exe)");
        LogMessage("2. Select the root folder containing your PS3 game folders");
        LogMessage("3. Click 'Create Batch Files' to generate the batch files");
        LogMessage("");
    }

    private void Window_Closing(object sender, CancelEventArgs e)
    {
        Application.Current.Shutdown();
        Environment.Exit(0);
    }

    private void LogMessage(string message)
    {
        Application.Current.Dispatcher.Invoke(() =>
        {
            LogTextBox.AppendText(message + Environment.NewLine);
            LogTextBox.ScrollToEnd();
        });
    }

    private void BrowseRPCS3Button_Click(object sender, RoutedEventArgs e)
    {
        var rpcs3ExePath = SelectFile();
        if (!string.IsNullOrEmpty(rpcs3ExePath))
        {
            Rpcs3PathTextBox.Text = rpcs3ExePath;
            LogMessage($"RPCS3 executable selected: {rpcs3ExePath}");

            if (!rpcs3ExePath.EndsWith("rpcs3.exe", StringComparison.OrdinalIgnoreCase))
            {
                LogMessage("Warning: The selected file does not appear to be rpcs3.exe.");
                _ = ReportBugAsync("User selected a file that doesn't appear to be rpcs3.exe: " + rpcs3ExePath);
            }
        }
    }

    private void BrowseFolderButton_Click(object sender, RoutedEventArgs e)
    {
        var rootFolder = SelectFolder();
        if (!string.IsNullOrEmpty(rootFolder))
        {
            GameFolderTextBox.Text = rootFolder;
            LogMessage($"Game folder selected: {rootFolder}");
        }
    }

    private async void CreateBatchFilesButton_Click(object sender, RoutedEventArgs e)
    {
        var rpcs3ExePath = Rpcs3PathTextBox.Text;
        var rootFolder = GameFolderTextBox.Text;

        if (string.IsNullOrEmpty(rpcs3ExePath))
        {
            LogMessage("Error: No RPCS3 executable selected.");
            ShowError("Please select the RPCS3 executable file (rpcs3.exe).");
            return;
        }

        if (!File.Exists(rpcs3ExePath))
        {
            LogMessage($"Error: RPCS3 executable not found at path: {rpcs3ExePath}");
            ShowError("The selected RPCS3 executable file does not exist.");
            await ReportBugAsync("RPCS3 executable not found", new FileNotFoundException("The RPCS3 executable was not found", rpcs3ExePath));
            return;
        }

        if (string.IsNullOrEmpty(rootFolder))
        {
            LogMessage("Error: No game folder selected.");
            ShowError("Please select the root folder containing your PS3 game folders.");
            return;
        }

        if (!Directory.Exists(rootFolder))
        {
            LogMessage($"Error: Game folder not found at path: {rootFolder}");
            ShowError("The selected game folder does not exist.");
            await ReportBugAsync("Game folder not found", new DirectoryNotFoundException($"Game folder not found: {rootFolder}"));
            return;
        }

        try
        {
            CreateBatchFilesForFolders(rootFolder, rpcs3ExePath);

            var rpcs3GameFolder = rpcs3ExePath.Replace("rpcs3.exe", "dev_hdd0\\game");

            CreateBatchFilesForFolders2(rootFolder, rpcs3GameFolder, rpcs3ExePath);

            LogMessage("");
            LogMessage("All batch files have been successfully created.");

            ShowMessageBox("All batch files have been successfully created.", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
        }
        catch (Exception ex)
        {
            LogMessage($"Error creating batch files: {ex.Message}");
            ShowError($"An error occurred while creating batch files: {ex.Message}");
            await ReportBugAsync("Error creating batch files", ex);
        }
    }

    private static string? SelectFolder()
    {
        var dialog = new OpenFolderDialog
        {
            Title = "Please select the root folder where your PS3 Game folders are located."
        };

        return dialog.ShowDialog() == true ? dialog.FolderName : null;
    }

    private static string? SelectFile()
    {
        var dialog = new OpenFileDialog
        {
            Title = "Please select the RPCS3 emulator executable file (rpcs3.exe)",
            Filter = "exe files (*.exe)|*.exe|All files (*.*)|*.*",
            RestoreDirectory = true
        };

        return dialog.ShowDialog() == true ? dialog.FileName : null;
    }

    private void CreateBatchFilesForFolders(string selectedFolder, string rpcs3BinaryPath)
    {
        try
        {
            var subdirectoryEntries = Directory.GetDirectories(selectedFolder);
            var filesCreated = 0;

            LogMessage("");
            LogMessage("Starting batch file creation process for main game folder...");
            LogMessage("");

            foreach (var subdirectory in subdirectoryEntries)
            {
                var ebootPath = Path.Combine(subdirectory, @"PS3_GAME\USRDIR\EBOOT.BIN");

                if (File.Exists(ebootPath))
                {
                    try
                    {
                        var title = GetTitle(subdirectory);
                        string batchFileName;

                        // Use TITLE if available, otherwise use TITLE_ID, and if neither, use the folder name
                        if (!string.IsNullOrEmpty(title))
                        {
                            batchFileName = title;
                        }
                        else
                        {
                            var titleId = GetId(subdirectory); // Fallback to TITLE_ID if TITLE is not available
                            batchFileName = !string.IsNullOrEmpty(titleId) ? titleId : Path.GetFileName(subdirectory);
                        }

                        // Sanitize the batch file name
                        batchFileName = SanitizeFileName(batchFileName);
                        var batchFilePath = Path.Combine(selectedFolder, batchFileName + ".bat");

                        using (StreamWriter sw = new(batchFilePath))
                        {
                            sw.WriteLine($"\"{rpcs3BinaryPath}\" --no-gui \"{ebootPath}\"");
                            LogMessage($"Batch file created: {batchFilePath}");
                        }

                        filesCreated++;
                    }
                    catch (Exception ex)
                    {
                        LogMessage($"Error creating batch file for {subdirectory}: {ex.Message}");
                        _ = ReportBugAsync($"Error creating batch file for {Path.GetFileName(subdirectory)}", ex);
                    }
                }
                else
                {
                    LogMessage($"EBOOT.BIN not found in {subdirectory}, skipping batch file creation.");
                }
            }

            if (filesCreated > 0)
            {
                LogMessage("");
                LogMessage($"{filesCreated} batch files have been successfully created for the games in the main folder.");
            }
            else
            {
                LogMessage("No EBOOT.BIN files found in subdirectories. No batch files were created.");
                ShowError("No EBOOT.BIN files found in subdirectories. No batch files were created.");
                _ = ReportBugAsync("No EBOOT.BIN files found in any subdirectories",
                    new FileNotFoundException("No EBOOT.BIN files found in subdirectories"));
            }
        }
        catch (Exception ex)
        {
            LogMessage($"Error accessing folder structure: {ex.Message}");
            _ = ReportBugAsync("Error accessing folder structure during batch file creation", ex);
            throw;
        }
    }

    private void CreateBatchFilesForFolders2(string selectedFolder, string rpcs3GameFolder, string rpcs3BinaryPath)
    {
        if (!Directory.Exists(rpcs3GameFolder))
        {
            LogMessage($"RPCS3 game folder not found: {rpcs3GameFolder}");
            _ = ReportBugAsync($"RPCS3 game folder not found: {rpcs3GameFolder}");
            return;
        }

        try
        {
            var subdirectoryEntries = Directory.GetDirectories(rpcs3GameFolder);
            var filesCreated = 0;

            LogMessage("");
            LogMessage("Starting batch file creation process for RPCS3 game folder...");
            LogMessage("");

            foreach (var subdirectory in subdirectoryEntries)
            {
                var ebootPath = Path.Combine(subdirectory, "USRDIR\\EBOOT.BIN");

                if (File.Exists(ebootPath))
                {
                    try
                    {
                        var title = GetTitle2(subdirectory);
                        string batchFileName;

                        // Use TITLE if available, otherwise use TITLE_ID, and if neither, use the folder name
                        if (!string.IsNullOrEmpty(title))
                        {
                            batchFileName = title;
                        }
                        else
                        {
                            var titleId = GetId2(subdirectory); // Fallback to TITLE_ID if TITLE is not available
                            batchFileName = !string.IsNullOrEmpty(titleId) ? titleId : Path.GetFileName(subdirectory);
                        }

                        // Sanitize the batch file name
                        batchFileName = SanitizeFileName(batchFileName);
                        var batchFilePath = Path.Combine(selectedFolder, batchFileName + ".bat");

                        using (StreamWriter sw = new(batchFilePath))
                        {
                            sw.WriteLine($"\"{rpcs3BinaryPath}\" --no-gui \"{ebootPath}\"");
                            LogMessage($"Batch file created: {batchFilePath}");
                        }

                        filesCreated++;
                    }
                    catch (Exception ex)
                    {
                        LogMessage($"Error creating batch file for {subdirectory}: {ex.Message}");
                        _ = ReportBugAsync($"Error creating batch file for RPCS3 game {Path.GetFileName(subdirectory)}", ex);
                    }
                }
                else
                {
                    LogMessage($"EBOOT.BIN not found in {subdirectory}, skipping batch file creation.");
                }
            }

            if (filesCreated > 0)
            {
                LogMessage("");
                LogMessage($"{filesCreated} batch files have been successfully created for the games in the RPCS3 folder.");
            }
            else
            {
                LogMessage("No EBOOT.BIN files found in RPCS3 game folder. No batch files were created.");
                _ = ReportBugAsync("No EBOOT.BIN files found in RPCS3 game folder");
            }
        }
        catch (Exception ex)
        {
            LogMessage($"Error accessing RPCS3 game folder structure: {ex.Message}");
            _ = ReportBugAsync("Error accessing RPCS3 game folder structure", ex);
        }
    }

    private string GetId(string folderPath)
    {
        var sfoFilePath = Path.Combine(folderPath, "PS3_GAME\\PARAM.SFO");

        try
        {
            var sfoData = ReadSfo(sfoFilePath);
            if (sfoData == null || !sfoData.TryGetValue("TITLE_ID", out var value))
                return "";

            return value.ToUpper();
        }
        catch (Exception ex)
        {
            LogMessage($"Error reading Title ID from SFO file at {sfoFilePath}: {ex.Message}");
            _ = ReportBugAsync($"Error reading Title ID from SFO file: {sfoFilePath}", ex);
            return "";
        }
    }

    private string GetId2(string folderPath)
    {
        var sfoFilePath = Path.Combine(folderPath, "PARAM.SFO");

        try
        {
            var sfoData = ReadSfo(sfoFilePath);
            if (sfoData == null || !sfoData.TryGetValue("TITLE_ID", out var value))
                return "";

            return value.ToUpper();
        }
        catch (Exception ex)
        {
            LogMessage($"Error reading Title ID from SFO file at {sfoFilePath}: {ex.Message}");
            _ = ReportBugAsync($"Error reading Title ID from SFO file: {sfoFilePath}", ex);
            return "";
        }
    }

    private string GetTitle(string folderPath)
    {
        var sfoFilePath = Path.Combine(folderPath, "PS3_GAME\\PARAM.SFO");

        try
        {
            var sfoData = ReadSfo(sfoFilePath);
            if (sfoData == null || !sfoData.TryGetValue("TITLE", out var value))
                return "";

            return value;
        }
        catch (Exception ex)
        {
            LogMessage($"Error reading Title from SFO file at {sfoFilePath}: {ex.Message}");
            _ = ReportBugAsync($"Error reading Title from SFO file: {sfoFilePath}", ex);
            return "";
        }
    }

    private string GetTitle2(string folderPath)
    {
        var sfoFilePath = Path.Combine(folderPath, "PARAM.SFO");

        try
        {
            var sfoData = ReadSfo(sfoFilePath);
            if (sfoData == null || !sfoData.TryGetValue("TITLE", out var value))
                return "";

            return value;
        }
        catch (Exception ex)
        {
            LogMessage($"Error reading Title from SFO file at {sfoFilePath}: {ex.Message}");
            _ = ReportBugAsync($"Error reading Title from SFO file: {sfoFilePath}", ex);
            return "";
        }
    }

    private static readonly char[] Separator = [' ', '.', '-', '_'];

    private string SanitizeFileName(string filename)
    {
        try
        {
            // Replace specific characters with words
            filename = filename.Replace("Σ", "Sigma");

            // Remove unwanted symbols
            filename = filename.Replace("™", "").Replace("®", "");

            // Add space between letters and numbers
            filename = Regex.Replace(filename, @"(\p{L})(\p{N})", "$1 $2");
            filename = Regex.Replace(filename, @"(\p{N})(\p{L})", "$1 $2");

            // Split the filename into words
            var words = filename.Split(Separator, StringSplitOptions.RemoveEmptyEntries);
            for (var i = 0; i < words.Length; i++)
            {
                // Convert Roman numerals to uppercase
                if (IsRomanNumeral(words[i]))
                {
                    words[i] = words[i].ToUpper();
                }
                else
                {
                    words[i] = CultureInfo.CurrentCulture.TextInfo.ToTitleCase(words[i].ToLower());
                }
            }

            // Reassemble the filename
            filename = string.Join(" ", words);

            // Remove any invalid characters
            var invalidChars = Path.GetInvalidFileNameChars();
            foreach (var c in invalidChars)
            {
                filename = filename.Replace(c.ToString(), "");
            }

            return filename;
        }
        catch (Exception ex)
        {
            LogMessage($"Error sanitizing filename '{filename}': {ex.Message}");
            _ = ReportBugAsync($"Error sanitizing filename: {filename}", ex);

            // Provide a safe fallback filename
            return "Game_" + DateTime.Now.Ticks;
        }
    }

    private static bool IsRomanNumeral(string word)
    {
        return Regex.IsMatch(word, @"^M{0,4}(CM|CD|D?C{0,3})(XC|XL|L?X{0,3})(IX|IV|V?I{0,3})$", RegexOptions.IgnoreCase);
    }

    private Dictionary<string, string>? ReadSfo(string sfoFilePath)
    {
        if (!File.Exists(sfoFilePath))
        {
            LogMessage($"SFO file not found: {sfoFilePath}");
            return null;
        }

        var result = new Dictionary<string, string>();
        var headerSize = Marshal.SizeOf(typeof(SfoHeader));
        var indexSize = Marshal.SizeOf(typeof(SfoTableEntry));

        try
        {
            var sfo = File.ReadAllBytes(sfoFilePath);
            SfoHeader sfoHeader;

            try
            {
                var handle = GCHandle.Alloc(sfo, GCHandleType.Pinned);
                sfoHeader = (SfoHeader)Marshal.PtrToStructure(handle.AddrOfPinnedObject(), typeof(SfoHeader))!;
                handle.Free();
            }
            catch (Exception ex)
            {
                LogMessage("Error reading SFO file: " + ex.Message);
                _ = ReportBugAsync($"Error parsing SFO header in file: {sfoFilePath}", ex);
                return null;
            }

            var indexOffset = headerSize;
            var keyOffset = sfoHeader.key_table_start;
            var valueOffset = sfoHeader.data_table_start;
            for (var i = 0; i < sfoHeader.tables_entries; i++)
            {
                var sfoEntry = new byte[indexSize];
                Array.Copy(sfo, indexOffset + i * indexSize, sfoEntry, 0, indexSize);

                SfoTableEntry sfoTableEntry;
                try
                {
                    var handle = GCHandle.Alloc(sfoEntry, GCHandleType.Pinned);
                    sfoTableEntry = (SfoTableEntry)Marshal.PtrToStructure(handle.AddrOfPinnedObject(), typeof(SfoTableEntry))!;
                    handle.Free();
                }
                catch (Exception ex)
                {
                    LogMessage("Error reading SFO table entry: " + ex.Message);
                    _ = ReportBugAsync($"Error parsing SFO table entry in file: {sfoFilePath}, entry index: {i}", ex);
                    continue; // Skip this entry and try to continue with others
                }

                var entryValueOffset = valueOffset + sfoTableEntry.data_offset;
                var entryKeyOffset = keyOffset + sfoTableEntry.key_offset;
                var val = "";

                try
                {
                    var keyBytes = Encoding.UTF8.GetString(sfo.Skip((int)entryKeyOffset).TakeWhile(b => !b.Equals(0)).ToArray());
                    switch (sfoTableEntry.data_fmt)
                    {
                        case 0x0004: //non-null string
                        case 0x0204: //null string
                            var strBytes = new byte[sfoTableEntry.data_len];
                            Array.Copy(sfo, entryValueOffset, strBytes, 0, sfoTableEntry.data_len);
                            val = Encoding.UTF8.GetString(strBytes).TrimEnd('\0');
                            break;
                        case 0x0404: //uint32
                            val = BitConverter.ToUInt32(sfo, (int)entryValueOffset).ToString();
                            break;
                    }

                    result.TryAdd(keyBytes, val);
                }
                catch (Exception ex)
                {
                    LogMessage($"Error processing SFO entry: {ex.Message}");
                    _ = ReportBugAsync($"Error processing SFO entry in file: {sfoFilePath}, entry index: {i}", ex);
                }
            }

            return result;
        }
        catch (Exception ex)
        {
            LogMessage($"Error reading SFO file {sfoFilePath}: {ex.Message}");
            _ = ReportBugAsync($"Error reading SFO file: {sfoFilePath}", ex);
            return null;
        }
    }

    [StructLayout(LayoutKind.Explicit)]
    private struct SfoHeader
    {
        [FieldOffset(0)] public uint magic;
        [FieldOffset(4)] public uint version;
        [FieldOffset(8)] public uint key_table_start;
        [FieldOffset(12)] public uint data_table_start;
        [FieldOffset(16)] public uint tables_entries;
    }

    [StructLayout(LayoutKind.Explicit)]
    private struct SfoTableEntry
    {
        [FieldOffset(0)] public ushort key_offset;
        [FieldOffset(2)] public ushort data_fmt; // 0x0004 utf8-S (non-null string), 0x0204 utf8 (null string), 0x0404 uint32
        [FieldOffset(4)] public uint data_len;
        [FieldOffset(8)] public uint data_max_len;
        [FieldOffset(12)] public uint data_offset;
    }

    private void ShowMessageBox(string message, string title, MessageBoxButton buttons, MessageBoxImage icon)
    {
        Dispatcher.Invoke(() =>
            MessageBox.Show(message, title, buttons, icon));
    }

    private void ShowError(string message)
    {
        ShowMessageBox(message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
    }

    /// <summary>
    /// Silently reports bugs/errors to the API
    /// </summary>
    private async Task ReportBugAsync(string message, Exception? exception = null)
    {
        try
        {
            var fullReport = new StringBuilder();

            // Add system information
            fullReport.AppendLine("=== Bug Report ===");
            fullReport.AppendLine($"Application: {ApplicationName}");
            fullReport.AppendLine($"Version: {GetType().Assembly.GetName().Version}");
            fullReport.AppendLine($"OS: {Environment.OSVersion}");
            fullReport.AppendLine($".NET Version: {Environment.Version}");
            fullReport.AppendLine($"Date/Time: {DateTime.Now}");
            fullReport.AppendLine();

            // Add a message
            fullReport.AppendLine("=== Error Message ===");
            fullReport.AppendLine(message);
            fullReport.AppendLine();

            // Add exception details if available
            if (exception != null)
            {
                fullReport.AppendLine("=== Exception Details ===");
                fullReport.AppendLine($"Type: {exception.GetType().FullName}");
                fullReport.AppendLine($"Message: {exception.Message}");
                fullReport.AppendLine($"Source: {exception.Source}");
                fullReport.AppendLine("Stack Trace:");
                fullReport.AppendLine(exception.StackTrace);

                // Add inner exception if available
                if (exception.InnerException != null)
                {
                    fullReport.AppendLine("Inner Exception:");
                    fullReport.AppendLine($"Type: {exception.InnerException.GetType().FullName}");
                    fullReport.AppendLine($"Message: {exception.InnerException.Message}");
                    fullReport.AppendLine($"Stack Trace:");
                    fullReport.AppendLine(exception.InnerException.StackTrace);
                }
            }

            // Add log contents if available
            if (LogTextBox != null)
            {
                var logContent = string.Empty;

                // Safely get log content from UI thread
                await Dispatcher.InvokeAsync(() =>
                {
                    logContent = LogTextBox.Text;
                });

                if (!string.IsNullOrEmpty(logContent))
                {
                    fullReport.AppendLine();
                    fullReport.AppendLine("=== Application Log ===");
                    fullReport.Append(logContent);
                }
            }

            // Add RPCS3 and games folder paths if available
            if (Rpcs3PathTextBox != null && GameFolderTextBox != null)
            {
                var rpcs3Path = string.Empty;
                var gameFolderPath = string.Empty;

                await Dispatcher.InvokeAsync(() =>
                {
                    rpcs3Path = Rpcs3PathTextBox.Text;
                    gameFolderPath = GameFolderTextBox.Text;
                });

                fullReport.AppendLine();
                fullReport.AppendLine("=== Configuration ===");
                fullReport.AppendLine($"RPCS3 Path: {rpcs3Path}");
                fullReport.AppendLine($"Games Folder: {gameFolderPath}");
            }

            // Silently send the report
            await _bugReportService.SendBugReportAsync(fullReport.ToString());
        }
        catch
        {
            // Silently fail if error reporting itself fails
        }
    }
}