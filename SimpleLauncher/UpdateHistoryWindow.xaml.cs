using System.Windows.Documents;
using System.Windows.Navigation;
using Microsoft.Extensions.DependencyInjection;
using SimpleLauncher.Services.DebugAndBugReport;
using SimpleLauncher.ViewModels;

namespace SimpleLauncher;

public partial class UpdateHistoryWindow
{
    private readonly UpdateHistoryViewModel _viewModel;

    public UpdateHistoryWindow()
    {
        InitializeComponent();
        App.ApplyThemeToWindow(this);

        var logErrors = App.ServiceProvider.GetRequiredService<ILogErrors>();
        _viewModel = new UpdateHistoryViewModel(logErrors);

        DataContext = _viewModel;

        HistoryMarkdownViewer.AddHandler(Hyperlink.RequestNavigateEvent, new RequestNavigateEventHandler(OnHyperlinkRequestNavigate));
    }

    private void OnHyperlinkRequestNavigate(object sender, RequestNavigateEventArgs e)
    {
        _viewModel.OnHyperlinkRequestNavigate(e.Uri);
        e.Handled = true;
    }
}