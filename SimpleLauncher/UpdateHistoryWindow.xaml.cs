using System.Windows;
using System.Windows.Documents;
using System.Windows.Navigation;
using SimpleLauncher.Services.DebugAndBugReport;
using SimpleLauncher.ViewModels;

namespace SimpleLauncher;

public partial class UpdateHistoryWindow
{
    private readonly UpdateHistoryViewModel _viewModel;
    private readonly IDebugLogger _debugLogger;

    public UpdateHistoryWindow(UpdateHistoryViewModel viewModel, IDebugLogger debugLogger)
    {
        InitializeComponent();
        App.ApplyThemeToWindow(this);

        _viewModel = viewModel;
        _debugLogger = debugLogger ?? throw new ArgumentNullException(nameof(debugLogger));

        DataContext = _viewModel;

        HistoryMarkdownViewer.AddHandler(Hyperlink.RequestNavigateEvent, new RequestNavigateEventHandler(OnHyperlinkRequestNavigate));
        Loaded += UpdateHistoryWindow_Loaded;
        Closed += UpdateHistoryWindow_Closed;
    }

    private async void UpdateHistoryWindow_Loaded(object sender, RoutedEventArgs e)
    {
        try
        {
            await _viewModel.InitializeAsync();
        }
        catch (Exception ex)
        {
            _debugLogger.LogException(ex, "Error initializing UpdateHistoryWindow.");
        }
    }

    private void UpdateHistoryWindow_Closed(object sender, EventArgs e)
    {
        HistoryMarkdownViewer.RemoveHandler(Hyperlink.RequestNavigateEvent, new RequestNavigateEventHandler(OnHyperlinkRequestNavigate));
    }

    private void OnHyperlinkRequestNavigate(object sender, RequestNavigateEventArgs e)
    {
        _viewModel.OnHyperlinkRequestNavigate(e.Uri);
        e.Handled = true;
    }
}