using System.Windows.Documents;
using System.Windows.Navigation;
using SimpleLauncher.ViewModels;

namespace SimpleLauncher;

public partial class RomHistoryWindow
{
    private readonly RequestNavigateEventHandler _requestNavigateHandler;

    public RomHistoryWindow(RomHistoryViewModel viewModel)
    {
        InitializeComponent();
        App.ApplyThemeToWindow(this);

        _requestNavigateHandler = OnHyperlinkRequestNavigate;

        Loaded += (_, _) =>
        {
            HistoryMarkdownViewer.AddHandler(Hyperlink.RequestNavigateEvent, _requestNavigateHandler);
        };

        Closed += (_, _) =>
        {
            HistoryMarkdownViewer.RemoveHandler(Hyperlink.RequestNavigateEvent, _requestNavigateHandler);
        };

        Loaded += async (_, _) =>
        {
            try
            {
                await viewModel.LoadRomHistoryAsync();
            }
            catch (Exception ex)
            {
                Services.DebugAndBugReport.DebugLogger.Log($"Error loading ROM history: {ex.Message}");
            }
        };

        DataContext = viewModel;
    }

    public void Initialize(string romName, string systemName, string searchTerm)
    {
        ((RomHistoryViewModel)DataContext).Initialize(romName, systemName, searchTerm);
    }

    private static void OnHyperlinkRequestNavigate(object sender, RequestNavigateEventArgs e)
    {
        try
        {
            System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo
            {
                FileName = e.Uri.AbsoluteUri,
                UseShellExecute = true
            });
        }
        catch (Exception ex)
        {
            Services.DebugAndBugReport.DebugLogger.Log($"Failed to open link: {e.Uri} - {ex.Message}");
        }

        e.Handled = true;
    }
}