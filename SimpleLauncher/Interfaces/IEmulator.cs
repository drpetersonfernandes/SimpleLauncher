namespace SimpleLauncher.Interfaces;

public interface IEmulator
{
    string EmulatorName { get; }
    string EmulatorLocation { get; }
    string EmulatorParameters { get; }
    bool ReceiveANotificationOnEmulatorError { get; }
    string ImagePackDownloadLink { get; }
    string ImagePackDownloadLink2 { get; }
    string ImagePackDownloadLink3 { get; }
    string ImagePackDownloadLink4 { get; }
    string ImagePackDownloadLink5 { get; }
    string ImagePackDownloadExtractPath { get; }
}
