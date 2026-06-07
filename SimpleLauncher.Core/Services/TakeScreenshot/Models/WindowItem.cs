namespace SimpleLauncher.Core.Services.TakeScreenshot.Models;

public class WindowItem
{
    public string Title { get; set; }
    public IntPtr Handle { get; init; }
}