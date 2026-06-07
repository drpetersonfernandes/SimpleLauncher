using System.Windows.Controls;
using System.Windows.Threading;

namespace SimpleLauncher.Services.UpdateStatusBar;

public interface IStatusBarHost
{
    Dispatcher Dispatcher { get; }
    Label StatusBarText { get; }
    DispatcherTimer StatusBarTimer { get; }
}
