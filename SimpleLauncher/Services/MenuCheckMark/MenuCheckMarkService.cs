namespace SimpleLauncher.Services.MenuCheckMark;

public class MenuCheckMarkService : IMenuCheckMarkService
{
    private IMenuCheckMarkHost _host;

    public void Initialize(IMenuCheckMarkHost host)
    {
        _host = host;
    }

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

    public void UpdateShowGamesCheckMarks(string selectedValue)
    {
        _host.ShowAll.IsChecked = selectedValue == "ShowAll";
        _host.ShowWithCover.IsChecked = selectedValue == "ShowWithCover";
        _host.ShowWithoutCover.IsChecked = selectedValue == "ShowWithoutCover";
    }

    public void UpdateButtonAspectRatioCheckMarks(string selectedValue)
    {
        _host.Square.IsChecked = selectedValue == "Square";
        _host.Wider.IsChecked = selectedValue == "Wider";
        _host.SuperWider.IsChecked = selectedValue == "SuperWider";
        _host.SuperWider2.IsChecked = selectedValue == "SuperWider2";
        _host.Taller.IsChecked = selectedValue == "Taller";
        _host.SuperTaller.IsChecked = selectedValue == "SuperTaller";
        _host.SuperTaller2.IsChecked = selectedValue == "SuperTaller2";
    }

    public void UpdateFilenameDisplayModeCheckMarks(string selectedValue)
    {
        _host.FilenameDisplayOriginal.IsChecked = selectedValue == "Original";
        _host.FilenameDisplayCleanUp.IsChecked = selectedValue == "CleanUp";
        _host.FilenameDisplayNoFilename.IsChecked = selectedValue == "NoFilename";
    }

    public void UpdateFilenameFontSizeCheckMarks(string selectedValue)
    {
        _host.FilenameFontSizeSmall.IsChecked = selectedValue == "Small";
        _host.FilenameFontSizeNormal.IsChecked = selectedValue == "Normal";
        _host.FilenameFontSizeBig.IsChecked = selectedValue == "Big";
    }

    public void UpdateMachineNameFontSizeCheckMarks(string selectedValue)
    {
        _host.MachineNameFontSizeSmall.IsChecked = selectedValue == "Small";
        _host.MachineNameFontSizeNormal.IsChecked = selectedValue == "Normal";
        _host.MachineNameFontSizeBig.IsChecked = selectedValue == "Big";
    }

    public void SetViewMode(string viewMode)
    {
        if (viewMode == "ListView")
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
