using System;

namespace CreateBatchFilesForPS3Games.Interfaces
{
    public interface ILogger
    {
        event EventHandler<string> OnLogMessageReceived;
        void LogInformation(string message);
        void LogWarning(string message);
        void LogError(string message);
    }
}