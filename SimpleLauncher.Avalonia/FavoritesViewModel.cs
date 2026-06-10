using System.Collections.ObjectModel;
using System.Diagnostics.CodeAnalysis;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using SimpleLauncher.Avalonia.Services;
using SimpleLauncher.Core.Interfaces;
using SimpleLauncher.Core.Models;
using SimpleLauncher.Core.Services.CheckPaths;
using SimpleLauncher.Core.Services.SettingsManager;

namespace SimpleLauncher.Avalonia;

[SuppressMessage("ReSharper", "NotAccessedField.Local")]
public partial class FavoritesViewModel : ObservableObject, IDisposable
{
    private readonly IConfiguration _configuration;
    private readonly IImageLoader _imageLoader;
    private readonly IMessageBoxLibraryService _messageBox;
    private readonly IMessageDialogService _messageDialog;
    private readonly SettingsManager _settings;
    private readonly GameLauncherService _gameLauncher;
    private readonly IFindCoverImageService _findCoverImage;

    public FavoritesViewModel(
        IConfiguration configuration,
        IImageLoader imageLoader,
        IMessageBoxLibraryService messageBox,
        IMessageDialogService messageDialog,
        SettingsManager settings,
        GameLauncherService gameLauncher,
        IFindCoverImageService findCoverImage)
    {
        _configuration = configuration;
        _imageLoader = imageLoader;
        _messageBox = messageBox;
        _messageDialog = messageDialog;
        _settings = settings;
        _gameLauncher = gameLauncher;
        _findCoverImage = findCoverImage;
    }

    // ── Collections ─────────────────────────────────────────────

    [ObservableProperty] private ObservableCollection<Favorite> _favorites = [];

    [ObservableProperty] private Favorite? _selectedFavorite;

    // ── Preview Image ───────────────────────────────────────────

    [ObservableProperty] private Stream? _previewImageSource;

    // ── Loading State ───────────────────────────────────────────

    [ObservableProperty] private bool _isLoading;

    [ObservableProperty] private string _loadingMessage = string.Empty;

    // ── Empty State ─────────────────────────────────────────────

    [ObservableProperty] private bool _isEmpty = true;

    // ── Commands ────────────────────────────────────────────────

    [RelayCommand]
    private async Task RemoveFavoriteAsync()
    {
        if (SelectedFavorite == null)
        {
            await _messageBox.SelectAFavoriteToRemoveMessageBox();
            return;
        }

        var result = await _messageDialog.ShowYesNoAsync(
            $"Remove '{SelectedFavorite.FileName}' from favorites?",
            "Remove Favorite");

        if (result)
        {
            Favorites.Remove(SelectedFavorite);
            await SaveFavoritesAsync();
            IsEmpty = Favorites.Count == 0;
            SelectedFavorite = null;
            PreviewImageSource = null;
        }
    }

    [RelayCommand]
    private async Task LaunchGameAsync()
    {
        if (SelectedFavorite == null)
        {
            await _messageDialog.ShowInfoAsync("Please select a game to launch.", "Launch Game");
            return;
        }

        // Find the system manager for this favorite
        var systemConfigService = App.ServiceProvider.GetRequiredService<ICoreSystemConfigurationService>();
        var systemManagers = systemConfigService.LoadSystemManagers();
        var systemManager = systemManagers.FirstOrDefault(s =>
            s.SystemName.Equals(SelectedFavorite.SystemName, StringComparison.OrdinalIgnoreCase));

        if (systemManager == null)
        {
            await _messageDialog.ShowErrorAsync($"System '{SelectedFavorite.SystemName}' not found.", "Launch Error");
            return;
        }

        var emulatorName = SelectedFavorite.DefaultEmulator ?? systemManager.Emulators.FirstOrDefault()?.EmulatorName;
        if (string.IsNullOrEmpty(emulatorName))
        {
            await _messageDialog.ShowErrorAsync("No emulator configured for this system.", "Launch Error");
            return;
        }

        var resolvedPath = PathHelper.FindFileInSystemFolders(systemManager.SystemFolders.ToList(), SelectedFavorite.FileName);
        if (string.IsNullOrEmpty(resolvedPath))
        {
            await _messageDialog.ShowErrorAsync($"Could not find game file '{SelectedFavorite.FileName}' in system folders.", "Launch Error");
            return;
        }

        await _gameLauncher.LaunchGameAsync(resolvedPath, emulatorName, systemManager, _settings);
    }

    // ── Public Methods ──────────────────────────────────────────

    public async Task LoadFavoritesAsync()
    {
        IsLoading = true;
        LoadingMessage = "Loading favorites...";

        try
        {
            var favoritesPath = GetFavoritesPath();
            if (!File.Exists(favoritesPath))
            {
                Favorites = [];
                IsEmpty = true;
                return;
            }

            var favorites = await LoadFavoritesFromFileAsync(favoritesPath);

            // Populate cover images
            var systemConfigService = App.ServiceProvider.GetRequiredService<ICoreSystemConfigurationService>();
            var systemManagers = systemConfigService.LoadSystemManagers();

            foreach (var fav in favorites)
            {
                var systemManager = systemManagers.FirstOrDefault(s =>
                    s.SystemName.Equals(fav.SystemName, StringComparison.OrdinalIgnoreCase));

                if (systemManager != null)
                {
                    var fileName = Path.GetFileNameWithoutExtension(fav.FileName);
                    fav.CoverImage = _findCoverImage.FindCoverImagePath(fileName, fav.SystemName, systemManager.SystemImageFolder);
                    fav.DefaultEmulator ??= systemManager.Emulators.FirstOrDefault()?.EmulatorName;
                }
            }

            Favorites = new ObservableCollection<Favorite>(favorites);
            IsEmpty = Favorites.Count == 0;
        }
        catch
        {
            Favorites = [];
            IsEmpty = true;
        }
        finally
        {
            IsLoading = false;
        }
    }

    public async Task UpdatePreviewImageAsync(string? imagePath)
    {
        if (string.IsNullOrEmpty(imagePath))
        {
            PreviewImageSource = null;
            return;
        }

        try
        {
            var (stream, _) = await _imageLoader.LoadImageAsync(imagePath);
            PreviewImageSource = stream;
        }
        catch
        {
            PreviewImageSource = null;
        }
    }

    public void RemoveFavoriteFromCollection(Favorite favorite)
    {
        Favorites.Remove(favorite);
        IsEmpty = Favorites.Count == 0;
    }

    // ── Private Methods ─────────────────────────────────────────

    private string GetFavoritesPath()
    {
        var basePath = _configuration.GetValue<string>("FavoritesPath") ?? "favorites";
        return Path.Combine(AppDomain.CurrentDomain.BaseDirectory, basePath, "favorites.bin");
    }

    private static async Task<List<Favorite>> LoadFavoritesFromFileAsync(string path)
    {
        try
        {
            var bytes = await File.ReadAllBytesAsync(path);
            return MessagePack.MessagePackSerializer.Deserialize<List<Favorite>>(bytes);
        }
        catch
        {
            return [];
        }
    }

    private async Task SaveFavoritesAsync()
    {
        try
        {
            var path = GetFavoritesPath();
            var directory = Path.GetDirectoryName(path);
            if (directory != null && !Directory.Exists(directory))
            {
                Directory.CreateDirectory(directory);
            }

            var bytes = MessagePack.MessagePackSerializer.Serialize(Favorites.ToList());
            await File.WriteAllBytesAsync(path, bytes);
        }
        catch
        {
            // Log error
        }
    }

    // ── IDisposable ─────────────────────────────────────────────

    public void Dispose()
    {
        PreviewImageSource?.Dispose();
        GC.SuppressFinalize(this);
    }
}
