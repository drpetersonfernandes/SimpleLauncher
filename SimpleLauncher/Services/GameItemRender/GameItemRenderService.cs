using System.Windows.Controls;
using Microsoft.Extensions.Configuration;
using SimpleLauncher.Core.Interfaces;
using SimpleLauncher.Core.Services.DebugAndBugReport;
using SimpleLauncher.Models;
using SimpleLauncher.Services.Favorites;
using SimpleLauncher.Services.FindCoverImage;
using SimpleLauncher.Services.GameItemFactory;
using SimpleLauncher.Services.GameListUI;
using SimpleLauncher.Services.GamePad;
using SimpleLauncher.Services.GetListOfFiles;
using SimpleLauncher.Services.PlayHistory;
using SimpleLauncher.Services.PlaySound;

namespace SimpleLauncher.Services.GameItemRender;

public class GameItemRenderService : IGameItemRenderService
{
    private const int BatchSize = 100;

    private readonly Core.Services.SettingsManager.SettingsManager _settings;
    private readonly FavoritesManager _favoritesManager;
    private readonly PlayHistoryManager _playHistoryManager;
    private readonly GamePadController _gamePadController;
    private readonly GameLauncher.GameLauncher _gameLauncher;
    private readonly PlaySoundEffects _playSoundEffects;
    private readonly IConfiguration _configuration;
    private readonly ILogErrors _logErrors;
    private readonly IGetListOfFiles _getListOfFiles;
    private readonly IFindCoverImage _findCoverImage;
    private readonly IImageLoader _imageLoader;
    private readonly GameListUiService _gameListUiService;
    private readonly IMessageBoxLibraryService _messageBox;

    private IGameItemRenderHost _host;
    private GameButtonFactory _gameButtonFactory;
    private GameListFactory _gameListFactory;

    public GameItemRenderService(
        Core.Services.SettingsManager.SettingsManager settings,
        FavoritesManager favoritesManager,
        PlayHistoryManager playHistoryManager,
        GamePadController gamePadController,
        GameLauncher.GameLauncher gameLauncher,
        PlaySoundEffects playSoundEffects,
        IConfiguration configuration,
        ILogErrors logErrors,
        IGetListOfFiles getListOfFiles,
        IFindCoverImage findCoverImage,
        IImageLoader imageLoader,
        GameListUiService gameListUiService,
        IMessageBoxLibraryService messageBox)
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
    }

    public void Initialize(IGameItemRenderHost host)
    {
        _host = host;
    }

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
            _imageLoader);

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

    public Task HandleDoubleClickAsync(GameListViewItem selectedItem)
    {
        return _gameListFactory.HandleDoubleClickAsync(selectedItem);
    }

    public void HandleSelectionChangedAsync(GameListViewItem selectedItem)
    {
        _gameListFactory.HandleSelectionChangedAsync(selectedItem);
    }

    public void ClearRenderedItems()
    {
        _host.GameFileGrid.Dispatcher.Invoke(() =>
        {
            GameListUiService.ClearGameButtonImages(_host.GameFileGrid);
            _host.GameFileGrid.Children.Clear();
        });
        _host.Dispatcher.Invoke(() => _host.GameListItems.Clear());
    }

    public void SetGameButtonsEnabled(bool isEnabled)
    {
        _gameListUiService.SetGameButtonsEnabled(isEnabled);
    }

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
