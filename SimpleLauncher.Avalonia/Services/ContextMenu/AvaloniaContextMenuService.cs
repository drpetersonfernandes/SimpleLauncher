using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Interactivity;
using SimpleLauncher.Avalonia.ViewModels;
using SimpleLauncher.Avalonia.Services;

namespace SimpleLauncher.Avalonia.Services.ContextMenus;

/// <summary>
/// Builds game context menus for right-click interactions.
/// Extracted from MainWindow.ShowGameContextMenu() for testability and reuse.
/// Mirrors the WPF ContextMenuService.
/// </summary>
public class AvaloniaContextMenuService
{
    private readonly LocalizationService _localization;

    public AvaloniaContextMenuService(LocalizationService localization)
    {
        _localization = localization;
    }

    /// <summary>
    /// Builds and opens a context menu for the given game at the pointer location.
    /// </summary>
    /// <param name="game">The game card ViewModel.</param>
    /// <param name="placementTarget">The control to anchor the menu to.</param>
    /// <param name="callbacks">Callback actions for each menu item.</param>
    public void ShowGameContextMenu(GameCardViewModel game, Control placementTarget, GameContextMenuCallbacks callbacks)
    {
        var contextMenu = new ContextMenu
        {
            Placement = PlacementMode.Pointer
        };

        var playItem = new MenuItem { Header = $"\u25B6 {_localization.GetString("Context.Play")}" };
        playItem.Click += (_, _) => callbacks.OnPlay(game);
        contextMenu.Items.Add(playItem);

        var favItem = new MenuItem
        {
            Header = game.IsFavorite
                ? $"\u2665 {_localization.GetString("Context.RemoveFavorites")}"
                : $"\u2661 {_localization.GetString("Context.AddFavorites")}"
        };
        favItem.Click += (_, _) => callbacks.OnToggleFavorite(game);
        contextMenu.Items.Add(favItem);

        contextMenu.Items.Add(new Separator());

        var detailItem = new MenuItem { Header = $"\u2139 {_localization.GetString("Context.ShowDetails")}" };
        detailItem.Click += (_, _) => callbacks.OnShowDetails(game);
        contextMenu.Items.Add(detailItem);

        var raItem = new MenuItem { Header = $"\uD83C\uDFC6 {_localization.GetString("Context.Achievements")}" };
        raItem.Click += (_, _) => callbacks.OnShowAchievements(game);
        contextMenu.Items.Add(raItem);

        var copyItem = new MenuItem { Header = $"\uD83D\uDCCB {_localization.GetString("Context.CopyPath")}" };
        copyItem.Click += (_, _) => callbacks.OnCopyPath(game);
        contextMenu.Items.Add(copyItem);

        var copyNameItem = new MenuItem { Header = $"\uD83D\uDCDD {_localization.GetString("Context.CopyName")}" };
        copyNameItem.Click += (_, _) => callbacks.OnCopyName(game);
        contextMenu.Items.Add(copyNameItem);

        contextMenu.Items.Add(new Separator());

        var showInFolderItem = new MenuItem { Header = $"\uD83D\uDCC2 {_localization.GetString("Context.ShowInFolder")}" };
        showInFolderItem.Click += (_, _) => callbacks.OnShowInFolder(game);
        contextMenu.Items.Add(showInFolderItem);

        var editSystemItem = new MenuItem { Header = $"\u270F {_localization.GetString("Context.EditSystem")}" };
        editSystemItem.Click += (_, _) => callbacks.OnEditSystem(game);
        contextMenu.Items.Add(editSystemItem);

        contextMenu.Open(placementTarget);
    }
}

/// <summary>
/// Callback actions for game context menu interactions.
/// </summary>
public class GameContextMenuCallbacks
{
    public required Action<GameCardViewModel> OnPlay { get; init; }
    public required Action<GameCardViewModel> OnToggleFavorite { get; init; }
    public required Action<GameCardViewModel> OnShowDetails { get; init; }
    public required Action<GameCardViewModel> OnShowAchievements { get; init; }
    public required Action<GameCardViewModel> OnCopyPath { get; init; }
    public required Action<GameCardViewModel> OnCopyName { get; init; }
    public required Action<GameCardViewModel> OnShowInFolder { get; init; }
    public required Action<GameCardViewModel> OnEditSystem { get; init; }
}
