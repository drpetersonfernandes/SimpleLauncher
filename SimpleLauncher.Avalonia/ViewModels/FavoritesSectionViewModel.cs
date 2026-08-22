using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Extensions.Configuration;
using SimpleLauncher.Avalonia.Services.Favorites;
using SimpleLauncher.Avalonia.Services.SystemManager;
using SimpleLauncher.Core.Interfaces;
using SimpleLauncher.Core.Services.PlaySound;
using ILogger = Serilog.ILogger;
using PathHelper = SimpleLauncher.Core.Services.CheckPaths.PathHelper;

namespace SimpleLauncher.Avalonia.ViewModels;

/// <summary>
/// ViewModel for the Favorites section of the main window, showing the user's
/// favorite games in a table with preview, removal, and launching.
/// Mirrors the WPF FavoritesPage / FavoritesViewModel flow: favorites are stored
/// as a file NAME and resolved against the system folders at use time.
/// </summary>
public partial class FavoritesSectionViewModel : ObservableObject
{
    private readonly FavoritesManager _favoritesManager;
    private readonly SystemManagerService _systemManager;
    private readonly IFindCoverImageService _findCoverImage;
    private readonly IMameDataService _mameData;
    private readonly PlaySoundEffects _playSoundEffects;
    private readonly IMessageBoxLibraryService _messageBox;
    private readonly IConfiguration _configuration;
    private readonly MainViewModel _mainViewModel;
    private readonly ILogger _logErrors;

    [ObservableProperty] private ObservableCollection<FavoriteRowViewModel> _favorites = [];

    [ObservableProperty] private FavoriteRowViewModel? _selectedFavorite;

    [ObservableProperty] private bool _isLoading;

    [ObservableProperty] private string _loadingMessage = "";

    public FavoritesSectionViewModel(
        FavoritesManager favoritesManager,
        SystemManagerService systemManager,
        IFindCoverImageService findCoverImage,
        IMameDataService mameData,
        PlaySoundEffects playSoundEffects,
        IMessageBoxLibraryService messageBox,
        IConfiguration configuration,
        MainViewModel mainViewModel,
        ILogger logErrors)
    {
        _favoritesManager = favoritesManager;
        _systemManager = systemManager;
        _findCoverImage = findCoverImage;
        _mameData = mameData;
        _playSoundEffects = playSoundEffects;
        _messageBox = messageBox;
        _configuration = configuration;
        _mainViewModel = mainViewModel;
        _logErrors = logErrors;
    }

    /// <summary>
    /// Loads the favorites from the manager and enriches each row with machine
    /// description, default emulator, and cover image.
    /// </summary>
    public async Task LoadFavoritesAsync()
    {
        try
        {
            IsLoading = true;
            LoadingMessage = "Loading favorites...";

            await Task.Yield();

            var rows = await Task.Run(() =>
            {
                var favorites = _favoritesManager.FavoriteList.ToList();
                var rows = new List<FavoriteRowViewModel>(favorites.Count);

                foreach (var favorite in favorites)
                {
                    // WPF parity: FileName is a bare file name resolved against the
                    // system folders (older entries may hold full paths — kept as-is).
                    var fileNameWithoutExtension = Path.GetFileNameWithoutExtension(favorite.FileName) ?? favorite.FileName;
                    var system = _systemManager.GetSystem(favorite.SystemName);

                    rows.Add(new FavoriteRowViewModel
                    {
                        FilePath = favorite.FileName,
                        SystemName = favorite.SystemName,
                        MachineDescription = _mameData.Lookup.TryGetValue(fileNameWithoutExtension, out var description)
                            ? description
                            : "",
                        DefaultEmulator = system?.Emulators.FirstOrDefault()?.EmulatorName ?? "No Default Emulator",
                        CoverImage = system is null
                            ? ""
                            : _findCoverImage.FindCoverImagePath(
                                fileNameWithoutExtension, favorite.SystemName, system.SystemImageFolder)
                    });
                }

                return rows;
            });

            Favorites = new ObservableCollection<FavoriteRowViewModel>(rows);
            SelectedFavorite = null;
        }
        catch (Exception ex)
        {
            _logErrors.Error(ex, "Error loading favorites in the Favorites section.");
            Favorites = [];
        }
        finally
        {
            IsLoading = false;
        }
    }

    /// <summary>
    /// Resolves a favorite row to an existing file path (WPF FindFileInSystemFolders parity).
    /// Falls back to the stored value so legacy full-path entries keep working.
    /// </summary>
    public string? ResolveFavoritePath(FavoriteRowViewModel row)
    {
        var system = _systemManager.GetSystem(row.SystemName);
        if (system is null) return null;

        return PathHelper.FindFileInSystemFolders(system.SystemFolders, row.FilePath) ?? row.FilePath;
    }

    private string GetLogFilePath()
    {
        return PathHelper.ResolveLogFilePath(_configuration.GetValue<string>("LogPath") ?? "error_user.log");
    }

    /// <summary>
    /// Removes the given favorite rows (WPF RemoveFavoriteButton_ClickAsync parity:
    /// trash sound, per-row removal, preview reset, main-view refresh).
    /// </summary>
    public async Task RemoveFavoritesAsync(IReadOnlyList<FavoriteRowViewModel> rows)
    {
        try
        {
            if (rows.Count == 0)
            {
                _mainViewModel.StatusText = "Select a favorite to remove first.";
                return;
            }

            _playSoundEffects.PlayTrashSound();

            foreach (var row in rows)
            {
                await _favoritesManager.RemoveFavoriteAsync(row.FilePath);
                Favorites.Remove(row);
            }

            SelectedFavorite = null;

            var lastName = rows[^1].DisplayName;
            _mainViewModel.StatusText = rows.Count == 1
                ? $"Removed from favorites: {lastName}"
                : $"Removed {rows.Count} favorites";

            _mainViewModel.RefreshFavoritesAndHistory();
        }
        catch (Exception ex)
        {
            _logErrors.Error(ex, "Error removing favorite from the Favorites section.");
        }
    }

    [RelayCommand]
    private Task RemoveSelectedAsync()
    {
        return RemoveFavoritesAsync(SelectedFavorite is null ? [] : [SelectedFavorite]);
    }

    /// <summary>
    /// Launches the selected favorite (WPF LaunchGameFromFavoriteAsync parity):
    /// resolve against system folders, prompt to delete when the file is gone,
    /// and report missing systems/emulators through message boxes.
    /// </summary>
    [RelayCommand]
    private async Task LaunchSelectedAsync()
    {
        try
        {
            if (SelectedFavorite is not { } favorite)
            {
                _mainViewModel.StatusText = "Select a game to launch first.";
                return;
            }

            _playSoundEffects.PlayNotificationSound();

            var system = _systemManager.GetSystem(favorite.SystemName);
            if (system is null)
            {
                // Expected condition (favorite references a removed system).
                _logErrors.Information("[Favorites] systemManager is null for '{System}'", favorite.SystemName);
                await _messageBox.CouldNotLaunchThisGameMessageBoxAsync(GetLogFilePath());
                return;
            }

            var filePath = ResolveFavoritePath(favorite);
            if (string.IsNullOrEmpty(filePath) || !File.Exists(filePath))
            {
                var result = await _messageBox.FavoriteFileDoesNotExistAskToDeleteMessageBoxAsync(filePath ?? favorite.DisplayName);
                if (result == Core.Models.MessageBoxResult.Yes)
                {
                    await RemoveFavoritesAsync([favorite]);
                }

                return;
            }

            var emulator = system.Emulators.FirstOrDefault();
            if (emulator is null)
            {
                // Expected condition (system has no emulators configured).
                _logErrors.Information("[Favorites] emulatorManager is null for '{System}'", favorite.SystemName);
                await _messageBox.CouldNotLaunchThisGameMessageBoxAsync(GetLogFilePath());
                return;
            }

            await _mainViewModel.LaunchGameAtPathAsync(filePath, favorite.SystemName);
        }
        catch (Exception ex)
        {
            _logErrors.Error(ex, "Error launching favorite from the Favorites section.");
            await _messageBox.CouldNotLaunchThisGameMessageBoxAsync(GetLogFilePath());
        }
    }
}
