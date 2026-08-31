using SimpleLauncher.Avalonia.ViewModels;

namespace SimpleLauncher.Avalonia.Services.ContextMenus;

/// <summary>
///     Avalonia-only extra actions appended after the WPF-parity menu entries.
/// </summary>
public class GameContextMenuCallbacks
{
    public required Action<GameCardViewModel> OnShowDetails { get; init; }
    public required Action<GameCardViewModel> OnCopyPath { get; init; }
    public required Action<GameCardViewModel> OnCopyName { get; init; }
    public required Action<GameCardViewModel> OnShowInFolder { get; init; }
    public required Action<GameCardViewModel> OnEditSystem { get; init; }
}