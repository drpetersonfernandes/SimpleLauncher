using System.Windows.Controls;

namespace SimpleLauncher.Services.MenuCheckMark;

public interface IMenuCheckMarkHost
{
    // Thumbnail sizes (16 items)
    MenuItem Size50 { get; }
    MenuItem Size100 { get; }
    MenuItem Size150 { get; }
    MenuItem Size200 { get; }
    MenuItem Size250 { get; }
    MenuItem Size300 { get; }
    MenuItem Size350 { get; }
    MenuItem Size400 { get; }
    MenuItem Size450 { get; }
    MenuItem Size500 { get; }
    MenuItem Size550 { get; }
    MenuItem Size600 { get; }
    MenuItem Size650 { get; }
    MenuItem Size700 { get; }
    MenuItem Size750 { get; }
    MenuItem Size800 { get; }

    // Pages per page (8 items)
    MenuItem Page100 { get; }
    MenuItem Page200 { get; }
    MenuItem Page300 { get; }
    MenuItem Page400 { get; }
    MenuItem Page500 { get; }
    MenuItem Page1000 { get; }
    MenuItem Page10000 { get; }
    MenuItem Page1000000 { get; }

    // Show games (3 items)
    MenuItem ShowAll { get; }
    MenuItem ShowWithCover { get; }
    MenuItem ShowWithoutCover { get; }

    // Aspect ratio (7 items)
    MenuItem Square { get; }
    MenuItem Wider { get; }
    MenuItem SuperWider { get; }
    MenuItem SuperWider2 { get; }
    MenuItem Taller { get; }
    MenuItem SuperTaller { get; }
    MenuItem SuperTaller2 { get; }

    // Filename display mode (3 items)
    MenuItem FilenameDisplayOriginal { get; }
    MenuItem FilenameDisplayCleanUp { get; }
    MenuItem FilenameDisplayNoFilename { get; }

    // Machine name toggle (1 item)
    MenuItem DisplayMachineNameToggle { get; }

    // Filename font size (3 items)
    MenuItem FilenameFontSizeSmall { get; }
    MenuItem FilenameFontSizeNormal { get; }
    MenuItem FilenameFontSizeBig { get; }

    // Machine name font size (3 items)
    MenuItem MachineNameFontSizeSmall { get; }
    MenuItem MachineNameFontSizeNormal { get; }
    MenuItem MachineNameFontSizeBig { get; }

    // View mode (2 items)
    MenuItem GridView { get; }
    MenuItem ListView { get; }
}
