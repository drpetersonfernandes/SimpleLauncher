using System.Windows.Threading;
using Moq;
using SimpleLauncher.Interfaces;
using SimpleLauncher.Services.LoadingOverlay;
using SimpleLauncher.Tests.TestHelpers;
using Xunit;

namespace SimpleLauncher.Tests;

/// <summary>
///     Tests for <see cref="LoadingOverlayService" /> reference-counted overlay state.
///     WPF objects are created on a dedicated STA thread with an Application so resource lookups work.
/// </summary>
public class LoadingOverlayServiceTests
{
    private static (LoadingOverlayService Service, Mock<ILoadingOverlayHost> Host, Mock<IUpdateStatusBar> StatusBar)
        CreateService()
    {
        var statusBarMock = new Mock<IUpdateStatusBar>();
        var hostMock = new Mock<ILoadingOverlayHost>();
        hostMock.SetupGet(x => x.Dispatcher).Returns(() => Dispatcher.CurrentDispatcher);
        hostMock.SetupGet(x => x.UpdateStatusBarService).Returns(statusBarMock.Object);
        hostMock.Setup(x => x.ResetUiAsync()).Returns(Task.CompletedTask);

        // PlaySoundEffects is intentionally null: EmergencyRelease guards it with ?.
        var service = new LoadingOverlayService(null!, new NoOpLogger());
        service.Initialize(hostMock.Object);
        return (service, hostMock, statusBarMock);
    }

    [Fact]
    public void SetLoadingState_WithoutHost_IsNoOp()
    {
        StaApartment.Run(() =>
        {
            var service = new LoadingOverlayService(null!, new NoOpLogger());

            service.SetLoadingState(true, "Loading...");
            service.EmergencyRelease();
            // No exception expected
        });
    }

    [Fact]
    public void SetLoadingState_True_ShowsOverlayAndSetsMessage()
    {
        StaApartment.Run(() =>
        {
            StaApartment.EnsureApplication();
            var (service, host, _) = CreateService();

            service.SetLoadingState(true, "Loading games...");

            host.Verify(x => x.SetIsLoadingGamesInternal(true), Times.Once);
            host.Verify(x => x.SetLoadingOverlayVisible(true), Times.Once);
            host.Verify(x => x.SetMainContentGridEnabled(false), Times.Once);
            host.Verify(x => x.SetLoadingOverlayContent("Loading games..."), Times.Once);
        });
    }

    [Fact]
    public void SetLoadingState_TrueWithoutMessage_DoesNotSetContent()
    {
        StaApartment.Run(() =>
        {
            StaApartment.EnsureApplication();
            var (service, host, _) = CreateService();

            service.SetLoadingState(true);

            host.Verify(x => x.SetLoadingOverlayContent(It.IsAny<object>()), Times.Never);
        });
    }

    [Fact]
    public void SetLoadingState_TrueTwiceFalseOnce_OverlayStaysVisible()
    {
        StaApartment.Run(() =>
        {
            StaApartment.EnsureApplication();
            var (service, host, _) = CreateService();

            service.SetLoadingState(true, "first");
            service.SetLoadingState(true, "second");
            service.SetLoadingState(false);

            host.Verify(x => x.SetLoadingOverlayVisible(true),
                Times.Exactly(3)); // shown twice + re-asserted while count > 0
            host.Verify(x => x.SetLoadingOverlayVisible(false), Times.Never);
            host.Verify(x => x.SetMainContentGridEnabled(false),
                Times.Exactly(3)); // one call per SetLoadingState invocation
        });
    }

    [Fact]
    public void SetLoadingState_FalseWhenCountReachesZero_HidesOverlayAndResetsContent()
    {
        StaApartment.Run(() =>
        {
            StaApartment.EnsureApplication();
            var (service, host, _) = CreateService();

            service.SetLoadingState(true, "Working...");
            service.SetLoadingState(false);

            host.Verify(x => x.SetLoadingOverlayVisible(false), Times.Once);
            host.Verify(x => x.SetMainContentGridEnabled(true), Times.Once);
            host.Verify(x => x.SetLoadingOverlayContent("Working..."), Times.Once);
            host.Verify(x => x.SetLoadingOverlayContent("Loading..."), Times.Once); // reset to default text on hide
        });
    }

    [Fact]
    public void SetLoadingState_FalseAtZeroCount_DoesNotThrow()
    {
        StaApartment.Run(() =>
        {
            StaApartment.EnsureApplication();
            var (service, host, _) = CreateService();

            service.SetLoadingState(false);

            host.Verify(x => x.SetLoadingOverlayVisible(false), Times.Once);
        });
    }

    [Fact]
    public void EmergencyRelease_ResetsStateAndRestoresUi()
    {
        StaApartment.Run(() =>
        {
            StaApartment.EnsureApplication();
            var (service, host, statusBar) = CreateService();

            service.SetLoadingState(true, "stuck...");
            service.EmergencyRelease();

            host.Verify(x => x.SetIsLoadingGamesInternal(false), Times.AtLeastOnce);
            host.Verify(x => x.CancelAndRecreateToken(), Times.Once);
            host.Verify(x => x.SetLoadingOverlayVisible(false), Times.AtLeastOnce);
            host.Verify(x => x.SetMainContentGridEnabled(true), Times.AtLeastOnce);
            host.Verify(x => x.ResetUiAsync(), Times.Once);
            statusBar.Verify(x => x.UpdateContent("Emergency reset performed."), Times.Once);
        });
    }
}