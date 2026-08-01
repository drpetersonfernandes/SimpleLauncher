using SimpleLauncher.Models;
using SimpleLauncher.Services.MameManager;
using SimpleLauncher.Services.SystemManager;

namespace SimpleLauncher.Interfaces;

public interface IGameItemRenderService
{
    void Initialize(IGameItemRenderHost host);
    void ReloadFactories(IList<SystemManagerService> systemManagers, IList<MameManagerService> machines);
    Task RenderGameItemsAsync(IList<string> files, string systemName, SystemManagerService systemManager, CancellationToken ct);
    Task HandleSelectionChangedAsync(GameListViewItem selectedItem);
    Task HandleDoubleClickAsync(GameListViewItem selectedItem);
    void ClearRenderedItems();
    void SetGameButtonsEnabled(bool isEnabled);
    int ImageHeight { get; set; }
}
