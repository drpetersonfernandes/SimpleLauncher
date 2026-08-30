using SimpleLauncher.Avalonia.ViewModels;

namespace SimpleLauncher.Avalonia.Tests;

/// <summary>
///     Tests for the UpdateHistoryWindow ViewModel (Phase 4.1 port).
/// </summary>
public class UpdateHistoryViewModelTests
{
    [Fact]
    public async Task Initialize_WhatsNewMissing_ShowsFallbackMessage()
    {
        var whatsNew = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "WhatsNew.md");
        if (File.Exists(whatsNew)) File.Delete(whatsNew);

        var vm = new UpdateHistoryViewModel(TestDependencies.Logger().Object,
            TestDependencies.ResourceProvider().Object);
        await vm.InitializeAsync();

        Assert.Contains("whatsnew.md", vm.MarkdownContent, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task Initialize_WhatsNewPresent_LoadsContent()
    {
        var whatsNew = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "WhatsNew.md");
        File.WriteAllText(whatsNew, "# Release 5.6.1\n- Fixed a bug");
        try
        {
            var vm = new UpdateHistoryViewModel(TestDependencies.Logger().Object,
                TestDependencies.ResourceProvider().Object);
            await vm.InitializeAsync();

            Assert.Contains("Release 5.6.1", vm.MarkdownContent, StringComparison.OrdinalIgnoreCase);
            Assert.Contains("Fixed a bug", vm.MarkdownContent, StringComparison.OrdinalIgnoreCase);
        }
        finally
        {
            File.Delete(whatsNew);
        }
    }
}