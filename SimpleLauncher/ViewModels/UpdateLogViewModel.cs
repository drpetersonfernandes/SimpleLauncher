using System.Text;
using CommunityToolkit.Mvvm.ComponentModel;

namespace SimpleLauncher.ViewModels;

/// <summary>
/// ViewModel for the UpdateLogWindow.
/// </summary>
public class UpdateLogViewModel : ObservableObject
{
    private readonly StringBuilder _logBuilder = new();
    private string _logText = "";

    /// <summary>
    /// Gets the log text content.
    /// </summary>
    public string LogText
    {
        get => _logText;
        private set => SetProperty(ref _logText, value);
    }

    /// <summary>
    /// Appends a message to the log.
    /// </summary>
    public void AppendLog(string message)
    {
        _logBuilder.Append(DateTime.Now.ToString("HH:mm:ss"));
        _logBuilder.Append(" - ");
        _logBuilder.AppendLine(message);
        LogText = _logBuilder.ToString();
    }
}