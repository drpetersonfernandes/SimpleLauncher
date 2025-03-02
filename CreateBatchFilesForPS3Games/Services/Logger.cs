using System;
using CreateBatchFilesForPS3Games.Interfaces;

namespace CreateBatchFilesForPS3Games.Services
{
    public class Logger : ILogger
    {
        public event EventHandler<string>? OnLogMessageReceived;

        public void LogInformation(string message)
        {
            OnLogMessageReceived?.Invoke(this, message);
        }

        public void LogWarning(string message)
        {
            OnLogMessageReceived?.Invoke(this, $"WARNING: {message}");
        }

        public void LogError(string message)
        {
            OnLogMessageReceived?.Invoke(this, $"ERROR: {message}");
        }
    }
}