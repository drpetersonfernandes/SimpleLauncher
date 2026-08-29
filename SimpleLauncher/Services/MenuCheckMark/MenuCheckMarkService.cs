using SimpleLauncher.Interfaces;

namespace SimpleLauncher.Services.MenuCheckMark;

/// <summary>
///     Manages the checked state of menu items for thumbnail size, games per page, display options, and view mode.
/// </summary>
public class MenuCheckMarkService : IMenuCheckMarkService
{
    private IMenuCheckMarkHost _host = null!;

    /// <summary>
    ///     Initializes the service with the specified host that provides access to menu check mark controls.
    /// </summary>
    /// <param name="host">The host providing menu check mark UI elements.</param>
    public void Initialize(IMenuCheckMarkHost host)
    {
        _host = host;
    }

    /// <summary>
    ///     Updates the thumbnail size menu check marks to reflect the currently selected size.
    /// </summary>
    /// <param name="selectedSize">The currently selected thumbnail size in pixels.</param>
    public void UpdateThumbnailSizeCheckMarks(int selectedSize)
    {
        _host.Size50.IsChecked = selectedSize == 50;
        _host.Size100.IsChecked = selectedSize == 100;
        _host.Size150.IsChecked = selectedSize == 150;
        _host.Size200.IsChecked = selectedSize == 200;
        _host.Size250.IsChecked = selectedSize == 250;
        _host.Size300.IsChecked = selectedSize == 300;
        _host.Size350.IsChecked = selectedSize == 350;
        _host.Size400.IsChecked = selectedSize == 400;
        _host.Size450.IsChecked = selectedSize == 450;
        _host.Size500.IsChecked = selectedSize == 500;
        _host.Size550.IsChecked = selectedSize == 550;
        _host.Size600.IsChecked = selectedSize == 600;
        _host.Size650.IsChecked = selectedSize == 650;
        _host.Size700.IsChecked = selectedSize == 700;
        _host.Size750.IsChecked = selectedSize == 750;
        _host.Size800.IsChecked = selectedSize == 800;
    }

    /// <summary>
    ///     Updates the games-per-page menu check marks to reflect the currently selected count.
    /// </summary>
    /// <param name="selectedSize">The currently selected number of games per page.</param>
    public void UpdateNumberOfGamesPerPageCheckMarks(int selectedSize)
    {
        _host.Page100.IsChecked = selectedSize == 100;
        _host.Page200.IsChecked = selectedSize == 200;
        _host.Page300.IsChecked = selectedSize == 300;
        _host.Page400.IsChecked = selectedSize == 400;
        _host.Page500.IsChecked = selectedSize == 500;
        _host.Page1000.IsChecked = selectedSize == 1000;
        _host.Page10000.IsChecked = selectedSize == 10000;
        _host.Page1000000.IsChecked = selectedSize == 1000000;
    }

    /// <summary>
    ///     Updates the show-games filter menu check marks to reflect the selected filter mode.
    /// </summary>
    public void UpdateShowGamesCheckMarks(string selectedValue)
    {
        _host.ShowAll.IsChecked = string.Equals(selectedValue, "ShowAll", StringComparison.Ordinal);
        _host.ShowWithCover.IsChecked = string.Equals(selectedValue, "ShowWithCover", StringComparison.Ordinal);
        _host.ShowWithoutCover.IsChecked = string.Equals(selectedValue, "ShowWithoutCover", StringComparison.Ordinal);
    }

    /// <summary>
    ///     Updates the button aspect ratio menu check marks to reflect the selected aspect ratio.
    /// </summary>
    public void UpdateButtonAspectRatioCheckMarks(string selectedValue)
    {
        _host.Square.IsChecked = string.Equals(selectedValue, "Square", StringComparison.Ordinal);
        _host.Wider.IsChecked = string.Equals(selectedValue, "Wider", StringComparison.Ordinal);
        _host.SuperWider.IsChecked = string.Equals(selectedValue, "SuperWider", StringComparison.Ordinal);
        _host.SuperWider2.IsChecked = string.Equals(selectedValue, "SuperWider2", StringComparison.Ordinal);
        _host.Taller.IsChecked = string.Equals(selectedValue, "Taller", StringComparison.Ordinal);
        _host.SuperTaller.IsChecked = string.Equals(selectedValue, "SuperTaller", StringComparison.Ordinal);
        _host.SuperTaller2.IsChecked = string.Equals(selectedValue, "SuperTaller2", StringComparison.Ordinal);
    }

    /// <summary>
    ///     Updates the filename display mode menu check marks to reflect the selected display mode.
    /// </summary>
    public void UpdateFilenameDisplayModeCheckMarks(string selectedValue)
    {
        _host.FilenameDisplayOriginal.IsChecked = string.Equals(selectedValue, "Original", StringComparison.Ordinal);
        _host.FilenameDisplayCleanUp.IsChecked = string.Equals(selectedValue, "CleanUp", StringComparison.Ordinal);
        _host.FilenameDisplayNoFilename.IsChecked =
            string.Equals(selectedValue, "NoFilename", StringComparison.Ordinal);
    }

    /// <summary>
    ///     Updates the filename font size menu check marks to reflect the selected font size.
    /// </summary>
    public void UpdateFilenameFontSizeCheckMarks(string selectedValue)
    {
        _host.FilenameFontSizeSmall.IsChecked = string.Equals(selectedValue, "Small", StringComparison.Ordinal);
        _host.FilenameFontSizeNormal.IsChecked = string.Equals(selectedValue, "Normal", StringComparison.Ordinal);
        _host.FilenameFontSizeBig.IsChecked = string.Equals(selectedValue, "Big", StringComparison.Ordinal);
    }

    /// <summary>
    ///     Updates the machine name font size menu check marks to reflect the selected font size.
    /// </summary>
    public void UpdateMachineNameFontSizeCheckMarks(string selectedValue)
    {
        _host.MachineNameFontSizeSmall.IsChecked = string.Equals(selectedValue, "Small", StringComparison.Ordinal);
        _host.MachineNameFontSizeNormal.IsChecked = string.Equals(selectedValue, "Normal", StringComparison.Ordinal);
        _host.MachineNameFontSizeBig.IsChecked = string.Equals(selectedValue, "Big", StringComparison.Ordinal);
    }

    /// <summary>
    ///     Sets the view mode check marks to indicate whether list view or grid view is active.
    /// </summary>
    /// <param name="viewMode">The view mode to set ("ListView" or "GridView").</param>
    public void SetViewMode(string viewMode)
    {
        if (string.Equals(viewMode, "ListView", StringComparison.Ordinal))
        {
            _host.ListView.IsChecked = true;
            _host.GridView.IsChecked = false;
        }
        else
        {
            _host.GridView.IsChecked = true;
            _host.ListView.IsChecked = false;
        }
    }
}