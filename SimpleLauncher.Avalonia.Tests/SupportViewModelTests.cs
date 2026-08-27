using System.Text;
using Moq;
using SimpleLauncher.Avalonia.ViewModels;
using SimpleLauncher.Core.Interfaces;

namespace SimpleLauncher.Avalonia.Tests;

/// <summary>
/// Tests for the SupportWindow ViewModel (Phase 4.1 port). HTTP traffic goes
/// through a fake handler; audio is disabled via settings.
/// </summary>
public class SupportViewModelTests
{
    private static SupportViewModel CreateVm(
        Mock<IMessageBoxLibraryService> messageBox,
        HttpClient? httpClient = null,
        Mock<ILogger>? logger = null)
    {
        var settings = TestDependencies.Settings(messageBox: messageBox);
        settings.EnableNotificationSound = false;
        var playSound = TestDependencies.PlaySound(settings);

        var config =
            TestEnvironment.ConfigurationFromJson(
                """{"EmailApiBaseUrl": "https://example.com/api", "SupportEmailTo": "support@example.com"}""");

        var factory = TestDependencies.HttpFactory(httpClient ?? TestDependencies.HttpClientWith(_ =>
            new HttpResponseMessage(System.Net.HttpStatusCode.OK)
                { Content = new StringContent("{}", Encoding.UTF8, "application/json") }));

        var resourceProvider = TestDependencies.ResourceProvider();
        return new SupportViewModel(playSound, factory.Object, config, messageBox.Object, resourceProvider.Object,
            (logger ?? TestDependencies.Logger()).Object);
    }

    [Fact]
    public async Task SendRequest_EmptyName_PromptsForName()
    {
        var messageBox = TestDependencies.MessageBox();
        var vm = CreateVm(messageBox);

        await vm.SendSupportRequestCommand.ExecuteAsync(null);

        messageBox.Verify(m => m.EnterNameMessageBoxAsync(), Times.Once);
        messageBox.Verify(m => m.EnterEmailMessageBoxAsync(), Times.Never);
        messageBox.Verify(m => m.EnterSupportRequestMessageBoxAsync(), Times.Never);
    }

    [Fact]
    public async Task SendRequest_EmptyEmail_PromptsForEmail()
    {
        var messageBox = TestDependencies.MessageBox();
        var vm = CreateVm(messageBox);
        vm.Name = "Tester";

        await vm.SendSupportRequestCommand.ExecuteAsync(null);

        messageBox.Verify(m => m.EnterNameMessageBoxAsync(), Times.Never);
        messageBox.Verify(m => m.EnterEmailMessageBoxAsync(), Times.Once);
        messageBox.Verify(m => m.EnterSupportRequestMessageBoxAsync(), Times.Never);
    }

    [Fact]
    public async Task SendRequest_EmptyRequest_PromptsForRequest()
    {
        var messageBox = TestDependencies.MessageBox();
        var vm = CreateVm(messageBox);
        vm.Name = "Tester";
        vm.Email = "tester@example.com";

        await vm.SendSupportRequestCommand.ExecuteAsync(null);

        messageBox.Verify(m => m.EnterNameMessageBoxAsync(), Times.Never);
        messageBox.Verify(m => m.EnterEmailMessageBoxAsync(), Times.Never);
        messageBox.Verify(m => m.EnterSupportRequestMessageBoxAsync(), Times.Once);
    }

    [Fact]
    public async Task SendRequest_ApiSuccess_ClearsFormAndRaisesFormCleared()
    {
        var messageBox = TestDependencies.MessageBox();
        var vm = CreateVm(messageBox);
        vm.Name = "Tester";
        vm.Email = "tester@example.com";
        vm.SupportRequest = "I need help with my emulator setup.";
        var formCleared = false;
        vm.FormCleared += (_, _) => { formCleared = true; };

        await vm.SendSupportRequestCommand.ExecuteAsync(null);

        Assert.True(formCleared);
        Assert.Equal("", vm.Name);
        Assert.Equal("", vm.Email);
        Assert.Equal("", vm.SupportRequest);
        Assert.False(vm.IsLoading);
        messageBox.Verify(m => m.SupportRequestSuccessMessageBoxAsync(), Times.Once);
        messageBox.Verify(m => m.SupportRequestSendErrorMessageBoxAsync(), Times.Never);
    }

    [Fact]
    public async Task SendRequest_ApiError_ShowsErrorMessage()
    {
        var messageBox = TestDependencies.MessageBox();
        var client = TestDependencies.HttpClientWith(_ =>
            new HttpResponseMessage(System.Net.HttpStatusCode.InternalServerError)
            {
                Content = new StringContent("oops", Encoding.UTF8, "application/json")
            });
        var vm = CreateVm(messageBox, httpClient: client);
        vm.Name = "Tester";
        vm.Email = "tester@example.com";
        vm.SupportRequest = "Broken.";

        await vm.SendSupportRequestCommand.ExecuteAsync(null);

        messageBox.Verify(m => m.SupportRequestSendErrorMessageBoxAsync(), Times.Once);
        messageBox.Verify(m => m.SupportRequestSuccessMessageBoxAsync(), Times.Never);
        Assert.False(vm.IsLoading);
    }

    [Fact]
    public async Task SendRequest_NetworkFailure_ShowsErrorMessage()
    {
        var messageBox = TestDependencies.MessageBox();
        var client = TestDependencies.HttpClientWith(_ => throw new HttpRequestException("network down"));
        var vm = CreateVm(messageBox, httpClient: client);
        vm.Name = "Tester";
        vm.Email = "tester@example.com";
        vm.SupportRequest = "Broken.";

        await vm.SendSupportRequestCommand.ExecuteAsync(null);

        messageBox.Verify(m => m.SupportRequestSendErrorMessageBoxAsync(), Times.Once);
        Assert.False(vm.IsLoading);
    }

    [Fact]
    public void CloseCommand_RaisesCloseRequested()
    {
        var messageBox = TestDependencies.MessageBox();
        var vm = CreateVm(messageBox);
        var raised = false;
        vm.CloseRequested += (_, _) => { raised = true; };

        vm.CloseCommand.Execute(null);

        Assert.True(raised);
    }
}