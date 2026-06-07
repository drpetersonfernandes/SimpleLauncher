using SimpleLauncher.Services.Pagination;

namespace SimpleLauncher;

public partial class MainWindow
{
    void IPaginationHost.SetPrevPageButtonEnabled(bool enabled)
    {
        PrevPageButton2.IsEnabled = enabled;
    }

    void IPaginationHost.SetNextPageButtonEnabled(bool enabled)
    {
        NextPageButton2.IsEnabled = enabled;
    }

    void IPaginationHost.ScrollToTop()
    {
        Scroller.ScrollToTop();
    }

    void IPaginationHost.UpdateTotalFilesLabel(string text)
    {
        TotalFilesLabel.Dispatcher.Invoke(() => TotalFilesLabel.Content = text);
    }

    void IPaginationHost.AddNoFilesMessage()
    {
        _gameListUiService.AddNoFilesMessage();
        _topLetterNumberMenu.DeselectLetter();
    }
}
