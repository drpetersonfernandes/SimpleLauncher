namespace CreateBatchFilesForPS3Games.Models
{
    public class BatchCreationOptions
    {
        public required string GameFolderPath { get; init; }
        public required string Rpcs3Path { get; init; }
        public bool IncludeInstalledGames { get; init; } = true;
        public bool OverwriteExisting { get; init; } = true;
    }

    public class BatchCreationProgress
    {
        public int PercentComplete { get; init; }
        public string StatusMessage { get; init; } = string.Empty;
        public int FilesCreated { get; init; }
        public int TotalFiles { get; init; }
    }
}