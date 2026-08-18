using System.Text;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using SimpleLauncher.Avalonia.Services.AvaloniaServices;
using SimpleLauncher.Avalonia.ViewModels;
using SimpleLauncher.Core.Interfaces;
using SimpleLauncher.Core.Services.DownloadService;
using SimpleLauncher.Core.Services.EasyMode;

namespace SimpleLauncher.Avalonia.Tests;

/// <summary>
/// Tests for the DownloadImagePackWindow ViewModel (Phase 4.1 port). The EasyMode
/// configuration is served by a fake HTTP handler (no live endpoints), downloads are
/// never started, and the temp extraction service is a no-op mock.
/// </summary>
public class DownloadImagePackViewModelTests
{
    private const string EasyModeJson = """
        [
          {
            "systemName": "NES",
            "systemFolder": "roms/nes",
            "systemImageFolder": "images/nes",
            "fileFormatsToSearch": [".nes"],
            "fileFormatsToLaunch": [".nes"],
            "emulators": {
              "emulator": {
                "emulatorName": "Mesen",
                "imagePackDownloadLink": "https://example.com/pack1.zip",
                "imagePackDownloadLink2": "https://example.com/pack2.zip",
                "imagePackDownloadExtractPath": "images/nes"
              }
            }
          },
          {
            "systemName": "NoPackSystem",
            "systemFolder": "roms/nopack",
            "emulators": { "emulator": { "emulatorName": "NoPacker" } }
          }
        ]
        """;

    private static DownloadImagePackViewModel CreateVm()
    {
        var messageBox = TestDependencies.MessageBox();
        var logger = TestDependencies.Logger();

        var settings = TestDependencies.Settings(messageBox: messageBox);
        settings.EnableNotificationSound = false;
        var playSound = TestDependencies.PlaySound(settings);

        // Force the API path: no local easymode.xml in the test output.
        File.Delete(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "easymode.xml"));
        File.Delete(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "easymode_arm64.xml"));

        var config = TestEnvironment.ConfigurationFromJson("""{"EasyModeCacheDurationMinutes": "0"}""");
        var httpClient = new HttpClient(new JsonHandler(EasyModeJson)) { BaseAddress = new Uri("https://example.com/") };
        var httpFactory = TestDependencies.HttpFactory(httpClient);

        var easyModeManager = new EasyModeManager(logger.Object, config, httpFactory.Object, logger.Object);
        var resourceProvider = TestDependencies.ResourceProvider().Object;

        var downloadManager = new DownloadManager(
            httpFactory.Object,
            new Mock<IExtractionService>().Object,
            logger.Object,
            resourceProvider,
            new AvaloniaDispatcherService());

        var services = new ServiceCollection();
        services.AddSingleton(downloadManager);
        var scopeFactory = services.BuildServiceProvider().GetRequiredService<IServiceScopeFactory>();

        return new DownloadImagePackViewModel(playSound, logger.Object, easyModeManager, messageBox.Object, scopeFactory, resourceProvider);
    }

    [Fact]
    public async Task Initialize_LoadsSystemsWithImagePacks()
    {
        var vm = CreateVm();
        try
        {
            await vm.InitializeAsync();

            Assert.Contains("NES", vm.SystemNames);
            Assert.DoesNotContain("NoPackSystem", vm.SystemNames); // no image pack links → filtered
        }
        finally
        {
            vm.Dispose();
        }
    }

    [Fact]
    public async Task SelectingSystem_PopulatesImagePackList()
    {
        var vm = CreateVm();
        try
        {
            await vm.InitializeAsync();
            vm.SelectedSystemName = "NES";

            Assert.Equal(2, vm.ImagePacksToDisplay.Count);
            Assert.Equal("Image Pack 1", vm.ImagePacksToDisplay[0].DisplayName);
            Assert.Equal("https://example.com/pack1.zip", vm.ImagePacksToDisplay[0].DownloadUrl);
            Assert.Equal("images/nes", vm.ImagePacksToDisplay[0].ExtractPath);
            Assert.Equal("Image Pack 2", vm.ImagePacksToDisplay[1].DisplayName);
            Assert.Equal("https://example.com/pack2.zip", vm.ImagePacksToDisplay[1].DownloadUrl);
        }
        finally
        {
            vm.Dispose();
        }
    }

    [Fact]
    public async Task SelectingUnknownSystem_YieldsEmptyList()
    {
        var vm = CreateVm();
        try
        {
            await vm.InitializeAsync();
            vm.SelectedSystemName = "DoesNotExist";

            Assert.Empty(vm.ImagePacksToDisplay);
        }
        finally
        {
            vm.Dispose();
        }
    }

    [Fact]
    public async Task SelectingEmptySystem_ClearsList()
    {
        var vm = CreateVm();
        try
        {
            await vm.InitializeAsync();
            vm.SelectedSystemName = "NES";
            Assert.Equal(2, vm.ImagePacksToDisplay.Count);

            vm.SelectedSystemName = "";
            Assert.Empty(vm.ImagePacksToDisplay);
        }
        finally
        {
            vm.Dispose();
        }
    }

    [Fact]
    public async Task StopDownload_DoesNotThrow()
    {
        var vm = CreateVm();
        try
        {
            await vm.InitializeAsync();
            vm.StopDownloadCommand.Execute(null);

            Assert.False(vm.IsStopEnabled);
            Assert.False(vm.IsOperationInProgress);
        }
        finally
        {
            vm.Dispose();
        }
    }

    [Fact]
    public async Task EmergencyOverlayRelease_ResetsUiState()
    {
        var vm = CreateVm();
        try
        {
            await vm.InitializeAsync();
            vm.EmergencyOverlayRelease();

            Assert.False(vm.IsLoading);
            Assert.False(vm.IsOperationInProgress);
            Assert.False(vm.IsStopEnabled);
        }
        finally
        {
            vm.Dispose();
        }
    }

    [Fact]
    public async Task CloseWindowRoutineAsync_Completes()
    {
        var vm = CreateVm();
        await vm.CloseWindowRoutineAsync();
    }

    [Fact]
    public void Dispose_IsIdempotent()
    {
        var vm = CreateVm();
        vm.Dispose();
        vm.Dispose();
    }

    private sealed class JsonHandler : HttpMessageHandler
    {
        private readonly string _json;

        public JsonHandler(string json)
        {
            _json = json;
        }

        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            return Task.FromResult(new HttpResponseMessage(System.Net.HttpStatusCode.OK)
            {
                Content = new StringContent(_json, Encoding.UTF8, "application/json")
            });
        }
    }
}