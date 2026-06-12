using System.Windows.Controls;
using SimpleLauncher.Models;

namespace SimpleLauncher.Interfaces;

public interface IContextMenuService
{
    ContextMenu AddRightClickReturnContextMenu(RightClickContext context, IFindCoverImageService findCoverImage, IContextMenuFunctions contextMenuFunctions);
    Button AddRightClickReturnButton(RightClickContext context, IFindCoverImageService findCoverImage, IContextMenuFunctions contextMenuFunctions);
}
