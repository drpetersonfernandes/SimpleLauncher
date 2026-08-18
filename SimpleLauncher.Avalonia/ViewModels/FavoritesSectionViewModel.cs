using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using SimpleLauncher.Avalonia.Services.Favorites;
using SimpleLauncher.Avalonia.Services.SystemManager;
using SimpleLauncher.Core.Interfaces;
using SimpleLauncher.Core.Services.PlaySound;

namespace SimpleLauncher.Avalonia.ViewModels;

/// <summary>
/// ViewModel for the Favorites section of the main window, showing the user's
/// favorite games in a table with preview, removal, and launching (WPF FavoritesPage equivalent).
/// </summary>
public partial class FavoritesSectionViewModel : ObservableObject
{
    private readonly FavoritesManager _favoritesManager;
    private readonly SystemManagerService _systemManager;
    private readonly IFindCoverImageService _findCoverImage;
    private readonly IMameDataService _mameData;
    private readonly PlaySoundEffects _playSoundEffects;
    private readonly IMessageBoxLibraryService _messageBox;
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
        MainViewModel mainViewModel,
        ILogger logErrors)
    {
        _favoritesManager = favoritesManager;
        _systemManager = systemManager;
        _findCoverImage = findCoverImage;
        _mameData = mameData;
        _playSoundEffects = playSoundEffects;
        _messageBox = messageBox;
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

    [RelayCommand]
    private async Task RemoveSelectedAsync()
    {
        try
        {
            if (SelectedFavorite is not { } favorite)
            {
                _mainViewModel.StatusText = "Select a favorite to remove first.";
                return;
            }

            _playSoundEffects.PlayTrashSound();

            await _favoritesManager.RemoveFavoriteAsync(favorite.FilePath);
            Favorites.Remove(favorite);
            SelectedFavorite = null;
            _mainViewModel.StatusText = $"Removed from favorites: {favorite.DisplayName}";

            _mainViewModel.RefreshFavoritesAndHistory();
        }
        catch (Exception ex)
        {
            _logErrors.Error(ex, "Error removing favorite from the Favorites section.");
        }
    }

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

            if (!File.Exists(favorite.FilePath))
            {
                // Expected condition (favorite points to a missing file) — keep out of the bug report service.
                _logErrors.Information("Favorite file does not exist: {Path}", favorite.FilePath);
                _mainViewModel.StatusText = $"File does not exist: {favorite.DisplayName}";
                return;
            }

            await _mainViewModel.LaunchGameAtPathAsync(favorite.FilePath, favorite.SystemName);
        }
        catch (Exception ex)
        {
            _logErrors.Error(ex, "Error launching favorite from the Favorites section.");
        }
    }
}
