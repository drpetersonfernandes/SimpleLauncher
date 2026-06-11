using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;
using Microsoft.Extensions.Configuration;
using SimpleLauncher.Interfaces;
using SimpleLauncher.Services.DebugAndBugReport;
using SimpleLauncher.Tests.TestHelpers;
using SimpleLauncher.ViewModels;
using Xunit;

namespace SimpleLauncher.Tests;

[SuppressMessage("ReSharper", "NullableWarningSuppressionIsUsed")]
public class GlobalStatsViewModelTests : IDisposable
{
    private readonly IConfiguration _configuration;
    private readonly ILogErrors _logErrors = new NoOpLogErrors();
    private readonly IGetListOfFilesService _getListOfFiles = new NoOpGetListOfFiles();

    public GlobalStatsViewModelTests()
    {
        ServiceProviderMock.Install();
        _configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["ImageExtensions:0"] = ".png",
                ["ImageExtensions:1"] = ".jpg",
                ["ImageExtensions:2"] = ".jpeg"
            })
            .Build();
    }

    public void Dispose()
    {
        ServiceProviderMock.Restore();
        GC.SuppressFinalize(this);
    }

    [Fact]
    public void ConstructorWithEmptySystemManagersInitializesEmptyCollections()
    {
        var systemManagers = new List<Services.SystemManager.SystemManager>();

        var viewModel = new GlobalStatsViewModel(_configuration, _logErrors, _getListOfFiles, new NoOpMessageBoxLibraryService(), new NoOpResourceProvider());
        viewModel.Initialize(systemManagers);

        Assert.Empty(viewModel.SystemStats);
        Assert.True(viewModel.IsStartButtonVisible);
        Assert.False(viewModel.IsSaveButtonVisible);
        Assert.False(viewModel.IsBusyOverlayVisible);
        Assert.False(viewModel.IsCancelOverlayVisible);
        Assert.False(viewModel.IsProcessing);
    }

    [Fact]
    public void ConstructorInitializesInfoText()
    {
        var systemManagers = new List<Services.SystemManager.SystemManager>();

        var viewModel = new GlobalStatsViewModel(_configuration, _logErrors, _getListOfFiles, new NoOpMessageBoxLibraryService(), new NoOpResourceProvider());
        viewModel.Initialize(systemManagers);

        Assert.NotNull(viewModel.InfoText);
        // InfoText may be empty when Application.Current is null (in unit tests)
        // In production it will be initialized from resources
    }

    [Fact]
    public void ConstructorInitializesBusyOverlayText()
    {
        var systemManagers = new List<Services.SystemManager.SystemManager>();

        var viewModel = new GlobalStatsViewModel(_configuration, _logErrors, _getListOfFiles, new NoOpMessageBoxLibraryService(), new NoOpResourceProvider());
        viewModel.Initialize(systemManagers);

        Assert.NotNull(viewModel.BusyOverlayText);
        Assert.NotEmpty(viewModel.BusyOverlayText);
    }

    [Fact]
    public void ImplementsINotifyPropertyChanged()
    {
        var systemManagers = new List<Services.SystemManager.SystemManager>();
        var viewModel = new GlobalStatsViewModel(_configuration, _logErrors, _getListOfFiles, new NoOpMessageBoxLibraryService(), new NoOpResourceProvider());
        viewModel.Initialize(systemManagers);

        Assert.IsAssignableFrom<INotifyPropertyChanged>(viewModel);
    }

    [Fact]
    public void StartCommandExists()
    {
        var systemManagers = new List<Services.SystemManager.SystemManager>();
        var viewModel = new GlobalStatsViewModel(_configuration, _logErrors, _getListOfFiles, new NoOpMessageBoxLibraryService(), new NoOpResourceProvider());
        viewModel.Initialize(systemManagers);

        Assert.NotNull(viewModel.StartCommand);
    }

    [Fact]
    public void CancelCommandExists()
    {
        var systemManagers = new List<Services.SystemManager.SystemManager>();
        var viewModel = new GlobalStatsViewModel(_configuration, _logErrors, _getListOfFiles, new NoOpMessageBoxLibraryService(), new NoOpResourceProvider());
        viewModel.Initialize(systemManagers);

        Assert.NotNull(viewModel.CancelCommand);
    }

    [Fact]
    public void SaveReportCommandExists()
    {
        var systemManagers = new List<Services.SystemManager.SystemManager>();
        var viewModel = new GlobalStatsViewModel(_configuration, _logErrors, _getListOfFiles, new NoOpMessageBoxLibraryService(), new NoOpResourceProvider());
        viewModel.Initialize(systemManagers);

        Assert.NotNull(viewModel.SaveReportCommand);
    }

    [Fact]
    public void ClosingCommandExists()
    {
        var systemManagers = new List<Services.SystemManager.SystemManager>();
        var viewModel = new GlobalStatsViewModel(_configuration, _logErrors, _getListOfFiles, new NoOpMessageBoxLibraryService(), new NoOpResourceProvider());
        viewModel.Initialize(systemManagers);

        Assert.NotNull(viewModel.ClosingCommand);
    }

    [Fact]
    public void CancelCommandCannotExecuteInitially()
    {
        var systemManagers = new List<Services.SystemManager.SystemManager>();
        var viewModel = new GlobalStatsViewModel(_configuration, _logErrors, _getListOfFiles, new NoOpMessageBoxLibraryService(), new NoOpResourceProvider());
        viewModel.Initialize(systemManagers);

        // Initially cannot execute because IsProcessing is false
        Assert.False(viewModel.CancelCommand.CanExecute(null));
    }

    [Fact]
    public void SaveReportCommandCannotExecuteInitially()
    {
        var systemManagers = new List<Services.SystemManager.SystemManager>();
        var viewModel = new GlobalStatsViewModel(_configuration, _logErrors, _getListOfFiles, new NoOpMessageBoxLibraryService(), new NoOpResourceProvider());
        viewModel.Initialize(systemManagers);

        Assert.False(viewModel.SaveReportCommand.CanExecute(null));
    }

    [Fact]
    public void StartCommandCanExecuteWhenNotProcessing()
    {
        var systemManagers = new List<Services.SystemManager.SystemManager>();
        var viewModel = new GlobalStatsViewModel(_configuration, _logErrors, _getListOfFiles, new NoOpMessageBoxLibraryService(), new NoOpResourceProvider());
        viewModel.Initialize(systemManagers);

        // StartCommand CanExecute depends on !IsProcessing
        Assert.True(viewModel.StartCommand.CanExecute(null));
    }

    [Fact]
    public void ConstructorThrowsOnNullSystemManagers()
    {
        var viewModel = new GlobalStatsViewModel(_configuration, _logErrors, _getListOfFiles, new NoOpMessageBoxLibraryService(), new NoOpResourceProvider());
        Assert.Throws<ArgumentNullException>(() => viewModel.Initialize(null!));
    }

    [Fact]
    public void ConstructorThrowsOnNullConfiguration()
    {
        Assert.Throws<ArgumentNullException>(() => new GlobalStatsViewModel(null!, _logErrors, _getListOfFiles, new NoOpMessageBoxLibraryService(), new NoOpResourceProvider()));
    }

    [Fact]
    public void DisposeDoesNotThrow()
    {
        var systemManagers = new List<Services.SystemManager.SystemManager>();
        var viewModel = new GlobalStatsViewModel(_configuration, _logErrors, _getListOfFiles, new NoOpMessageBoxLibraryService(), new NoOpResourceProvider());
        viewModel.Initialize(systemManagers);

        var exception = Record.Exception(viewModel.Dispose);

        Assert.Null(exception);
    }

    [Fact]
    public void CloseRequestedEventCanBeSubscribed()
    {
        var systemManagers = new List<Services.SystemManager.SystemManager>();
        var viewModel = new GlobalStatsViewModel(_configuration, _logErrors, _getListOfFiles, new NoOpMessageBoxLibraryService(), new NoOpResourceProvider());
        viewModel.Initialize(systemManagers);

        // Verify event can be subscribed to
        var eventRaised = false;
        viewModel.CloseRequested += () => { eventRaised = true; };

        Assert.False(eventRaised); // Event was just subscribed, not raised
    }

    [Fact]
    public void ConfirmSaveReportRequestedEventCanBeSubscribed()
    {
        var systemManagers = new List<Services.SystemManager.SystemManager>();
        var viewModel = new GlobalStatsViewModel(_configuration, _logErrors, _getListOfFiles, new NoOpMessageBoxLibraryService(), new NoOpResourceProvider());
        viewModel.Initialize(systemManagers);

        viewModel.ConfirmSaveReportRequested += Handler;
        viewModel.ConfirmSaveReportRequested -= Handler;

        Assert.NotNull(viewModel);
        return;

        // Verify event can be subscribed to
        static System.Windows.MessageBoxResult Handler()
        {
            return System.Windows.MessageBoxResult.Yes;
        }
    }

    [Fact]
    public void ConfirmCancelRequestedEventCanBeSubscribed()
    {
        var systemManagers = new List<Services.SystemManager.SystemManager>();
        var viewModel = new GlobalStatsViewModel(_configuration, _logErrors, _getListOfFiles, new NoOpMessageBoxLibraryService(), new NoOpResourceProvider());
        viewModel.Initialize(systemManagers);

        viewModel.ConfirmCancelRequested += Handler;
        viewModel.ConfirmCancelRequested -= Handler;

        Assert.NotNull(viewModel);
        return;

        // Verify event can be subscribed to
        static System.Windows.MessageBoxResult Handler()
        {
            return System.Windows.MessageBoxResult.Yes;
        }
    }

    [Fact]
    public void PropertyChangedEventCanBeSubscribed()
    {
        var systemManagers = new List<Services.SystemManager.SystemManager>();
        var viewModel = new GlobalStatsViewModel(_configuration, _logErrors, _getListOfFiles, new NoOpMessageBoxLibraryService(), new NoOpResourceProvider());
        viewModel.Initialize(systemManagers);

        // Verify PropertyChanged event can be subscribed to
        viewModel.PropertyChanged += static (_, _) => { };

        // Properties exist and PropertyChanged can be raised
        Assert.NotNull(viewModel.InfoText);
        Assert.NotNull(viewModel.BusyOverlayText);
        Assert.NotNull(viewModel.SystemStats);
    }

    private sealed class NoOpLogErrors : ILogErrors
    {
        public Task LogErrorAsync(Exception? ex, string? contextMessage = null)
        {
            return Task.CompletedTask;
        }
    }
}
