using System.Windows.Controls;

namespace SimpleLauncher.Interfaces;

/// <summary>
/// Provides access to menu item references for managing check marks across various settings menus.
/// </summary>
public interface IMenuCheckMarkHost
{
    /// <summary>
    /// Gets the 50px thumbnail size menu item.
    /// </summary>
    MenuItem Size50 { get; }

    /// <summary>
    /// Gets the 100px thumbnail size menu item.
    /// </summary>
    MenuItem Size100 { get; }

    /// <summary>
    /// Gets the 150px thumbnail size menu item.
    /// </summary>
    MenuItem Size150 { get; }

    /// <summary>
    /// Gets the 200px thumbnail size menu item.
    /// </summary>
    MenuItem Size200 { get; }

    /// <summary>
    /// Gets the 250px thumbnail size menu item.
    /// </summary>
    MenuItem Size250 { get; }

    /// <summary>
    /// Gets the 300px thumbnail size menu item.
    /// </summary>
    MenuItem Size300 { get; }

    /// <summary>
    /// Gets the 350px thumbnail size menu item.
    /// </summary>
    MenuItem Size350 { get; }

    /// <summary>
    /// Gets the 400px thumbnail size menu item.
    /// </summary>
    MenuItem Size400 { get; }

    /// <summary>
    /// Gets the 450px thumbnail size menu item.
    /// </summary>
    MenuItem Size450 { get; }

    /// <summary>
    /// Gets the 500px thumbnail size menu item.
    /// </summary>
    MenuItem Size500 { get; }

    /// <summary>
    /// Gets the 550px thumbnail size menu item.
    /// </summary>
    MenuItem Size550 { get; }

    /// <summary>
    /// Gets the 600px thumbnail size menu item.
    /// </summary>
    MenuItem Size600 { get; }

    /// <summary>
    /// Gets the 650px thumbnail size menu item.
    /// </summary>
    MenuItem Size650 { get; }

    /// <summary>
    /// Gets the 700px thumbnail size menu item.
    /// </summary>
    MenuItem Size700 { get; }

    /// <summary>
    /// Gets the 750px thumbnail size menu item.
    /// </summary>
    MenuItem Size750 { get; }

    /// <summary>
    /// Gets the 800px thumbnail size menu item.
    /// </summary>
    MenuItem Size800 { get; }

    /// <summary>
    /// Gets the 100 games per page menu item.
    /// </summary>
    MenuItem Page100 { get; }

    /// <summary>
    /// Gets the 200 games per page menu item.
    /// </summary>
    MenuItem Page200 { get; }

    /// <summary>
    /// Gets the 300 games per page menu item.
    /// </summary>
    MenuItem Page300 { get; }

    /// <summary>
    /// Gets the 400 games per page menu item.
    /// </summary>
    MenuItem Page400 { get; }

    /// <summary>
    /// Gets the 500 games per page menu item.
    /// </summary>
    MenuItem Page500 { get; }

    /// <summary>
    /// Gets the 1000 games per page menu item.
    /// </summary>
    MenuItem Page1000 { get; }

    /// <summary>
    /// Gets the 10000 games per page menu item.
    /// </summary>
    MenuItem Page10000 { get; }

    /// <summary>
    /// Gets the 1000000 games per page menu item.
    /// </summary>
    MenuItem Page1000000 { get; }

    /// <summary>
    /// Gets the show all games menu item.
    /// </summary>
    MenuItem ShowAll { get; }

    /// <summary>
    /// Gets the show games with cover art menu item.
    /// </summary>
    MenuItem ShowWithCover { get; }

    /// <summary>
    /// Gets the show games without cover art menu item.
    /// </summary>
    MenuItem ShowWithoutCover { get; }

    /// <summary>
    /// Gets the square aspect ratio menu item.
    /// </summary>
    MenuItem Square { get; }

    /// <summary>
    /// Gets the wider aspect ratio menu item.
    /// </summary>
    MenuItem Wider { get; }

    /// <summary>
    /// Gets the super wider aspect ratio menu item.
    /// </summary>
    MenuItem SuperWider { get; }

    /// <summary>
    /// Gets the second super wider aspect ratio menu item.
    /// </summary>
    MenuItem SuperWider2 { get; }

    /// <summary>
    /// Gets the taller aspect ratio menu item.
    /// </summary>
    MenuItem Taller { get; }

    /// <summary>
    /// Gets the super taller aspect ratio menu item.
    /// </summary>
    MenuItem SuperTaller { get; }

    /// <summary>
    /// Gets the second super taller aspect ratio menu item.
    /// </summary>
    MenuItem SuperTaller2 { get; }

    /// <summary>
    /// Gets the original filename display mode menu item.
    /// </summary>
    MenuItem FilenameDisplayOriginal { get; }

    /// <summary>
    /// Gets the cleaned-up filename display mode menu item.
    /// </summary>
    MenuItem FilenameDisplayCleanUp { get; }

    /// <summary>
    /// Gets the no-filename display mode menu item.
    /// </summary>
    MenuItem FilenameDisplayNoFilename { get; }

    /// <summary>
    /// Gets the machine name display toggle menu item.
    /// </summary>
    MenuItem DisplayMachineNameToggle { get; }

    /// <summary>
    /// Gets the small filename font size menu item.
    /// </summary>
    MenuItem FilenameFontSizeSmall { get; }

    /// <summary>
    /// Gets the normal filename font size menu item.
    /// </summary>
    MenuItem FilenameFontSizeNormal { get; }

    /// <summary>
    /// Gets the big filename font size menu item.
    /// </summary>
    MenuItem FilenameFontSizeBig { get; }

    /// <summary>
    /// Gets the small machine name font size menu item.
    /// </summary>
    MenuItem MachineNameFontSizeSmall { get; }

    /// <summary>
    /// Gets the normal machine name font size menu item.
    /// </summary>
    MenuItem MachineNameFontSizeNormal { get; }

    /// <summary>
    /// Gets the big machine name font size menu item.
    /// </summary>
    MenuItem MachineNameFontSizeBig { get; }

    /// <summary>
    /// Gets the grid view mode menu item.
    /// </summary>
    MenuItem GridView { get; }

    /// <summary>
    /// Gets the list view mode menu item.
    /// </summary>
    MenuItem ListView { get; }
}
