using System.ComponentModel;
using System.Windows;
using Microsoft.Extensions.Configuration;
using SimpleLauncher.Services.DebugAndBugReport;
using SimpleLauncher.Tests.TestHelpers;
using SimpleLauncher.ViewModels;
using Xunit;

namespace SimpleLauncher.Tests;

public class GlobalStatsViewModelTests : IDisposable
{
    private readonly IConfiguration _configuration;
    private readonly ILogErrors _logErrors = new NoOpLogErrors();

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

        var viewModel = new GlobalStatsViewModel(systemManagers, _configuration, _logErrors);

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

        var viewModel = new GlobalStatsViewModel(systemManagers, _configuration, _logErrors);

        Assert.NotNull(viewModel.InfoText);
        // InfoText may be empty when Application.Current is null (in unit tests)
        // In production it will be initialized from resources
    }

    [Fact]
    public void ConstructorInitializesBusyOverlayText()
    {
        var systemManagers = new List<Services.SystemManager.SystemManager>();

        var viewModel = new GlobalStatsViewModel(systemManagers, _configuration, _logErrors);

        Assert.NotNull(viewModel.BusyOverlayText);
        Assert.NotEmpty(viewModel.BusyOverlayText);
    }

    [Fact]
    public void ImplementsINotifyPropertyChanged()
    {
        var systemManagers = new List<Services.SystemManager.SystemManager>();
        var viewModel = new GlobalStatsViewModel(systemManagers, _configuration, _logErrors);

        Assert.IsAssignableFrom<INotifyPropertyChanged>(viewModel);
    }

    [Fact]
    public void StartCommandExists()
    {
        var systemManagers = new List<Services.SystemManager.SystemManager>();
        var viewModel = new GlobalStatsViewModel(systemManagers, _configuration, _logErrors);

        Assert.NotNull(viewModel.StartCommand);
    }

    [Fact]
    public void CancelCommandExists()
    {
        var systemManagers = new List<Services.SystemManager.SystemManager>();
        var viewModel = new GlobalStatsViewModel(systemManagers, _configuration, _logErrors);

        Assert.NotNull(viewModel.CancelCommand);
    }

    [Fact]
    public void SaveReportCommandExists()
    {
        var systemManagers = new List<Services.SystemManager.SystemManager>();
        var viewModel = new GlobalStatsViewModel(systemManagers, _configuration, _logErrors);

        Assert.NotNull(viewModel.SaveReportCommand);
    }

    [Fact]
    public void ClosingCommandExists()
    {
        var systemManagers = new List<Services.SystemManager.SystemManager>();
        var viewModel = new GlobalStatsViewModel(systemManagers, _configuration, _logErrors);

        Assert.NotNull(viewModel.ClosingCommand);
    }

    [Fact]
    public void CancelCommandCannotExecuteInitially()
    {
        var systemManagers = new List<Services.SystemManager.SystemManager>();
        var viewModel = new GlobalStatsViewModel(systemManagers, _configuration, _logErrors);

        // Initially cannot execute because IsProcessing is false
        Assert.False(viewModel.CancelCommand.CanExecute(null));
    }

    [Fact]
    public void SaveReportCommandCannotExecuteInitially()
    {
        var systemManagers = new List<Services.SystemManager.SystemManager>();
        var viewModel = new GlobalStatsViewModel(systemManagers, _configuration, _logErrors);

        Assert.False(viewModel.SaveReportCommand.CanExecute(null));
    }

    [Fact]
    public void StartCommandCanExecuteWhenNotProcessing()
    {
        var systemManagers = new List<Services.SystemManager.SystemManager>();
        var viewModel = new GlobalStatsViewModel(systemManagers, _configuration, _logErrors);

        // StartCommand CanExecute depends on !IsProcessing
        Assert.True(viewModel.StartCommand.CanExecute(null));
    }

    [Fact]
    public void ConstructorThrowsOnNullSystemManagers()
    {
        Assert.Throws<ArgumentNullException>(() => new GlobalStatsViewModel(null!, _configuration, _logErrors));
    }

    [Fact]
    public void ConstructorThrowsOnNullConfiguration()
    {
        var systemManagers = new List<Services.SystemManager.SystemManager>();
        Assert.Throws<ArgumentNullException>(() => new GlobalStatsViewModel(systemManagers, null!, _logErrors));
    }

    [Fact]
    public void DisposeDoesNotThrow()
    {
        var systemManagers = new List<Services.SystemManager.SystemManager>();
        var viewModel = new GlobalStatsViewModel(systemManagers, _configuration, _logErrors);

        var exception = Record.Exception(viewModel.Dispose);

        Assert.Null(exception);
    }

    [Fact]
    public void CloseRequestedEventCanBeSubscribed()
    {
        var systemManagers = new List<Services.SystemManager.SystemManager>();
        var viewModel = new GlobalStatsViewModel(systemManagers, _configuration, _logErrors);

        // Verify event can be subscribed to
        var eventRaised = false;
        viewModel.CloseRequested += () => { eventRaised = true; };

        Assert.False(eventRaised); // Event was just subscribed, not raised
    }

    [Fact]
    public void ConfirmSaveReportRequestedEventCanBeSubscribed()
    {
        var systemManagers = new List<Services.SystemManager.SystemManager>();
        var viewModel = new GlobalStatsViewModel(systemManagers, _configuration, _logErrors);

        viewModel.ConfirmSaveReportRequested += Handler;
        viewModel.ConfirmSaveReportRequested -= Handler;

        Assert.NotNull(viewModel);
        return;

        // Verify event can be subscribed to
        static MessageBoxResult Handler()
        {
            return MessageBoxResult.Yes;
        }
    }

    [Fact]
    public void ConfirmCancelRequestedEventCanBeSubscribed()
    {
        var systemManagers = new List<Services.SystemManager.SystemManager>();
        var viewModel = new GlobalStatsViewModel(systemManagers, _configuration, _logErrors);

        viewModel.ConfirmCancelRequested += Handler;
        viewModel.ConfirmCancelRequested -= Handler;

        Assert.NotNull(viewModel);
        return;

        // Verify event can be subscribed to
        static MessageBoxResult Handler()
        {
            return MessageBoxResult.Yes;
        }
    }

    [Fact]
    public void PropertyChangedEventCanBeSubscribed()
    {
        var systemManagers = new List<Services.SystemManager.SystemManager>();
        var viewModel = new GlobalStatsViewModel(systemManagers, _configuration, _logErrors);

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
