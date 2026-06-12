using System.Windows.Controls;
using Microsoft.Extensions.Configuration;
using SimpleLauncher.Interfaces;
using SimpleLauncher.Models;
using SimpleLauncher.Services.Favorites;
using SimpleLauncher.Services.GameItemFactory;
using SimpleLauncher.Services.GameListUI;
using SimpleLauncher.Services.GamePad;
using SimpleLauncher.Services.PlayHistory;
using SimpleLauncher.Services.PlaySound;

namespace SimpleLauncher.Services.GameItemRender;

/// <summary>
/// Renders game items in either grid or list view mode, delegating to
/// <see cref="GameButtonFactory"/> or <see cref="GameListFactory"/> as appropriate.
/// </summary>
public class GameItemRenderService : IGameItemRenderService
{
    private const int BatchSize = 100;

    private readonly SettingsManager.SettingsManager _settings;
    private readonly FavoritesManager _favoritesManager;
    private readonly PlayHistoryManager _playHistoryManager;
    private readonly GamePadController _gamePadController;
    private readonly GameLauncher.GameLauncher _gameLauncher;
    private readonly PlaySoundEffects _playSoundEffects;
    private readonly IConfiguration _configuration;
    private readonly ILogErrors _logErrors;
    private readonly IGetListOfFilesService _getListOfFiles;
    private readonly IFindCoverImageService _findCoverImage;
    private readonly IImageLoader _imageLoader;
    private readonly GameListUiService _gameListUiService;
    private readonly IMessageBoxLibraryService _messageBox;
    private readonly IRetroAchievementsHasherTool _raHasherTool;
    private readonly IContextMenuFunctions _contextMenuFunctions;
    private readonly IDebugLogger _debugLogger;
    private readonly IContextMenuService _contextMenuService;

    private IGameItemRenderHost _host;
    private GameButtonFactory _gameButtonFactory;
    private GameListFactory _gameListFactory;

    /// <summary>
    /// Initializes a new instance of <see cref="GameItemRenderService"/> with all required dependencies.
    /// </summary>
    public GameItemRenderService(
        SettingsManager.SettingsManager settings,
        FavoritesManager favoritesManager,
        PlayHistoryManager playHistoryManager,
        GamePadController gamePadController,
        GameLauncher.GameLauncher gameLauncher,
        PlaySoundEffects playSoundEffects,
        IConfiguration configuration,
        ILogErrors logErrors,
        IGetListOfFilesService getListOfFiles,
        IFindCoverImageService findCoverImage,
        IImageLoader imageLoader,
        GameListUiService gameListUiService,
        IMessageBoxLibraryService messageBox,
        IRetroAchievementsHasherTool raHasherTool,
        IContextMenuFunctions contextMenuFunctions,
        IDebugLogger debugLogger,
        IContextMenuService contextMenuService)
    {
        _settings = settings;
        _favoritesManager = favoritesManager;
        _playHistoryManager = playHistoryManager;
        _gamePadController = gamePadController;
        _gameLauncher = gameLauncher;
        _playSoundEffects = playSoundEffects;
        _configuration = configuration;
        _logErrors = logErrors;
        _getListOfFiles = getListOfFiles;
        _findCoverImage = findCoverImage;
        _imageLoader = imageLoader;
        _gameListUiService = gameListUiService;
        _messageBox = messageBox;
        _raHasherTool = raHasherTool;
        _contextMenuFunctions = contextMenuFunctions;
        _debugLogger = debugLogger ?? throw new ArgumentNullException(nameof(debugLogger));
        _contextMenuService = contextMenuService ?? throw new ArgumentNullException(nameof(contextMenuService));
    }

    /// <summary>
    /// Binds the render service to the host that provides UI controls and dispatchers.
    /// </summary>
    public void Initialize(IGameItemRenderHost host)
    {
        _host = host;
    }

    /// <summary>
    /// Rebuilds the grid and list view factories with updated system and MAME machine data.
    /// </summary>
    public void ReloadFactories(List<SystemManager.SystemManager> systemManagers, List<MameManager.MameManager> machines)
    {
        _gameButtonFactory = new GameButtonFactory(
            _host.EmulatorComboBox,
            _host.SystemComboBox,
            systemManagers,
            machines,
            _settings,
            _favoritesManager,
            _host.GameFileGrid,
            _host.MainWindow,
            _gamePadController,
            _gameLauncher,
            _playSoundEffects,
            _logErrors,
            _getListOfFiles,
            _findCoverImage,
            _imageLoader,
            _messageBox,
            _raHasherTool,
            _contextMenuFunctions,
            _debugLogger,
            _contextMenuService);

        _gameListFactory = new GameListFactory(
            _host.EmulatorComboBox,
            _host.SystemComboBox,
            systemManagers,
            machines,
            _settings,
            _favoritesManager,
            _playHistoryManager,
            _host.MainWindow,
            _gamePadController,
            _gameLauncher,
            _playSoundEffects,
            _configuration,
            _logErrors,
            _getListOfFiles,
            _findCoverImage,
            _imageLoader,
            _messageBox);
    }

    /// <summary>
    /// Renders the game items in the current view mode (grid or list) by delegating to the appropriate factory.
    /// </summary>
    public Task RenderGameItemsAsync(IList<string> files, string systemName, SystemManager.SystemManager systemManager, CancellationToken ct)
    {
        if (_settings.ViewMode == "GridView")
        {
            return RenderGridViewAsync(files, systemName, systemManager, ct);
        }
        else
        {
            return RenderListViewAsync(files, systemName, systemManager, ct);
        }
    }

    /// <summary>
    /// Handles a double-click on a game item in the list view by launching the game.
    /// </summary>
    public Task HandleDoubleClickAsync(GameListViewItem selectedItem)
    {
        return _gameListFactory.HandleDoubleClickAsync(selectedItem);
    }

    /// <summary>
    /// Handles a selection change in the list view by updating the preview image.
    /// </summary>
    public Task HandleSelectionChangedAsync(GameListViewItem selectedItem)
    {
        return _gameListFactory.HandleSelectionChangedAsync(selectedItem);
    }

    /// <summary>
    /// Clears all rendered game items from both the grid and list views.
    /// </summary>
    public void ClearRenderedItems()
    {
        _host.GameFileGrid.Dispatcher.Invoke(() =>
        {
            GameListUiService.ClearGameButtonImages(_host.GameFileGrid);
            _host.GameFileGrid.Children.Clear();
        });
        _host.Dispatcher.Invoke(() => _host.GameListItems.Clear());
    }

    /// <summary>
    /// Enables or disables all game item buttons in the UI.
    /// </summary>
    public void SetGameButtonsEnabled(bool isEnabled)
    {
        _gameListUiService.SetGameButtonsEnabled(isEnabled);
    }

    /// <summary>
    /// Gets or sets the height (in pixels) of game item thumbnail images in the grid view.
    /// </summary>
    public int ImageHeight
    {
        get => _gameButtonFactory?.ImageHeight ?? _settings.ThumbnailSize;
        set => _gameButtonFactory?.ImageHeight = value;
    }

    private async Task RenderGridViewAsync(IList<string> files, string systemName, SystemManager.SystemManager systemManager, CancellationToken ct)
    {
        var buttonBatch = new List<Button>(Math.Min(BatchSize, files.Count));

        foreach (var filePath in files)
        {
            ct.ThrowIfCancellationRequested();

            var gameButton = await _gameButtonFactory.CreateGameButtonAsync(filePath, systemName, systemManager);
            buttonBatch.Add(gameButton);

            if (buttonBatch.Count >= BatchSize)
            {
                _host.GameFileGrid.Dispatcher.Invoke(() =>
                {
                    foreach (var btn in buttonBatch)
                        _host.GameFileGrid.Children.Add(btn);
                });
                buttonBatch.Clear();
            }
        }

        if (buttonBatch.Count > 0)
        {
            _host.GameFileGrid.Dispatcher.Invoke(() =>
            {
                foreach (var btn in buttonBatch)
                    _host.GameFileGrid.Children.Add(btn);
            });
        }
    }

    private async Task RenderListViewAsync(IList<string> files, string systemName, SystemManager.SystemManager systemManager, CancellationToken ct)
    {
        var itemBatch = new List<GameListViewItem>(Math.Min(BatchSize, files.Count));

        foreach (var filePath in files)
        {
            ct.ThrowIfCancellationRequested();

            var gameListViewItem = await _gameListFactory.CreateGameListViewItemAsync(filePath, systemName, systemManager);
            itemBatch.Add(gameListViewItem);

            if (itemBatch.Count >= BatchSize)
            {
                await _host.Dispatcher.InvokeAsync(() =>
                {
                    foreach (var item in itemBatch)
                        _host.GameListItems.Add(item);
                });
                itemBatch.Clear();
            }
        }

        if (itemBatch.Count > 0)
        {
            await _host.Dispatcher.InvokeAsync(() =>
            {
                foreach (var item in itemBatch)
                    _host.GameListItems.Add(item);
            });
        }
    }
}