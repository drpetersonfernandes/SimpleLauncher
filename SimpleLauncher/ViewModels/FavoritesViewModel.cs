#nullable enable

using System.Collections.ObjectModel;
using System.Diagnostics.CodeAnalysis;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Extensions.Configuration;
using SimpleLauncher.Interfaces;
using SimpleLauncher.Models;
using SimpleLauncher.Services.Favorites;
using SimpleLauncher.Services.MameManager;
using SimpleLauncher.Services.PlaySound;
using SimpleLauncher.Services.SettingsManager;
using PathHelper = SimpleLauncher.Services.CheckPaths.PathHelper;
using SystemManager = SimpleLauncher.Services.SystemManager.SystemManager;
using CoreMessageBoxResult = SimpleLauncher.Interfaces.MessageBoxResult;

namespace SimpleLauncher.ViewModels;

[SuppressMessage("ReSharper", "NotAccessedField.Local")]
public partial class FavoritesViewModel : ObservableObject, IDisposable
{
    private readonly IConfiguration _configuration;
    private readonly ILogErrors _logErrors;
    private readonly FavoritesManager _favoritesManager;
    private readonly SettingsManager _settings;
    private readonly List<SystemManager> _systemManagers;
    private readonly List<MameManager> _machines;
    private readonly PlaySoundEffects _playSoundEffects;
    private readonly IFindCoverImageService _findCoverImage;
    private readonly IImageLoader _imageLoader;
    private readonly IMessageBoxLibraryService _messageBox;
    private readonly IResourceProvider _resourceProvider;

    [ObservableProperty] private ObservableCollection<Favorite> _favorites = [];

    [ObservableProperty] private Favorite? _selectedFavorite;

    [ObservableProperty] private Stream? _previewImageSource;

    partial void OnPreviewImageSourceChanging(Stream? value)
    {
        value?.Dispose();
    }

    [ObservableProperty] private bool _isLoading;

    [ObservableProperty] private string _loadingMessage = "";

    public FavoritesViewModel(
        IConfiguration configuration,
        ILogErrors logErrors,
        FavoritesManager favoritesManager,
        SettingsManager settings,
        List<SystemManager> systemManagers,
        List<MameManager> machines,
        PlaySoundEffects playSoundEffects,
        IFindCoverImageService findCoverImage,
        IImageLoader imageLoader,
        IMessageBoxLibraryService messageBox,
        IResourceProvider resourceProvider)
    {
        _configuration = configuration;
        _logErrors = logErrors;
        _favoritesManager = favoritesManager;
        _settings = settings;
        _systemManagers = systemManagers;
        _machines = machines;
        _playSoundEffects = playSoundEffects;
        _findCoverImage = findCoverImage;
        _imageLoader = imageLoader;
        _messageBox = messageBox;
        _resourceProvider = resourceProvider;
    }

    public async Task LoadFavoritesAsync()
    {
        try
        {
            IsLoading = true;
            LoadingMessage = _resourceProvider.GetString("LoadingFavorites", "Loading favorites...");

            await Task.Yield();

            var favoritesSnapshot = _favoritesManager.FavoriteList.ToList();
            var systemManagersSnapshot = _systemManagers.ToList();
            var machinesSnapshot = _machines.ToList();

            var processedFavorites = await Task.Run(() =>
            {
                var processedList = new List<Favorite>();
                foreach (var favoriteConfigItem in favoritesSnapshot)
                {
                    var machine = machinesSnapshot.FirstOrDefault(m =>
                        m.MachineName.Equals(Path.GetFileNameWithoutExtension(favoriteConfigItem.FileName), StringComparison.OrdinalIgnoreCase));

                    var machineDescription = machine?.Description ?? "";

                    var systemManager = systemManagersSnapshot.FirstOrDefault(manager =>
                        manager.SystemName.Equals(favoriteConfigItem.SystemName, StringComparison.OrdinalIgnoreCase));

                    var defaultEmulator = systemManager?.Emulators.FirstOrDefault()?.EmulatorName
                                          ?? _resourceProvider.GetString("UnknownString", "Unknown");

                    var coverImagePath = GetCoverImagePath(favoriteConfigItem.SystemName, favoriteConfigItem.FileName);

                    processedList.Add(new Favorite
                    {
                        FileName = favoriteConfigItem.FileName,
                        SystemName = favoriteConfigItem.SystemName,
                        MachineDescription = machineDescription,
                        DefaultEmulator = defaultEmulator,
                        CoverImage = coverImagePath
                    });
                }

                return processedList;
            });

            Favorites = new ObservableCollection<Favorite>(processedFavorites);
        }
        catch (Exception ex)
        {
            _logErrors.LogAndForget(ex, "Error loading favorites data in FavoritesViewModel.");
            await _messageBox.ErrorWhileAddingFavoritesMessageBoxAsync();
        }
        finally
        {
            IsLoading = false;
        }
    }

    [RelayCommand]
    private async Task RemoveFavoriteAsync()
    {
        try
        {
            if (SelectedFavorite == null)
            {
                await _messageBox.SelectAFavoriteToRemoveMessageBoxAsync();
                return;
            }

            _playSoundEffects.PlayTrashSound();

            Favorites.Remove(SelectedFavorite);

            UpdateFavoritesManagerList();

            PreviewImageSource = null;
        }
        catch (Exception ex)
        {
            _logErrors.LogAndForget(ex, "Error in RemoveFavoriteAsync.");
        }
    }

    [RelayCommand]
    private async Task LaunchGameAsync()
    {
        try
        {
            if (SelectedFavorite == null)
            {
                await _messageBox.SelectAGameToLaunchMessageBoxAsync();
                return;
            }

            _playSoundEffects.PlayNotificationSound();
            await LaunchGameFromFavoriteAsync(SelectedFavorite.FileName, SelectedFavorite.SystemName);
        }
        catch (Exception ex)
        {
            _logErrors.LogAndForget(ex, "Error in LaunchGameAsync.");
            await _messageBox.CouldNotLaunchThisGameMessageBoxAsync(
                PathHelper.ResolveRelativeToAppDirectory(_configuration.GetValue("LogPath", "error_user.log")));
        }
    }

    public async Task LaunchGameFromFavoriteAsync(string fileName, string selectedSystemName, ILoadingState? loadingStateProvider = null)
    {
        try
        {
            var selectedSystemManager = _systemManagers.FirstOrDefault(manager => manager.SystemName.Equals(selectedSystemName, StringComparison.OrdinalIgnoreCase));
            if (selectedSystemManager == null)
            {
                _logErrors.LogAndForget(null, "[LaunchGameFromFavoritesAsync] selectedSystemManager is null.");
                await _messageBox.CouldNotLaunchThisGameMessageBoxAsync(
                    PathHelper.ResolveRelativeToAppDirectory(_configuration.GetValue("LogPath", "error_user.log")));
                return;
            }

            var filePath = PathHelper.FindFileInSystemFolders(selectedSystemManager.SystemFolders, fileName);
            if (string.IsNullOrEmpty(filePath) || !File.Exists(filePath))
            {
                var result = await _messageBox.FavoriteFileDoesNotExistAskToDeleteMessageBoxAsync(filePath ?? fileName);
                if (result == CoreMessageBoxResult.Yes)
                {
                    var favoriteToRemove = Favorites.FirstOrDefault(fav => fav.FileName.Equals(fileName, StringComparison.OrdinalIgnoreCase)
                                                                           && fav.SystemName.Equals(selectedSystemName, StringComparison.OrdinalIgnoreCase));
                    if (favoriteToRemove != null)
                    {
                        RemoveFavoriteFromCollection(favoriteToRemove);
                    }
                }

                _logErrors.LogAndForget(null, $"[LaunchGameFromFavoritesAsync] File does not exist: {filePath}");
                return;
            }

            var emulatorManager = selectedSystemManager.Emulators.FirstOrDefault();
            if (emulatorManager == null)
            {
                _logErrors.LogAndForget(null, "[LaunchGameFromFavoritesAsync] emulatorManager is null.");
                await _messageBox.CouldNotLaunchThisGameMessageBoxAsync(
                    PathHelper.ResolveRelativeToAppDirectory(_configuration.GetValue("LogPath", "error_user.log")));
            }

            // Game launching is handled by the caller (code-behind) since it needs WPF Window context
            // This method provides the data needed for launching
        }
        catch (Exception ex)
        {
            _logErrors.LogAndForget(ex, $"[LaunchGameFromFavoritesAsync] Error launching: {fileName}, {selectedSystemName}");
            await _messageBox.CouldNotLaunchThisGameMessageBoxAsync(
                PathHelper.ResolveRelativeToAppDirectory(_configuration.GetValue("LogPath", "error_user.log")));
        }
    }

    public async Task UpdatePreviewImageAsync(string? imagePath)
    {
        try
        {
            if (string.IsNullOrEmpty(imagePath))
            {
                PreviewImageSource = null;
                return;
            }

            var (imageStream, _) = await _imageLoader.LoadImageAsync(imagePath);
            PreviewImageSource = imageStream;
        }
        catch (Exception ex)
        {
            _logErrors.LogAndForget(ex, "Error loading preview image.");
        }
    }

    public void RemoveFavoriteFromCollection(Favorite favorite)
    {
        Favorites.Remove(favorite);
        UpdateFavoritesManagerList();
        PreviewImageSource = null;
    }

    private void UpdateFavoritesManagerList()
    {
        _favoritesManager.FavoriteList.Clear();
        foreach (var favorite in Favorites)
        {
            _favoritesManager.FavoriteList.Add(favorite);
        }

        _favoritesManager.SaveFavoritesAsync();
    }

    private string GetCoverImagePath(string systemName, string fileName)
    {
        var baseDirectory = AppDomain.CurrentDomain.BaseDirectory;
        var fileNameWithoutExtension = Path.GetFileNameWithoutExtension(fileName);
        var systemManager = _systemManagers.FirstOrDefault(manager => manager.SystemName.Equals(systemName, StringComparison.OrdinalIgnoreCase));
        var defaultImagePath = Path.Combine(baseDirectory, "images", "default.png");

        if (systemManager == null)
        {
            return defaultImagePath;
        }

        return _findCoverImage.FindCoverImagePath(fileNameWithoutExtension, systemName, systemManager.SystemImageFolder);
    }

    public SystemManager? GetSystemManager(string systemName)
    {
        return _systemManagers.FirstOrDefault(manager => manager.SystemName.Equals(systemName, StringComparison.OrdinalIgnoreCase));
    }

    public void Dispose()
    {
        PreviewImageSource?.Dispose();
        PreviewImageSource = null;
        GC.SuppressFinalize(this);
    }
}