using System.Windows.Controls;
using System.Windows.Threading;

namespace SimpleLauncher.Services.GameFileLoadingOrchestrator;

public interface IGameFileLoadingHost
{
    Dispatcher Dispatcher { get; }

    ComboBox SystemComboBox { get; }
    WrapPanel GameFileGrid { get; }
    ScrollViewer Scroller { get; }
    DataGrid GameDataGrid { get; }
    Grid ListViewPreviewArea { get; }
    Image PreviewImage { get; }

    string ViewMode { get; }
    bool IsResortOperation { get; }

    List<SystemManager.SystemManager> GetSystemManagers();

    Task DisplaySystemSelectionScreenAsync(CancellationToken cancellationToken);
    void SetLoadingState(bool isLoading, string message = null);
    Task SetUiBeforeLoadGameFilesAsync();
    List<string> SetPaginationOfListOfFiles(List<string> allFiles);
    string GetCurrentFilter();
    string GetActiveSearchQueryOrMode();
    string GetMameSortOrder();
}
