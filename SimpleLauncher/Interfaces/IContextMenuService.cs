using System.Windows.Controls;
using SimpleLauncher.Core.Interfaces;
using SimpleLauncher.Models;

namespace SimpleLauncher.Interfaces;

/// <summary>
///     Provides methods to create context menus and buttons for right-click interactions on game items.
/// </summary>
public interface IContextMenuService
{
    /// <summary>
    ///     Creates and returns a context menu for a right-click action on a game item.
    /// </summary>
    /// <param name="context">The context information for the right-click operation.</param>
    /// <param name="findCoverImage">The service used to locate cover images.</param>
    /// <param name="contextMenuFunctions">The functions available in the context menu.</param>
    /// <returns>The constructed context menu.</returns>
    ContextMenu AddRightClickReturnContextMenu(RightClickContext context, IFindCoverImageService findCoverImage,
        IContextMenuFunctions contextMenuFunctions);

    /// <summary>
    ///     Creates and returns a button that triggers a right-click context menu for a game item.
    /// </summary>
    /// <param name="context">The context information for the right-click operation.</param>
    /// <param name="findCoverImage">The service used to locate cover images.</param>
    /// <param name="contextMenuFunctions">The functions available in the context menu.</param>
    /// <returns>The constructed button.</returns>
    Button AddRightClickReturnButton(RightClickContext context, IFindCoverImageService findCoverImage,
        IContextMenuFunctions contextMenuFunctions);
}