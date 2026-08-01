namespace SimpleLauncher.Models;

public enum MessageBoxResult
{
    None = 0,
    Ok = 1,
    Cancel = 2,
    Yes = 6,
    No = 7
}

public enum MessageBoxButton
{
    Ok = 0,
    OkCancel = 1,
    YesNo = 4,
    YesNoCancel = 3
}

public enum MessageBoxImage
{
    None = 0,
    Error = 16,
    Warning = 48,
    Information = 64,
    Question = 32
}
