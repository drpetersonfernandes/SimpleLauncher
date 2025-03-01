using System;
using System.IO;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using CreateBatchFilesForPS3Games2.Interfaces;
using CreateBatchFilesForPS3Games2.Models;

namespace CreateBatchFilesForPS3Games2.Services
{
    public class BatchFileService : IBatchFileService
    {
        private readonly ISfoParser _sfoParser;
        private readonly ILogger _logger;

        public BatchFileService(ISfoParser sfoParser, ILogger logger)
        {
            _sfoParser = sfoParser;
            _logger = logger;
        }

        public async Task<int> CreateBatchFilesAsync(
            BatchCreationOptions options,
            IProgress<BatchCreationProgress>? progress = null,
            CancellationToken cancellationToken = default)
        {
            var filesCreated = 0;

            try
            {
                // Check if directories exist
                if (!Directory.Exists(options.GameFolderPath))
                {
                    throw new DirectoryNotFoundException($"Game folder path not found: {options.GameFolderPath}");
                }

                if (!File.Exists(options.Rpcs3Path))
                {
                    throw new FileNotFoundException($"RPCS3 executable not found: {options.Rpcs3Path}");
                }

                // Create batch files from disc-based games
                _logger.LogInformation("Scanning for disc-based games...");
                var discGamesCreated = await CreateBatchFilesForDiscBasedGamesAsync(
                    options, progress, cancellationToken);
                filesCreated += discGamesCreated;

                // Create batch files for installed games if requested
                if (options.IncludeInstalledGames)
                {
                    _logger.LogInformation("Scanning for installed games...");
                    var installedGamesCreated = await CreateBatchFilesForInstalledGamesAsync(
                        options, progress, cancellationToken);
                    filesCreated += installedGamesCreated;
                }

                _logger.LogInformation($"Successfully created {filesCreated} batch files.");
                return filesCreated;
            }
            catch (OperationCanceledException)
            {
                _logger.LogWarning("Operation was canceled.");
                throw; // Re-throw to be handled by the caller
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error creating batch files: {ex.Message}");
                throw; // Re-throw to be handled by the caller
            }
        }

        private async Task<int> CreateBatchFilesForDiscBasedGamesAsync(
            BatchCreationOptions options,
            IProgress<BatchCreationProgress>? progress,
            CancellationToken cancellationToken)
        {
            var subdirectories = Directory.GetDirectories(options.GameFolderPath);
            var totalDirectories = subdirectories.Length;
            var processed = 0;
            var filesCreated = 0;

            foreach (var subdirectory in subdirectories)
            {
                cancellationToken.ThrowIfCancellationRequested();

                processed++;
                var percentComplete = (int)((float)processed / totalDirectories * 100);
                var statusMessage = $"Processing disc game {processed}/{totalDirectories}: {Path.GetFileName(subdirectory)}";

                progress?.Report(new BatchCreationProgress
                {
                    PercentComplete = percentComplete,
                    StatusMessage = statusMessage,
                    FilesCreated = filesCreated,
                    TotalFiles = totalDirectories
                });

                var ebootPath = Path.Combine(subdirectory, "PS3_GAME", "USRDIR", "EBOOT.BIN");

                if (File.Exists(ebootPath))
                {
                    // Get game info from SFO
                    var sfoPath = Path.Combine(subdirectory, "PS3_GAME", "PARAM.SFO");
                    var sfoData = await _sfoParser.ParseSfoFileAsync(sfoPath, cancellationToken);

                    string fileName;
                    if (sfoData != null)
                    {
                        // Try to use TITLE field, fall back to TITLE_ID or directory name
                        if (sfoData.TryGetValue("TITLE", out var title) && !string.IsNullOrEmpty(title))
                        {
                            fileName = SanitizeFileName(title);
                        }
                        else if (sfoData.TryGetValue("TITLE_ID", out var titleId) && !string.IsNullOrEmpty(titleId))
                        {
                            fileName = titleId.ToUpper();
                        }
                        else
                        {
                            fileName = Path.GetFileName(subdirectory);
                        }
                    }
                    else
                    {
                        fileName = Path.GetFileName(subdirectory);
                    }

                    // Create batch file
                    var batchFilePath = Path.Combine(options.GameFolderPath, $"{fileName}.bat");

                    // Check if file exists and we're not set to overwrite
                    if (File.Exists(batchFilePath) && !options.OverwriteExisting)
                    {
                        _logger.LogInformation($"Skipping existing batch file: {batchFilePath}");
                        continue;
                    }

                    await File.WriteAllTextAsync(
                        batchFilePath,
                        $"\"{options.Rpcs3Path}\" --no-gui \"{ebootPath}\"",
                        cancellationToken);

                    _logger.LogInformation($"Created batch file: {batchFilePath}");
                    filesCreated++;
                }
            }

            return filesCreated;
        }

        private async Task<int> CreateBatchFilesForInstalledGamesAsync(
            BatchCreationOptions options,
            IProgress<BatchCreationProgress>? progress,
            CancellationToken cancellationToken)
        {
            var rpcs3GamesDir = Path.Combine(
                Path.GetDirectoryName(options.Rpcs3Path) ?? string.Empty,
                "dev_hdd0",
                "game");

            if (!Directory.Exists(rpcs3GamesDir))
            {
                _logger.LogWarning($"RPCS3 games directory not found: {rpcs3GamesDir}");
                return 0;
            }

            var subdirectories = Directory.GetDirectories(rpcs3GamesDir);
            var totalDirectories = subdirectories.Length;
            var processed = 0;
            var filesCreated = 0;
            var startPercent = progress != null ? 50 : 0; // Start at 50% if we're also processing disc games

            foreach (var subdirectory in subdirectories)
            {
                cancellationToken.ThrowIfCancellationRequested();

                processed++;
                var percentComplete = startPercent + (int)((float)processed / totalDirectories * (100 - startPercent));
                var statusMessage = $"Processing installed game {processed}/{totalDirectories}: {Path.GetFileName(subdirectory)}";

                progress?.Report(new BatchCreationProgress
                {
                    PercentComplete = percentComplete,
                    StatusMessage = statusMessage,
                    FilesCreated = filesCreated,
                    TotalFiles = totalDirectories
                });

                var ebootPath = Path.Combine(subdirectory, "USRDIR", "EBOOT.BIN");

                if (File.Exists(ebootPath))
                {
                    // Get game info from SFO
                    var sfoPath = Path.Combine(subdirectory, "PARAM.SFO");
                    var sfoData = await _sfoParser.ParseSfoFileAsync(sfoPath, cancellationToken);

                    string fileName;
                    if (sfoData != null)
                    {
                        // Try to use TITLE field, fall back to TITLE_ID or directory name
                        if (sfoData.TryGetValue("TITLE", out var title) && !string.IsNullOrEmpty(title))
                        {
                            fileName = SanitizeFileName(title);
                        }
                        else if (sfoData.TryGetValue("TITLE_ID", out var titleId) && !string.IsNullOrEmpty(titleId))
                        {
                            fileName = titleId.ToUpper();
                        }
                        else
                        {
                            fileName = Path.GetFileName(subdirectory);
                        }
                    }
                    else
                    {
                        fileName = Path.GetFileName(subdirectory);
                    }

                    // Create batch file
                    var batchFilePath = Path.Combine(options.GameFolderPath, $"{fileName}.bat");

                    // Check if file exists and we're not set to overwrite
                    if (File.Exists(batchFilePath) && !options.OverwriteExisting)
                    {
                        _logger.LogInformation($"Skipping existing batch file: {batchFilePath}");
                        continue;
                    }

                    await File.WriteAllTextAsync(
                        batchFilePath,
                        $"\"{options.Rpcs3Path}\" --no-gui \"{ebootPath}\"",
                        cancellationToken);

                    _logger.LogInformation($"Created batch file: {batchFilePath}");
                    filesCreated++;
                }
            }

            return filesCreated;
        }

        private string SanitizeFileName(string fileName)
        {
            // Remove invalid characters
            var invalidChars = Path.GetInvalidFileNameChars();
            foreach (var c in invalidChars)
            {
                fileName = fileName.Replace(c.ToString(), "");
            }

            // Replace specific characters with words
            fileName = fileName.Replace("Σ", "Sigma");

            // Remove unwanted symbols
            fileName = fileName.Replace("™", "").Replace("®", "");

            // Add space between letters and numbers
            fileName = Regex.Replace(fileName, @"(\p{L})(\p{N})", "$1 $2");
            fileName = Regex.Replace(fileName, @"(\p{N})(\p{L})", "$1 $2");

            // Split into words
            var words = fileName.Split(new[] { ' ', '.', '-', '_' }, StringSplitOptions.RemoveEmptyEntries);
            for (var i = 0; i < words.Length; i++)
            {
                // Convert Roman numerals to uppercase
                if (IsRomanNumeral(words[i]))
                {
                    words[i] = words[i].ToUpper();
                }
                else
                {
                    words[i] = Thread.CurrentThread.CurrentCulture.TextInfo.ToTitleCase(words[i].ToLower());
                }
            }

            // Reassemble the filename
            fileName = string.Join(" ", words);

            return fileName;
        }

        private bool IsRomanNumeral(string word)
        {
            return Regex.IsMatch(word, @"^M{0,4}(CM|CD|D?C{0,3})(XC|XL|L?X{0,3})(IX|IV|V?I{0,3})$", RegexOptions.IgnoreCase);
        }
    }
}