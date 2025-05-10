using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using SimpleLauncher.Managers;
using SimpleLauncher.Models;
using SimpleLauncher.Services;

namespace SimpleLauncher;

public partial class FavoritesWindow
{
    private static readonly string LogPath = GetLogPath.Path();
    private readonly FavoritesManager _favoritesManager;
    private ObservableCollection<Favorite> _favoriteList;
    private readonly SettingsManager _settings;
    private readonly List<SystemManager> _systemConfigs;
    private readonly List<MameManager> _machines;
    private readonly MainWindow _mainWindow;

    private readonly Button _fakebutton = new();
    private readonly WrapPanel _fakeGameFileGrid = new();

    public FavoritesWindow(SettingsManager settings, List<SystemManager> systemConfigs, List<MameManager> machines, FavoritesManager favoritesManager, MainWindow mainWindow)
    {
        InitializeComponent();

        _settings = settings;
        _systemConfigs = systemConfigs;
        _machines = machines;
        _mainWindow = mainWindow;
        _favoritesManager = favoritesManager;

        App.ApplyThemeToWindow(this);
        LoadFavorites();
    }

    private void LoadFavorites()
    {
        var favoritesConfig = FavoritesManager.LoadFavorites();
        _favoriteList = [];
        foreach (var favorite in favoritesConfig.FavoriteList)
        {
            // Find machine description if available
            var machine = _machines.FirstOrDefault(m =>
                m.MachineName.Equals(Path.GetFileNameWithoutExtension(favorite.FileName),
                    StringComparison.OrdinalIgnoreCase));
            var machineDescription = machine?.Description ?? string.Empty;

            // Retrieve the system configuration for the favorite
            var systemConfig = _systemConfigs.FirstOrDefault(config =>
                config.SystemName.Equals(favorite.SystemName, StringComparison.OrdinalIgnoreCase));

            // Get the default emulator, e.g., the first one in the list
            var defaultEmulator = systemConfig?.Emulators.FirstOrDefault()?.EmulatorName ?? "Unknown";

            var favoriteItem = new Favorite
            {
                FileName = favorite.FileName,
                SystemName = favorite.SystemName,
                MachineDescription = machineDescription,
                DefaultEmulator = defaultEmulator,
                CoverImage = GetCoverImagePath(favorite.SystemName, favorite.FileName)
            };
            _favoriteList.Add(favoriteItem);
        }

        FavoritesDataGrid.ItemsSource = _favoriteList;
    }

    private string GetCoverImagePath(string systemName, string fileName)
    {
        var baseDirectory = AppDomain.CurrentDomain.BaseDirectory;
        var fileNameWithoutExtension = Path.GetFileNameWithoutExtension(fileName);
        var systemConfig = _systemConfigs.FirstOrDefault(config => config.SystemName.Equals(systemName, StringComparison.OrdinalIgnoreCase));
        var defaultImagePath = Path.Combine(baseDirectory, "images", "default.png");

        if (systemConfig == null)
        {
            return defaultImagePath;
        }
        else
        {
            return FindCoverImage.FindCoverImagePath(fileNameWithoutExtension, systemName, systemConfig);
        }
    }

    private void RemoveFavoriteButton_Click(object sender, RoutedEventArgs e)
    {
        if (FavoritesDataGrid.SelectedItem is Favorite selectedFavorite)
        {
            _favoriteList.Remove(selectedFavorite);
            _favoritesManager.FavoriteList = _favoriteList; // Keep the instance in sync
            _favoritesManager.SaveFavorites(); // Save using the existing instance

            PlayClick.PlayTrashSound();
            PreviewImage.Source = null;
        }
        else
        {
            // Notify user
            MessageBoxLibrary.SelectAFavoriteToRemoveMessageBox();
        }
    }

    private void FavoritesWindowRightClickContextMenu(object sender, MouseButtonEventArgs e)
    {
        try
        {
            if (FavoritesDataGrid.SelectedItem is not Favorite selectedFavorite) return;

            var systemConfig = _systemConfigs.FirstOrDefault(config => config.SystemName.Equals(selectedFavorite.SystemName, StringComparison.OrdinalIgnoreCase));
            if (systemConfig == null)
            {
                // Notify developer
                const string contextMessage = "systemConfig is null for the selected favorite";
                var ex = new Exception(contextMessage);
                _ = LogErrors.LogErrorAsync(ex, contextMessage);

                // Notify user
                MessageBoxLibrary.RightClickContextMenuErrorMessageBox();

                return;
            }

            var fileNameWithExtension = selectedFavorite.FileName;
            var fileNameWithoutExtension = Path.GetFileNameWithoutExtension(selectedFavorite.FileName);
            var systemFolderPath = PathHelper.ResolveRelativeToAppDirectory(systemConfig.SystemFolder);
            var filePath = PathHelper.CombineAndResolveRelativeToCurrentDirectory(systemFolderPath, selectedFavorite.FileName);

            AddRightClickContextMenuFavoritesWindow(fileNameWithExtension, selectedFavorite, fileNameWithoutExtension, systemConfig, filePath);
        }
        catch (Exception ex)
        {
            // Notify developer
            const string contextMessage = "There was an error in the right-click context menu.";
            _ = LogErrors.LogErrorAsync(ex, contextMessage);

            // Notify user
            MessageBoxLibrary.RightClickContextMenuErrorMessageBox();
        }
    }

    private async void LaunchGame_Click(object sender, RoutedEventArgs e)
    {
        try
        {
            if (FavoritesDataGrid.SelectedItem is Favorite selectedFavorite)
            {
                PlayClick.PlayClickSound();
                await LaunchGameFromFavorite(selectedFavorite.FileName, selectedFavorite.SystemName);
            }
            else
            {
                // Notify user
                MessageBoxLibrary.SelectAGameToLaunchMessageBox();
            }
        }
        catch (Exception ex)
        {
            // Notify developer
            const string contextMessage = "Error in the LaunchGame_Click method.";
            _ = LogErrors.LogErrorAsync(ex, contextMessage);

            // Notify user
            MessageBoxLibrary.CouldNotLaunchThisGameMessageBox(LogPath);
        }
    }

    private async Task LaunchGameFromFavorite(string fileName, string systemName)
    {
        try
        {
            var systemConfig = _systemConfigs.FirstOrDefault(config => config.SystemName.Equals(systemName, StringComparison.OrdinalIgnoreCase));
            if (systemConfig == null)
            {
                // Notify developer
                const string contextMessage = "systemConfig is null.";
                var ex = new Exception(contextMessage);
                _ = LogErrors.LogErrorAsync(ex, contextMessage);

                // Notify user
                MessageBoxLibrary.CouldNotLaunchThisGameMessageBox(LogPath);

                return;
            }

            var emulatorConfig = systemConfig.Emulators.FirstOrDefault();
            if (emulatorConfig == null)
            {
                // Notify developer
                const string contextMessage = "emulatorConfig is null.";
                var ex = new Exception(contextMessage);
                _ = LogErrors.LogErrorAsync(ex, contextMessage);

                // Notify user
                MessageBoxLibrary.CouldNotLaunchThisGameMessageBox(LogPath);

                return;
            }

            var partialPath = PathHelper.CombineAndResolveRelativeToCurrentDirectory(systemConfig.SystemFolder, fileName);
            var fullPath = PathHelper.ResolveRelativeToAppDirectory(partialPath);

            if (!File.Exists(fullPath))
            {
                // Auto remove the favorite from the list since the file no longer exists
                var favoriteToRemove = _favoriteList.FirstOrDefault(fav => fav.FileName == fileName && fav.SystemName == systemName);
                if (favoriteToRemove != null)
                {
                    _favoriteList.Remove(favoriteToRemove);
                    _favoritesManager.FavoriteList = _favoriteList;
                    _favoritesManager.SaveFavorites();
                }

                // Notify developer
                var contextMessage = $"Favorite file does not exist: {fullPath}";
                var ex = new Exception(contextMessage);
                _ = LogErrors.LogErrorAsync(ex, contextMessage);

                // Notify user
                MessageBoxLibrary.GameFileDoesNotExistMessageBox();

                return;
            }

            var mockSystemComboBox = new ComboBox();
            var mockEmulatorComboBox = new ComboBox();
            mockSystemComboBox.ItemsSource = _systemConfigs.Select(static config => config.SystemName).ToList();
            mockSystemComboBox.SelectedItem = systemConfig.SystemName;
            mockEmulatorComboBox.ItemsSource = systemConfig.Emulators.Select(static emulator => emulator.EmulatorName).ToList();
            mockEmulatorComboBox.SelectedItem = emulatorConfig.EmulatorName;

            // Launch Game
            await GameLauncher.HandleButtonClick(fullPath, mockEmulatorComboBox, mockSystemComboBox, _systemConfigs, _settings, _mainWindow);
        }
        catch (Exception ex)
        {
            // Notify developer
            var contextMessage = $"There was an error launching the game from Favorites.\n" +
                                 $"File Path: {fileName}\n" +
                                 $"System Name: {systemName}";
            _ = LogErrors.LogErrorAsync(ex, contextMessage);

            // Notify user
            MessageBoxLibrary.CouldNotLaunchThisGameMessageBox(LogPath);
        }
    }

    private void RemoveFavoriteFromXmlAndEmptyPreviewImage(Favorite selectedFavorite)
    {
        _favoriteList.Remove(selectedFavorite);
        _favoritesManager.FavoriteList = _favoriteList;
        _favoritesManager.SaveFavorites();

        PreviewImage.Source = null;
    }

    private async void LaunchGameWithDoubleClick(object sender, MouseButtonEventArgs e)
    {
        try
        {
            if (FavoritesDataGrid.SelectedItem is not Favorite selectedFavorite) return;

            PlayClick.PlayClickSound();
            await LaunchGameFromFavorite(selectedFavorite.FileName, selectedFavorite.SystemName);
        }
        catch (Exception ex)
        {
            // Notify developer
            const string contextMessage = "Error in the method MouseDoubleClick.";
            _ = LogErrors.LogErrorAsync(ex, contextMessage);

            // Notify user
            MessageBoxLibrary.CouldNotLaunchThisGameMessageBox(LogPath);
        }
    }

    private async void SetPreviewImageOnSelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        try
        {
            if (FavoritesDataGrid.SelectedItem is not Favorite selectedFavorite)
            {
                PreviewImage.Source = null; // Clear preview if nothing is selected
                return;
            }

            var imagePath = selectedFavorite.CoverImage;

            // Use the new ImageLoader to load the image
            var (loadedImage, _) = await ImageLoader.LoadImageAsync(imagePath);

            // Assign the loaded image to the PreviewImage control
            PreviewImage.Source = loadedImage;
        }
        catch (Exception ex)
        {
            _ = LogErrors.LogErrorAsync(ex, "Error in the SetPreviewImageOnSelectionChanged method.");
        }
    }

    private void DeleteFavoriteWithDelButton(object sender, KeyEventArgs e)
    {
        if (e.Key != Key.Delete) return;

        PlayClick.PlayTrashSound();

        if (FavoritesDataGrid.SelectedItem is Favorite selectedFavorite)
        {
            _favoriteList.Remove(selectedFavorite);
            _favoritesManager.FavoriteList = _favoriteList;
            _favoritesManager.SaveFavorites();
        }
        else
        {
            MessageBoxLibrary.SelectAFavoriteToRemoveMessageBox();
        }
    }

    private bool GetSystemConfigOfSelectedFavorite(Favorite selectedFavorite, out SystemManager systemManager)
    {
        systemManager = _systemConfigs?.FirstOrDefault(config =>
            config.SystemName.Equals(selectedFavorite.SystemName, StringComparison.OrdinalIgnoreCase));

        if (systemManager != null) return false;

        // Notify developer
        const string contextMessage = "systemManager is null.";
        var ex = new Exception(contextMessage);
        _ = LogErrors.LogErrorAsync(ex, contextMessage);

        // Notify user
        MessageBoxLibrary.ErrorOpeningCoverImageMessageBox();

        return true;
    }
}
