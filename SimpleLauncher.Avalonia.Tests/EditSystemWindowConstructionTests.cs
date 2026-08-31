using Microsoft.Extensions.Configuration;
using Moq;
using SimpleLauncher.Avalonia.Services;
using SimpleLauncher.Avalonia.Services.Favorites;
using SimpleLauncher.Avalonia.Services.PlayHistory;
using SimpleLauncher.Avalonia.Services.SystemManager;
using SimpleLauncher.Core.Interfaces;
using SimpleLauncher.Core.Services.ParameterResolver;
using SimpleLauncher.Core.Services.SystemConfiguration;

namespace SimpleLauncher.Avalonia.Tests;

/// <summary>
///     Regression tests that construct the real EditSystemWindow on the headless UI thread.
///     XAML population errors (invalid property values, unrecognized resource references,
///     missing event handlers) throw inside InitializeComponent and silently prevented the
///     Edit System window from opening — e.g. Cursor="SizeWE" (a WPF-only cursor name)
///     threw 'Unrecognized cursor type' on Avalonia. Construction alone exercises
///     !XamlIlPopulate, so these tests catch that class of breakage without showing a dialog.
/// </summary>
public class EditSystemWindowConstructionTests
{
    private static EditSystemWindow CreateWindow(string? preSelectedSystemName = null)
    {
        return HeadlessAvalonia.RunOnUiThread(() =>
        {
            var configuration = new ConfigurationBuilder().Build();
            var logger = TestDependencies.Logger().Object;
            var messageBox = TestDependencies.MessageBox().Object;

            var settings = TestDependencies.Settings();
            var playSound = TestDependencies.PlaySound(settings);
            var writer = new SystemConfigurationWriterService(configuration, logger);
            var systemManager = new SystemManagerService(configuration);
            var filePicker = new Mock<IFilePickerService>().Object;
            var favorites = new FavoritesManager();
            var playHistory = new PlayHistoryManager();
            var httpFactory = new Mock<IHttpClientFactory>();
            var parameterResolver = new ParameterResolverService(httpFactory.Object, logger);
            var helpUser = new AvaloniaHelpUserService(logger, messageBox);
            var localization = new LocalizationService();

            return new EditSystemWindow(
                playSound,
                configuration,
                messageBox,
                logger,
                writer,
                systemManager,
                filePicker,
                favorites,
                playHistory,
                parameterResolver,
                helpUser,
                localization,
                settings,
                preSelectedSystemName);
        });
    }

    [Fact]
    public void Construction_DoesNotThrow()
    {
        var window = CreateWindow();
        Assert.NotNull(window);
    }

    [Fact]
    public void Construction_WithPreSelectedSystem_DoesNotThrow()
    {
        var window = CreateWindow("Nintendo SNES");
        Assert.NotNull(window);
    }
}