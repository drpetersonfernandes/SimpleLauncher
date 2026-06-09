using System.Collections.ObjectModel;
using System.Diagnostics.CodeAnalysis;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Extensions.Configuration;
using SimpleLauncher.Core.Interfaces;
using SimpleLauncher.Core.Models;
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

    public FavoritesViewModel(
        IConfiguration configuration,
        IImageLoader imageLoader,
        IMessageBoxLibraryService messageBox,
        IMessageDialogService messageDialog,
        SettingsManager settings)
    {
        _configuration = configuration;
        _imageLoader = imageLoader;
        _messageBox = messageBox;
        _messageDialog = messageDialog;
        _settings = settings;
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

        // TODO: Implement game launching
        await _messageDialog.ShowInfoAsync("Game launching will be implemented soon.", "Launch Game");
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
