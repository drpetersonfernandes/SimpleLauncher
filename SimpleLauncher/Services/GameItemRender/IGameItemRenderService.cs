using SimpleLauncher.Models;

namespace SimpleLauncher.Services.GameItemRender;

public interface IGameItemRenderService
{
    void Initialize(IGameItemRenderHost host);
    void ReloadFactories(List<SystemManager.SystemManager> systemManagers, List<MameManager.MameManager> machines);
    Task RenderGameItemsAsync(IList<string> files, string systemName, SystemManager.SystemManager systemManager, CancellationToken ct);
    void HandleSelectionChangedAsync(GameListViewItem selectedItem);
    Task HandleDoubleClickAsync(GameListViewItem selectedItem);
    void ClearRenderedItems();
    void SetGameButtonsEnabled(bool isEnabled);
    int ImageHeight { get; set; }
}
