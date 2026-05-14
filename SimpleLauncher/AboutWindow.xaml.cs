using SimpleLauncher.ViewModels;

namespace SimpleLauncher;

public partial class AboutWindow
{
    public AboutWindow()
    {
        InitializeComponent();
        App.ApplyThemeToWindow(this);

        var viewModel = new AboutViewModel();
        viewModel.CloseRequested += Close;
        viewModel.OpenUpdateHistoryRequested += static () =>
        {
            var updateHistoryWindow = new UpdateHistoryWindow();
            updateHistoryWindow.ShowDialog();
        };
        viewModel.GetOwnerWindow += () => this;

        DataContext = viewModel;
    }
}
