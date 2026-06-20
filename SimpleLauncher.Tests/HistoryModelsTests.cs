using SimpleLauncher.Services.RomHistory.Models;
using Xunit;

namespace SimpleLauncher.Tests;

/// <summary>
/// Tests for ROM history data models: <see cref="HistoryData"/>, <see cref="EntryData"/>,
/// <see cref="SystemsData"/>, <see cref="SystemItemData"/>, <see cref="SoftwareData"/>, <see cref="ItemData"/>.
/// </summary>
public class HistoryModelsTests
{
    // HistoryData tests

    [Fact]
    public void HistoryDataDefaultValues()
    {
        var data = new HistoryData();
        Assert.Null(data.Version);
        Assert.Null(data.Date);
        Assert.NotNull(data.Entries);
        Assert.Empty(data.Entries);
    }

    [Fact]
    public void HistoryDataPropertiesCanBeSet()
    {
        var data = new HistoryData
        {
            Version = "1.0",
            Date = "2024-01-15",
            Entries = [new EntryData()]
        };

        Assert.Equal("1.0", data.Version);
        Assert.Equal("2024-01-15", data.Date);
        Assert.Single(data.Entries);
    }

    [Fact]
    public void HistoryDataEntriesSupportsMultipleItems()
    {
        var data = new HistoryData
        {
            Entries =
            [
                new EntryData { Text = "Entry 1" },
                new EntryData { Text = "Entry 2" },
                new EntryData { Text = "Entry 3" }
            ]
        };

        Assert.Equal(3, data.Entries.Length);
        Assert.Equal("Entry 1", data.Entries[0].Text);
        Assert.Equal("Entry 2", data.Entries[1].Text);
        Assert.Equal("Entry 3", data.Entries[2].Text);
    }

    // EntryData tests

    [Fact]
    public void EntryDataDefaultValues()
    {
        var entry = new EntryData();
        Assert.Null(entry.Software);
        Assert.Null(entry.Systems);
        Assert.Null(entry.Text);
    }

    [Fact]
    public void EntryDataPropertiesCanBeSet()
    {
        var entry = new EntryData
        {
            Software = new SoftwareData(),
            Systems = new SystemsData(),
            Text = "Game info"
        };

        Assert.NotNull(entry.Software);
        Assert.NotNull(entry.Systems);
        Assert.Equal("Game info", entry.Text);
    }

    // SystemsData tests

    [Fact]
    public void SystemsDataDefaultSystemItemsIsEmptyArray()
    {
        var systems = new SystemsData();
        Assert.NotNull(systems.SystemItems);
        Assert.Empty(systems.SystemItems);
    }

    [Fact]
    public void SystemsDataSystemItemsCanBePopulated()
    {
        var systems = new SystemsData
        {
            SystemItems =
            [
                new SystemItemData { Name = "NES", Game = "Super Mario Bros." },
                new SystemItemData { Name = "SNES", Game = "Super Mario World" }
            ]
        };

        Assert.Equal(2, systems.SystemItems.Length);
        Assert.Equal("NES", systems.SystemItems[0].Name);
        Assert.Equal("SNES", systems.SystemItems[1].Name);
    }

    // SystemItemData tests

    [Fact]
    public void SystemItemDataDefaultValues()
    {
        var item = new SystemItemData();
        Assert.Null(item.Name);
        Assert.Null(item.Game);
    }

    [Fact]
    public void SystemItemDataPropertiesCanBeSet()
    {
        var item = new SystemItemData
        {
            Name = "NES",
            Game = "Super Mario Bros."
        };

        Assert.Equal("NES", item.Name);
        Assert.Equal("Super Mario Bros.", item.Game);
    }

    [Fact]
    public void SystemItemDataSupportsUnicode()
    {
        var item = new SystemItemData
        {
            Name = "ファミコン",
            Game = "スーパーマリオブラザーズ"
        };

        Assert.Equal("ファミコン", item.Name);
        Assert.Equal("スーパーマリオブラザーズ", item.Game);
    }

    // SoftwareData tests

    [Fact]
    public void SoftwareDataDefaultItemsIsEmptyArray()
    {
        var software = new SoftwareData();
        Assert.NotNull(software.Items);
        Assert.Empty(software.Items);
    }

    [Fact]
    public void SoftwareDataItemsCanBePopulated()
    {
        var software = new SoftwareData
        {
            Items =
            [
                new ItemData { List = "list1", Name = "Game1", Game = "ROM1" },
                new ItemData { List = "list2", Name = "Game2", Game = "ROM2" }
            ]
        };

        Assert.Equal(2, software.Items.Length);
        Assert.Equal("Game1", software.Items[0].Name);
        Assert.Equal("Game2", software.Items[1].Name);
    }

    // ItemData tests

    [Fact]
    public void ItemDataDefaultValues()
    {
        var item = new ItemData();
        Assert.Null(item.List);
        Assert.Null(item.Name);
        Assert.Null(item.Game);
    }

    [Fact]
    public void ItemDataPropertiesCanBeSet()
    {
        var item = new ItemData
        {
            List = "No-Intro",
            Name = "Super Mario Bros.",
            Game = "Super Mario Bros. (World).nes"
        };

        Assert.Equal("No-Intro", item.List);
        Assert.Equal("Super Mario Bros.", item.Name);
        Assert.Equal("Super Mario Bros. (World).nes", item.Game);
    }

    [Fact]
    public void ItemDataSupportsSpecialCharacters()
    {
        var item = new ItemData
        {
            Name = "Game (v1.0) [!] {proto}",
            Game = "game (v1.0) [!] {proto}.zip"
        };

        Assert.Contains("(", item.Name);
        Assert.Contains("[", item.Name);
        Assert.Contains("{", item.Name);
    }

    // Full hierarchy integration test

    [Fact]
    public void FullHistoryDataHierarchy()
    {
        var data = new HistoryData
        {
            Version = "2.0",
            Date = "2024-12-25",
            Entries =
            [
                new EntryData
                {
                    Text = "Classic game information",
                    Software = new SoftwareData
                    {
                        Items =
                        [
                            new ItemData { List = "No-Intro", Name = "Super Mario Bros.", Game = "smb.nes" }
                        ]
                    },
                    Systems = new SystemsData
                    {
                        SystemItems =
                        [
                            new SystemItemData { Name = "NES", Game = "Super Mario Bros." }
                        ]
                    }
                }
            ]
        };

        Assert.Equal("2.0", data.Version);
        Assert.Single(data.Entries);
        Assert.Equal("Classic game information", data.Entries[0].Text);
        var softwareData = data.Entries[0].Software;
        if (softwareData != null)
        {
            Assert.Single(softwareData.Items);
            Assert.Equal("smb.nes", softwareData.Items[0].Game);
        }

        var systemsData = data.Entries[0].Systems;
        if (systemsData != null)
        {
            Assert.Single(systemsData.SystemItems);
            Assert.Equal("NES", systemsData.SystemItems[0].Name);
        }
    }

    [Fact]
    public void EmptyHistoryDataIsValid()
    {
        var data = new HistoryData();
        Assert.NotNull(data.Entries);
        Assert.Empty(data.Entries);
    }
}
