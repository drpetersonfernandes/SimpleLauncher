using System;

namespace SimpleLauncher.Services.TakeScreenshot.Models;

public class WindowItem
{
    public string Title { get; set; }
    public IntPtr Handle { get; init; }
}