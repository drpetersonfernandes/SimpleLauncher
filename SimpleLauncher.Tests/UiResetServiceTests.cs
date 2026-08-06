using System.Diagnostics.CodeAnalysis;
using Moq;
using SimpleLauncher.Core.Interfaces;
using SimpleLauncher.Services.UIReset;
using SimpleLauncher.Tests.TestHelpers;
using Xunit;

namespace SimpleLauncher.Tests;

/// <summary>
/// Tests for <see cref="UiResetService"/> orchestration using a mocked <see cref="IUiResetHost"/>.
/// </summary>
[SuppressMessage("ReSharper", "PlaceAssignmentExpressionIntoBlock")]
public class UiResetServiceTests
{
    private static (UiResetService Service, Mock<IUiResetHost> Host) CreateService()
    {
        var hostMock = new Mock<IUiResetHost>();
        hostMock.SetupAllProperties();
        hostMock.Setup(x => x.DisplaySystemSelectionScreenAsync(It.IsAny<CancellationToken>())).Returns(Task.CompletedTask);
        hostMock.SetupGet(x => x.CurrentCancellationToken).Returns(CancellationToken.None);

        var service = new UiResetService(new NoOpLogger());
        service.Initialize(hostMock.Object);
        return (service, hostMock);
    }

    [Fact]
    public void ResetUiAsync_PerformsFullResetSequence()
    {
        StaApartment.RunAsync(async () =>
        {
            StaApartment.EnsureApplication();
            var (service, host) = CreateService();

            await service.ResetUiAsync();

            host.Verify(x => x.CancelAndRecreateToken(), Times.Once);
            host.Verify(x => x.ResetPaginationButtons(), Times.Once);
            host.Verify(x => x.SetSearchTextBoxText(""), Times.Once);
            host.VerifySet(x => x.CurrentFilter = null, Times.Once);
            host.VerifySet(x => x.ActiveSearchQueryOrMode = null, Times.Once);
            host.Verify(x => x.ClearPreviewImage(), Times.Once);
            host.Verify(x => x.SetSystemComboBoxSelectedItem(null), Times.Once);
            host.Verify(x => x.SetEmulatorComboBoxSelectedItem(null), Times.Once);
            host.Verify(x => x.DisplaySystemSelectionScreenAsync(It.IsAny<CancellationToken>()), Times.Once);
            Assert.Equal("No system selected", host.Object.SelectedSystem);
            Assert.Equal("00:00:00", host.Object.PlayTime);
            Assert.False(host.Object.IsUiUpdating);
        });
    }

    [Fact]
    public void ResetUiAsync_WhileLoadingGames_HidesOverlay()
    {
        StaApartment.RunAsync(async () =>
        {
            StaApartment.EnsureApplication();
            var (service, host) = CreateService();
            host.SetupGet(x => x.IsLoadingGames).Returns(true);

            await service.ResetUiAsync();

            host.Verify(x => x.SetLoadingOverlayVisible(false), Times.Once);
            host.VerifySet(x => x.IsLoadingGames = false, Times.Once);
        });
    }

    [Fact]
    public void ResetUiAsync_WhenUiAlreadyUpdating_ReturnsImmediately()
    {
        StaApartment.Run(() =>
        {
            var (service, host) = CreateService();
            host.SetupGet(x => x.IsUiUpdating).Returns(true);

            service.ResetUiAsync().GetAwaiter().GetResult();

            host.Verify(x => x.CancelAndRecreateToken(), Times.Once);
            host.Verify(x => x.ResetPaginationButtons(), Times.Never);
            host.Verify(x => x.DisplaySystemSelectionScreenAsync(It.IsAny<CancellationToken>()), Times.Never);
        });
    }

    [Fact]
    public void ResetUiAsync_DisplayScreenThrowsOperationCanceled_IsSwallowed()
    {
        StaApartment.RunAsync(async () =>
        {
            StaApartment.EnsureApplication();
            var (service, host) = CreateService();
            host.Setup(x => x.DisplaySystemSelectionScreenAsync(It.IsAny<CancellationToken>()))
                .ThrowsAsync(new OperationCanceledException());

            await service.ResetUiAsync();

            Assert.False(host.Object.IsUiUpdating);
        });
    }

    [Fact]
    public void ResetUiAsync_DisplayScreenThrowsGenericException_IsLoggedAndUiUpdatingReset()
    {
        StaApartment.RunAsync(async () =>
        {
            StaApartment.EnsureApplication();
            var (service, host) = CreateService();
            host.Setup(x => x.DisplaySystemSelectionScreenAsync(It.IsAny<CancellationToken>()))
                .ThrowsAsync(new InvalidOperationException("screen failed"));

            await service.ResetUiAsync();

            Assert.False(host.Object.IsUiUpdating);
        });
    }

    [Fact]
    public async Task ResetUiAsync_WithoutHost_DoesNotThrow()
    {
        var service = new UiResetService(new NoOpLogger());
        await service.ResetUiAsync();
    }
}
