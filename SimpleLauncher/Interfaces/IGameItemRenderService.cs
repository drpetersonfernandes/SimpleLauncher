using SimpleLauncher.Models;
using SimpleLauncher.Services.MameManager;
using SimpleLauncher.Services.SystemManager;

namespace SimpleLauncher.Interfaces;

public interface IGameItemRenderService
{
    void Initialize(IGameItemRenderHost host);
    void ReloadFactories(List<SystemManager> systemManagers, List<MameManager> machines);
    Task RenderGameItemsAsync(IList<string> files, string systemName, SystemManager systemManager, CancellationToken ct);
    Task HandleSelectionChangedAsync(GameListViewItem selectedItem);
    Task HandleDoubleClickAsync(GameListViewItem selectedItem);
    void ClearRenderedItems();
    void SetGameButtonsEnabled(bool isEnabled);
    int ImageHeight { get; set; }
}
