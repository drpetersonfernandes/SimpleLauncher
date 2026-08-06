using System.Windows.Controls;
using System.Windows.Threading;
using Moq;
using SimpleLauncher.Interfaces;
using SimpleLauncher.Services.UpdateStatusBar;
using SimpleLauncher.Tests.TestHelpers;
using Xunit;

namespace SimpleLauncher.Tests;

/// <summary>
/// Tests for <see cref="UpdateStatusBarService"/> using a mocked <see cref="IStatusBarHost"/>
/// with real WPF controls on an STA thread.
/// </summary>
public class UpdateStatusBarServiceTests
{
    private static (UpdateStatusBarService Service, Mock<IStatusBarHost> Host) CreateService(Label label, DispatcherTimer? timer)
    {
        var hostMock = new Mock<IStatusBarHost>();
        hostMock.SetupGet(x => x.Dispatcher).Returns(() => Dispatcher.CurrentDispatcher);
        hostMock.SetupGet(x => x.StatusBarText).Returns(label);
        hostMock.SetupGet(x => x.StatusBarTimer).Returns(timer);

        var service = new UpdateStatusBarService();
        service.Initialize(hostMock.Object);
        return (service, hostMock);
    }

    [Fact]
    public void UpdateContent_SetsStatusBarText()
    {
        StaApartment.Run(() =>
        {
            var label = new Label();
            var (service, host) = CreateService(label, timer: null);

            service.UpdateContent("Loading 100 games...");

            Assert.Equal("Loading 100 games...", label.Content);
            host.Verify(x => x.Dispatcher, Times.AtLeastOnce);
        });
    }

    [Fact]
    public void UpdateContent_RestartsAutoClearTimer()
    {
        StaApartment.Run(() =>
        {
            var label = new Label();
            var timer = new DispatcherTimer { Interval = TimeSpan.FromMinutes(5) };
            try
            {
                var (service, _) = CreateService(label, timer);

                service.UpdateContent("Saving settings...");

                Assert.True(timer.IsEnabled, "The auto-clear timer should be restarted (enabled) after an update.");
            }
            finally
            {
                timer.Stop();
            }
        });
    }

    [Fact]
    public void UpdateContent_WithoutTimer_DoesNotThrow()
    {
        StaApartment.Run(() =>
        {
            var label = new Label();
            var (service, _) = CreateService(label, timer: null);

            service.UpdateContent("No timer configured");

            Assert.Equal("No timer configured", label.Content);
        });
    }

    [Fact]
    public void UpdateContent_WithoutHost_DoesNotThrow()
    {
        var service = new UpdateStatusBarService();
        service.UpdateContent("no host");
    }
}
