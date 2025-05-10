namespace SimpleLauncher.Managers;

public partial class SystemManager
{
    public class Emulator
    {
        public string EmulatorName { get; init; }
        public string EmulatorLocation { get; init; }
        public string EmulatorParameters { get; init; }
        public bool ReceiveANotificationOnEmulatorError { get; init; }
    }
}