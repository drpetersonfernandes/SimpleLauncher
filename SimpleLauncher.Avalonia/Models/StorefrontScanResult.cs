namespace SimpleLauncher.Avalonia.Models;

/// <summary>
///     Result of a storefront scan that materialized games into the "Microsoft Windows" system.
/// </summary>
public sealed record StorefrontScanResult(int GamesFound, int ShortcutsCreated, bool SystemWasCreated);