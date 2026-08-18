using Moq;
using SimpleLauncher.Avalonia.Services;
using SimpleLauncher.Avalonia.ViewModels;
using SimpleLauncher.Core.Interfaces;
using CoreMessageBoxResult = SimpleLauncher.Core.Models.MessageBoxResult;

namespace SimpleLauncher.Avalonia.Tests;

/// <summary>
/// Tests for the AboutWindow ViewModel (Phase 4.1 port).
/// </summary>
public class AboutViewModelTests
{
    private static (AboutViewModel Vm, Mock<IMessageBoxLibraryService> MessageBox, Mock<ILogger> Logger)
        CreateVm(Func<HttpRequestMessage, HttpResponseMessage>? responder = null)
    {
        responder ??= _ => new HttpResponseMessage(System.Net.HttpStatusCode.NotFound);
        var client = TestDependencies.HttpClientWith(responder);
        var factory = TestDependencies.HttpFactory(client);
        var messageBox = TestDependencies.MessageBox();
        var logger = TestDependencies.Logger();
        var updateChecker = new AvaloniaCheckForUpdatesService(factory.Object, messageBox.Object, logger.Object);
        var vm = new AboutViewModel(logger.Object, messageBox.Object, updateChecker);
        return (vm, messageBox, logger);
    }

    [Fact]
    public void Ctor_SetsVersionAndLogoPath()
    {
        var (vm, _, _) = CreateVm();
        Assert.StartsWith("Version: ", vm.AppVersion);
        Assert.False(string.IsNullOrEmpty(vm.LogoPath));
        Assert.EndsWith(Path.Combine("images", "logo2.png"), vm.LogoPath);
    }

    [Fact]
    public void CloseCommand_RaisesCloseRequested()
    {
        var (vm, _, _) = CreateVm();
        var raised = false;
        vm.CloseRequested += (_, _) => { raised = true; };

        vm.CloseCommand.Execute(null);

        Assert.True(raised);
    }

    [Fact]
    public void OpenUpdateHistoryCommand_RaisesOpenUpdateHistoryRequested()
    {
        var (vm, _, _) = CreateVm();
        var raised = false;
        vm.OpenUpdateHistoryRequested += (_, _) => { raised = true; };

        vm.OpenUpdateHistoryCommand.Execute(null);

        Assert.True(raised);
    }

    [Fact]
    public async Task CheckForUpdates_NewerVersionAvailable_AsksUser()
    {
        var (vm, messageBox, _) = CreateVm(_ =>
            new HttpResponseMessage(System.Net.HttpStatusCode.OK)
            {
                Content = new StringContent(
                    """{"tag_name": "v9.9.9", "assets": [{"name": "release_9.9.9_win-x64.zip", "browser_download_url": "https://example.com/x.zip"}]}""",
                    System.Text.Encoding.UTF8, "application/json")
            });
        messageBox.Setup(m => m.DoYouWantToUpdateMessageBoxAsync(It.IsAny<string>(), "9.9.9.0"))
            .ReturnsAsync(CoreMessageBoxResult.Yes);

        await vm.CheckForUpdatesCommand.ExecuteAsync(null);

        messageBox.Verify(m => m.DoYouWantToUpdateMessageBoxAsync(It.IsAny<string>(), "9.9.9.0"), Times.Once);
        messageBox.Verify(m => m.InstallUpdateManuallyMessageBoxAsync(), Times.Once);
        Assert.False(vm.IsCheckingForUpdates);
    }

    [Fact]
    public async Task CheckForUpdates_CommandDisabledWhileChecking()
    {
        // The responder delays, leaving a real async gap so IsCheckingForUpdates
        // (and therefore the command's CanExecute) is observable mid-flight.
        var handler = new DelayedHandler(TimeSpan.FromMilliseconds(300), new HttpResponseMessage(System.Net.HttpStatusCode.OK)
        {
            Content = new StringContent(
                """{"tag_name": "v9.9.9", "assets": [{"name": "release_9.9.9_win-x64.zip", "browser_download_url": "https://example.com/x.zip"}]}""",
                System.Text.Encoding.UTF8, "application/json")
        });
        var client = new HttpClient(handler);
        var factory = TestDependencies.HttpFactory(client);
        var messageBox = TestDependencies.MessageBox();
        var logger = TestDependencies.Logger();
        var updateChecker = new AvaloniaCheckForUpdatesService(factory.Object, messageBox.Object, logger.Object);
        var vm = new AboutViewModel(logger.Object, messageBox.Object, updateChecker);
        messageBox.Setup(m => m.DoYouWantToUpdateMessageBoxAsync(It.IsAny<string>(), It.IsAny<string>()))
            .ReturnsAsync(CoreMessageBoxResult.No);

        Assert.True(vm.CheckForUpdatesCommand.CanExecute(null));
        var task = vm.CheckForUpdatesCommand.ExecuteAsync(null);
        Assert.False(vm.CheckForUpdatesCommand.CanExecute(null)); // disabled mid-check
        await task;
        Assert.True(vm.CheckForUpdatesCommand.CanExecute(null)); // re-enabled after
    }

    private sealed class DelayedHandler : HttpMessageHandler
    {
        private readonly TimeSpan _delay;
        private readonly HttpResponseMessage _response;

        public DelayedHandler(TimeSpan delay, HttpResponseMessage response)
        {
            _delay = delay;
            _response = response;
        }

        protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            await Task.Delay(_delay, cancellationToken);
            return _response;
        }
    }
}