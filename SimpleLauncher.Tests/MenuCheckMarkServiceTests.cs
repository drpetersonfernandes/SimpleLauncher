using System.Windows.Controls;
using SimpleLauncher.Interfaces;
using SimpleLauncher.Services.MenuCheckMark;
using SimpleLauncher.Tests.TestHelpers;
using Xunit;

namespace SimpleLauncher.Tests;

/// <summary>
/// Tests for <see cref="MenuCheckMarkService"/> check-mark invariants:
/// exactly one item checked per group, matching the selected value.
/// </summary>
public class MenuCheckMarkServiceTests
{
    /// <summary>
    /// Concrete host exposing real <see cref="MenuItem"/> instances for every property,
    /// so the service can toggle IsChecked without WPF application infrastructure.
    /// </summary>
    private sealed class FakeMenuCheckMarkHost : IMenuCheckMarkHost
    {
        public MenuItem Size50 { get; } = new();
        public MenuItem Size100 { get; } = new();
        public MenuItem Size150 { get; } = new();
        public MenuItem Size200 { get; } = new();
        public MenuItem Size250 { get; } = new();
        public MenuItem Size300 { get; } = new();
        public MenuItem Size350 { get; } = new();
        public MenuItem Size400 { get; } = new();
        public MenuItem Size450 { get; } = new();
        public MenuItem Size500 { get; } = new();
        public MenuItem Size550 { get; } = new();
        public MenuItem Size600 { get; } = new();
        public MenuItem Size650 { get; } = new();
        public MenuItem Size700 { get; } = new();
        public MenuItem Size750 { get; } = new();
        public MenuItem Size800 { get; } = new();
        public MenuItem Page100 { get; } = new();
        public MenuItem Page200 { get; } = new();
        public MenuItem Page300 { get; } = new();
        public MenuItem Page400 { get; } = new();
        public MenuItem Page500 { get; } = new();
        public MenuItem Page1000 { get; } = new();
        public MenuItem Page10000 { get; } = new();
        public MenuItem Page1000000 { get; } = new();
        public MenuItem ShowAll { get; } = new();
        public MenuItem ShowWithCover { get; } = new();
        public MenuItem ShowWithoutCover { get; } = new();
        public MenuItem Square { get; } = new();
        public MenuItem Wider { get; } = new();
        public MenuItem SuperWider { get; } = new();
        public MenuItem SuperWider2 { get; } = new();
        public MenuItem Taller { get; } = new();
        public MenuItem SuperTaller { get; } = new();
        public MenuItem SuperTaller2 { get; } = new();
        public MenuItem FilenameDisplayOriginal { get; } = new();
        public MenuItem FilenameDisplayCleanUp { get; } = new();
        public MenuItem FilenameDisplayNoFilename { get; } = new();
        public MenuItem DisplayMachineNameToggle { get; } = new();
        public MenuItem FilenameFontSizeSmall { get; } = new();
        public MenuItem FilenameFontSizeNormal { get; } = new();
        public MenuItem FilenameFontSizeBig { get; } = new();
        public MenuItem MachineNameFontSizeSmall { get; } = new();
        public MenuItem MachineNameFontSizeNormal { get; } = new();
        public MenuItem MachineNameFontSizeBig { get; } = new();
        public MenuItem GridView { get; } = new();
        public MenuItem ListView { get; } = new();
    }

    private static int CountChecked(params MenuItem[] items)
    {
        return items.Count(static i => i.IsChecked);
    }

    [Fact]
    public void UpdateThumbnailSizeCheckMarks_ChecksExactlyOneItem()
    {
        StaApartment.Run(() =>
        {
            var host = new FakeMenuCheckMarkHost();
            var service = new MenuCheckMarkService();
            service.Initialize(host);

            service.UpdateThumbnailSizeCheckMarks(300);

            var sizes = new[] { host.Size50, host.Size100, host.Size150, host.Size200, host.Size250, host.Size300, host.Size350, host.Size400, host.Size450, host.Size500, host.Size550, host.Size600, host.Size650, host.Size700, host.Size750, host.Size800 };
            Assert.Equal(1, CountChecked(sizes));
            Assert.True(host.Size300.IsChecked);
        });
    }

    [Theory]
    [InlineData(100)]
    [InlineData(500)]
    [InlineData(1000000)]
    public void UpdateNumberOfGamesPerPageCheckMarks_ChecksExactlyOneItem(int selected)
    {
        StaApartment.Run(() =>
        {
            var host = new FakeMenuCheckMarkHost();
            var service = new MenuCheckMarkService();
            service.Initialize(host);

            service.UpdateNumberOfGamesPerPageCheckMarks(selected);

            var pages = new[] { host.Page100, host.Page200, host.Page300, host.Page400, host.Page500, host.Page1000, host.Page10000, host.Page1000000 };
            Assert.Equal(1, CountChecked(pages));
            var expected = selected switch
            {
                100 => host.Page100,
                500 => host.Page500,
                _ => host.Page1000000
            };
            Assert.True(expected.IsChecked);
        });
    }

    [Theory]
    [InlineData("ShowAll")]
    [InlineData("ShowWithCover")]
    [InlineData("ShowWithoutCover")]
    public void UpdateShowGamesCheckMarks_ChecksExactlyOneItem(string selected)
    {
        StaApartment.Run(() =>
        {
            var host = new FakeMenuCheckMarkHost();
            var service = new MenuCheckMarkService();
            service.Initialize(host);

            service.UpdateShowGamesCheckMarks(selected);

            var items = new[] { host.ShowAll, host.ShowWithCover, host.ShowWithoutCover };
            Assert.Equal(1, CountChecked(items));
        });
    }

    [Fact]
    public void UpdateShowGamesCheckMarks_IsCaseSensitive()
    {
        StaApartment.Run(() =>
        {
            var host = new FakeMenuCheckMarkHost();
            var service = new MenuCheckMarkService();
            service.Initialize(host);

            service.UpdateShowGamesCheckMarks("showall");

            Assert.Equal(0, CountChecked(host.ShowAll, host.ShowWithCover, host.ShowWithoutCover));
        });
    }

    [Theory]
    [InlineData("Square")]
    [InlineData("Wider")]
    [InlineData("SuperWider")]
    [InlineData("Taller")]
    [InlineData("SuperTaller2")]
    public void UpdateButtonAspectRatioCheckMarks_ChecksExactlyOneItem(string selected)
    {
        StaApartment.Run(() =>
        {
            var host = new FakeMenuCheckMarkHost();
            var service = new MenuCheckMarkService();
            service.Initialize(host);

            service.UpdateButtonAspectRatioCheckMarks(selected);

            var items = new[] { host.Square, host.Wider, host.SuperWider, host.SuperWider2, host.Taller, host.SuperTaller, host.SuperTaller2 };
            Assert.Equal(1, CountChecked(items));
        });
    }

    [Theory]
    [InlineData("Original")]
    [InlineData("CleanUp")]
    [InlineData("NoFilename")]
    public void UpdateFilenameDisplayModeCheckMarks_ChecksExactlyOneItem(string selected)
    {
        StaApartment.Run(() =>
        {
            var host = new FakeMenuCheckMarkHost();
            var service = new MenuCheckMarkService();
            service.Initialize(host);

            service.UpdateFilenameDisplayModeCheckMarks(selected);

            var items = new[] { host.FilenameDisplayOriginal, host.FilenameDisplayCleanUp, host.FilenameDisplayNoFilename };
            Assert.Equal(1, CountChecked(items));
        });
    }

    [Theory]
    [InlineData("Small")]
    [InlineData("Normal")]
    [InlineData("Big")]
    public void UpdateFilenameFontSizeCheckMarks_ChecksExactlyOneItem(string selected)
    {
        StaApartment.Run(() =>
        {
            var host = new FakeMenuCheckMarkHost();
            var service = new MenuCheckMarkService();
            service.Initialize(host);

            service.UpdateFilenameFontSizeCheckMarks(selected);

            var items = new[] { host.FilenameFontSizeSmall, host.FilenameFontSizeNormal, host.FilenameFontSizeBig };
            Assert.Equal(1, CountChecked(items));
        });
    }

    [Theory]
    [InlineData("Small")]
    [InlineData("Normal")]
    [InlineData("Big")]
    public void UpdateMachineNameFontSizeCheckMarks_ChecksExactlyOneItem(string selected)
    {
        StaApartment.Run(() =>
        {
            var host = new FakeMenuCheckMarkHost();
            var service = new MenuCheckMarkService();
            service.Initialize(host);

            service.UpdateMachineNameFontSizeCheckMarks(selected);

            var items = new[] { host.MachineNameFontSizeSmall, host.MachineNameFontSizeNormal, host.MachineNameFontSizeBig };
            Assert.Equal(1, CountChecked(items));
        });
    }

    [Fact]
    public void SetViewMode_ListView_ChecksListViewOnly()
    {
        StaApartment.Run(() =>
        {
            var host = new FakeMenuCheckMarkHost();
            var service = new MenuCheckMarkService();
            service.Initialize(host);

            service.SetViewMode("ListView");

            Assert.True(host.ListView.IsChecked);
            Assert.False(host.GridView.IsChecked);
        });
    }

    [Fact]
    public void SetViewMode_GridViewOrUnknown_ChecksGridView()
    {
        StaApartment.Run(() =>
        {
            var host = new FakeMenuCheckMarkHost();
            var service = new MenuCheckMarkService();
            service.Initialize(host);

            service.SetViewMode("GridView");

            Assert.True(host.GridView.IsChecked);
            Assert.False(host.ListView.IsChecked);
        });
    }
}
