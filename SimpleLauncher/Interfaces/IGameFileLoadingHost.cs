using System.Windows.Controls;
using System.Windows.Threading;
using SimpleLauncher.Services.SystemManager;

namespace SimpleLauncher.Interfaces;

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

    IList<SystemManagerService> GetSystemManagers();

    Task DisplaySystemSelectionScreenAsync(CancellationToken cancellationToken);
    void SetLoadingState(bool isLoading, string? message = null);
    Task SetUiBeforeLoadGameFilesAsync();
    IList<string> SetPaginationOfListOfFiles(IList<string> allFiles);
    string? GetCurrentFilter();
    string? GetActiveSearchQueryOrMode();
    string GetMameSortOrder();
}
