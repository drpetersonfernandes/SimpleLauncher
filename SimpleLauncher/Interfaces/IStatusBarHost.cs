using System.Windows.Controls;
using System.Windows.Threading;

namespace SimpleLauncher.Interfaces;

public interface IStatusBarHost
{
    Dispatcher Dispatcher { get; }
    Label StatusBarText { get; }
    DispatcherTimer StatusBarTimer { get; }
}
