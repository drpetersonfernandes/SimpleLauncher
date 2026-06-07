using System.Windows.Controls;
using System.Windows.Threading;
using SimpleLauncher.Services.GameFileLoadingOrchestrator;
using SimpleLauncher.Services.UIReset;
using SystemManager = SimpleLauncher.Services.SystemManager.SystemManager;

namespace SimpleLauncher;

public partial class MainWindow : IGameFileLoadingHost
{
    Dispatcher IGameFileLoadingHost.Dispatcher => Dispatcher;

    ComboBox IGameFileLoadingHost.SystemComboBox => SystemComboBox;
    WrapPanel IGameFileLoadingHost.GameFileGrid => GameFileGrid;
    ScrollViewer IGameFileLoadingHost.Scroller => Scroller;
    DataGrid IGameFileLoadingHost.GameDataGrid => GameDataGrid;
    Grid IGameFileLoadingHost.ListViewPreviewArea => ListViewPreviewArea;
    Image IGameFileLoadingHost.PreviewImage => PreviewImage;

    string IGameFileLoadingHost.ViewMode => _settings.ViewMode;

    // ReSharper disable once ConvertToAutoProperty
    bool IGameFileLoadingHost.IsResortOperation => _isResortOperation;

    List<SystemManager> IGameFileLoadingHost.GetSystemManagers()
    {
        return _systemManagers;
    }

    Task IGameFileLoadingHost.DisplaySystemSelectionScreenAsync(CancellationToken cancellationToken)
    {
        return _gameBrowser.DisplaySystemSelectionScreenAsync(cancellationToken);
    }

    void IGameFileLoadingHost.SetLoadingState(bool isLoading, string message)
    {
        SetLoadingState(isLoading, message);
    }

    Task IGameFileLoadingHost.SetUiBeforeLoadGameFilesAsync()
    {
        return SetUiBeforeLoadGameFilesAsync();
    }

    List<string> IGameFileLoadingHost.SetPaginationOfListOfFiles(List<string> allFiles)
    {
        return SetPaginationOfListOfFiles(allFiles);
    }

    string IGameFileLoadingHost.GetCurrentFilter()
    {
        return ((IUiResetHost)this).CurrentFilter;
    }

    string IGameFileLoadingHost.GetActiveSearchQueryOrMode()
    {
        return ((IUiResetHost)this).ActiveSearchQueryOrMode;
    }

    string IGameFileLoadingHost.GetMameSortOrder()
    {
        return ((IUiResetHost)this).MameSortOrder;
    }
}
