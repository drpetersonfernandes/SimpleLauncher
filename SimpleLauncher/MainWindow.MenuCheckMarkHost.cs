using System.Windows.Controls;

namespace SimpleLauncher;

using Interfaces;

/// <summary>
/// Partial MainWindow implementing <see cref="IMenuCheckMarkHost"/> for menu check mark state management.
/// </summary>
public partial class MainWindow
{
    MenuItem IMenuCheckMarkHost.Size50 => Size50;
    MenuItem IMenuCheckMarkHost.Size100 => Size100;
    MenuItem IMenuCheckMarkHost.Size150 => Size150;
    MenuItem IMenuCheckMarkHost.Size200 => Size200;
    MenuItem IMenuCheckMarkHost.Size250 => Size250;
    MenuItem IMenuCheckMarkHost.Size300 => Size300;
    MenuItem IMenuCheckMarkHost.Size350 => Size350;
    MenuItem IMenuCheckMarkHost.Size400 => Size400;
    MenuItem IMenuCheckMarkHost.Size450 => Size450;
    MenuItem IMenuCheckMarkHost.Size500 => Size500;
    MenuItem IMenuCheckMarkHost.Size550 => Size550;
    MenuItem IMenuCheckMarkHost.Size600 => Size600;
    MenuItem IMenuCheckMarkHost.Size650 => Size650;
    MenuItem IMenuCheckMarkHost.Size700 => Size700;
    MenuItem IMenuCheckMarkHost.Size750 => Size750;
    MenuItem IMenuCheckMarkHost.Size800 => Size800;

    MenuItem IMenuCheckMarkHost.Page100 => Page100;
    MenuItem IMenuCheckMarkHost.Page200 => Page200;
    MenuItem IMenuCheckMarkHost.Page300 => Page300;
    MenuItem IMenuCheckMarkHost.Page400 => Page400;
    MenuItem IMenuCheckMarkHost.Page500 => Page500;
    MenuItem IMenuCheckMarkHost.Page1000 => Page1000;
    MenuItem IMenuCheckMarkHost.Page10000 => Page10000;
    MenuItem IMenuCheckMarkHost.Page1000000 => Page1000000;

    MenuItem IMenuCheckMarkHost.ShowAll => ShowAll;
    MenuItem IMenuCheckMarkHost.ShowWithCover => ShowWithCover;
    MenuItem IMenuCheckMarkHost.ShowWithoutCover => ShowWithoutCover;

    MenuItem IMenuCheckMarkHost.Square => Square;
    MenuItem IMenuCheckMarkHost.Wider => Wider;
    MenuItem IMenuCheckMarkHost.SuperWider => SuperWider;
    MenuItem IMenuCheckMarkHost.SuperWider2 => SuperWider2;
    MenuItem IMenuCheckMarkHost.Taller => Taller;
    MenuItem IMenuCheckMarkHost.SuperTaller => SuperTaller;
    MenuItem IMenuCheckMarkHost.SuperTaller2 => SuperTaller2;

    MenuItem IMenuCheckMarkHost.FilenameDisplayOriginal => FilenameDisplayOriginal;
    MenuItem IMenuCheckMarkHost.FilenameDisplayCleanUp => FilenameDisplayCleanUp;
    MenuItem IMenuCheckMarkHost.FilenameDisplayNoFilename => FilenameDisplayNoFilename;

    MenuItem IMenuCheckMarkHost.DisplayMachineNameToggle => DisplayMachineNameToggle;

    MenuItem IMenuCheckMarkHost.FilenameFontSizeSmall => FilenameFontSizeSmall;
    MenuItem IMenuCheckMarkHost.FilenameFontSizeNormal => FilenameFontSizeNormal;
    MenuItem IMenuCheckMarkHost.FilenameFontSizeBig => FilenameFontSizeBig;

    MenuItem IMenuCheckMarkHost.MachineNameFontSizeSmall => MachineNameFontSizeSmall;
    MenuItem IMenuCheckMarkHost.MachineNameFontSizeNormal => MachineNameFontSizeNormal;
    MenuItem IMenuCheckMarkHost.MachineNameFontSizeBig => MachineNameFontSizeBig;

    MenuItem IMenuCheckMarkHost.GridView => GridView;
    MenuItem IMenuCheckMarkHost.ListView => ListView;
}
