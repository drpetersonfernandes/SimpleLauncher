namespace SimpleLauncher.Services.SystemManager;

public class Emulator
{
    public string EmulatorName { get; init; }
    public string EmulatorLocation { get; init; }
    public string EmulatorParameters { get; init; }
    public bool ReceiveANotificationOnEmulatorError { get; init; }
    public string ImagePackDownloadLink { get; init; }
    public string ImagePackDownloadLink2 { get; init; }
    public string ImagePackDownloadLink3 { get; init; }
    public string ImagePackDownloadLink4 { get; init; }
    public string ImagePackDownloadLink5 { get; init; }
    public string ImagePackDownloadExtractPath { get; init; }
}