using System.Windows.Documents;
using System.Windows.Navigation;
using SimpleLauncher.Core.ViewModels;

namespace SimpleLauncher;

public partial class UpdateHistoryWindow
{
    private readonly UpdateHistoryViewModel _viewModel;

    public UpdateHistoryWindow(UpdateHistoryViewModel viewModel)
    {
        InitializeComponent();
        App.ApplyThemeToWindow(this);

        _viewModel = viewModel;

        DataContext = _viewModel;

        HistoryMarkdownViewer.AddHandler(Hyperlink.RequestNavigateEvent, new RequestNavigateEventHandler(OnHyperlinkRequestNavigate));
    }

    private void OnHyperlinkRequestNavigate(object sender, RequestNavigateEventArgs e)
    {
        _viewModel.OnHyperlinkRequestNavigate(e.Uri);
        e.Handled = true;
    }
}