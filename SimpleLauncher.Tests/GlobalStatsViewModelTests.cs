using System.ComponentModel;
using System.Windows;
using Microsoft.Extensions.Configuration;
using SimpleLauncher.Tests.TestHelpers;
using SimpleLauncher.ViewModels;
using Xunit;

namespace SimpleLauncher.Tests;

public class GlobalStatsViewModelTests : IDisposable
{
    private readonly IConfiguration _configuration;

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
    public void Constructor_WithEmptySystemManagers_InitializesEmptyCollections()
    {
        var systemManagers = new List<Services.SystemManager.SystemManager>();

        var viewModel = new GlobalStatsViewModel(systemManagers, _configuration);

        Assert.Empty(viewModel.SystemStats);
        Assert.True(viewModel.IsStartButtonVisible);
        Assert.False(viewModel.IsSaveButtonVisible);
        Assert.False(viewModel.IsBusyOverlayVisible);
        Assert.False(viewModel.IsCancelOverlayVisible);
        Assert.False(viewModel.IsProcessing);
    }

    [Fact]
    public void Constructor_InitializesInfoText()
    {
        var systemManagers = new List<Services.SystemManager.SystemManager>();

        var viewModel = new GlobalStatsViewModel(systemManagers, _configuration);

        Assert.NotNull(viewModel.InfoText);
        // InfoText may be empty when Application.Current is null (in unit tests)
        // In production it will be initialized from resources
    }

    [Fact]
    public void Constructor_InitializesBusyOverlayText()
    {
        var systemManagers = new List<Services.SystemManager.SystemManager>();

        var viewModel = new GlobalStatsViewModel(systemManagers, _configuration);

        Assert.NotNull(viewModel.BusyOverlayText);
        Assert.NotEmpty(viewModel.BusyOverlayText);
    }

    [Fact]
    public void ImplementsINotifyPropertyChanged()
    {
        var systemManagers = new List<Services.SystemManager.SystemManager>();
        var viewModel = new GlobalStatsViewModel(systemManagers, _configuration);

        Assert.IsAssignableFrom<INotifyPropertyChanged>(viewModel);
    }

    [Fact]
    public void StartCommand_Exists()
    {
        var systemManagers = new List<Services.SystemManager.SystemManager>();
        var viewModel = new GlobalStatsViewModel(systemManagers, _configuration);

        Assert.NotNull(viewModel.StartCommand);
    }

    [Fact]
    public void CancelCommand_Exists()
    {
        var systemManagers = new List<Services.SystemManager.SystemManager>();
        var viewModel = new GlobalStatsViewModel(systemManagers, _configuration);

        Assert.NotNull(viewModel.CancelCommand);
    }

    [Fact]
    public void SaveReportCommand_Exists()
    {
        var systemManagers = new List<Services.SystemManager.SystemManager>();
        var viewModel = new GlobalStatsViewModel(systemManagers, _configuration);

        Assert.NotNull(viewModel.SaveReportCommand);
    }

    [Fact]
    public void ClosingCommand_Exists()
    {
        var systemManagers = new List<Services.SystemManager.SystemManager>();
        var viewModel = new GlobalStatsViewModel(systemManagers, _configuration);

        Assert.NotNull(viewModel.ClosingCommand);
    }

    [Fact]
    public void CancelCommand_CannotExecute_Initially()
    {
        var systemManagers = new List<Services.SystemManager.SystemManager>();
        var viewModel = new GlobalStatsViewModel(systemManagers, _configuration);

        // Initially cannot execute because IsProcessing is false
        Assert.False(viewModel.CancelCommand.CanExecute(null));
    }

    [Fact]
    public void SaveReportCommand_CannotExecute_Initially()
    {
        var systemManagers = new List<Services.SystemManager.SystemManager>();
        var viewModel = new GlobalStatsViewModel(systemManagers, _configuration);

        Assert.False(viewModel.SaveReportCommand.CanExecute(null));
    }

    [Fact]
    public void StartCommand_CanExecute_WhenNotProcessing()
    {
        var systemManagers = new List<Services.SystemManager.SystemManager>();
        var viewModel = new GlobalStatsViewModel(systemManagers, _configuration);

        // StartCommand CanExecute depends on !IsProcessing
        Assert.True(viewModel.StartCommand.CanExecute(null));
    }

    [Fact]
    public void Constructor_ThrowsOnNullSystemManagers()
    {
        Assert.Throws<ArgumentNullException>(() => new GlobalStatsViewModel(null!, _configuration));
    }

    [Fact]
    public void Constructor_ThrowsOnNullConfiguration()
    {
        var systemManagers = new List<Services.SystemManager.SystemManager>();
        Assert.Throws<ArgumentNullException>(() => new GlobalStatsViewModel(systemManagers, null!));
    }

    [Fact]
    public void Dispose_DoesNotThrow()
    {
        var systemManagers = new List<Services.SystemManager.SystemManager>();
        var viewModel = new GlobalStatsViewModel(systemManagers, _configuration);

        var exception = Record.Exception(viewModel.Dispose);

        Assert.Null(exception);
    }

    [Fact]
    public void CloseRequestedEvent_CanBeSubscribed()
    {
        var systemManagers = new List<Services.SystemManager.SystemManager>();
        var viewModel = new GlobalStatsViewModel(systemManagers, _configuration);

        // Verify event can be subscribed to
        var eventRaised = false;
        viewModel.CloseRequested += () => { eventRaised = true; };

        Assert.False(eventRaised); // Event was just subscribed, not raised
    }

    [Fact]
    public void ConfirmSaveReportRequestedEvent_CanBeSubscribed()
    {
        var systemManagers = new List<Services.SystemManager.SystemManager>();
        var viewModel = new GlobalStatsViewModel(systemManagers, _configuration);

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
    public void ConfirmCancelRequestedEvent_CanBeSubscribed()
    {
        var systemManagers = new List<Services.SystemManager.SystemManager>();
        var viewModel = new GlobalStatsViewModel(systemManagers, _configuration);

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
    public void PropertyChanged_EventCanBeSubscribed()
    {
        var systemManagers = new List<Services.SystemManager.SystemManager>();
        var viewModel = new GlobalStatsViewModel(systemManagers, _configuration);

        // Verify PropertyChanged event can be subscribed to
        viewModel.PropertyChanged += static (_, _) => { };

        // Properties exist and PropertyChanged can be raised
        Assert.NotNull(viewModel.InfoText);
        Assert.NotNull(viewModel.BusyOverlayText);
        Assert.NotNull(viewModel.SystemStats);
    }
}
