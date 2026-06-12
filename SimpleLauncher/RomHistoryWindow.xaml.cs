using System.Windows.Documents;
using System.Windows.Navigation;
using SimpleLauncher.ViewModels;

namespace SimpleLauncher;

using Interfaces;

/// <summary>
/// Window displaying ROM history information with hyperlinked references.
/// </summary>
public partial class RomHistoryWindow
{
    private readonly RequestNavigateEventHandler _requestNavigateHandler;
    private readonly IDebugLogger _debugLogger;

    /// <summary>
    /// Initializes a new instance of the <see cref="RomHistoryWindow"/> class.
    /// </summary>
    /// <param name="viewModel">The view model providing ROM history data.</param>
    /// <param name="debugLogger">The debug logger.</param>
    public RomHistoryWindow(RomHistoryViewModel viewModel, IDebugLogger debugLogger)
    {
        InitializeComponent();
        App.ApplyThemeToWindow(this);

        _debugLogger = debugLogger ?? throw new ArgumentNullException(nameof(debugLogger));
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
                _debugLogger.Log($"Error loading ROM history: {ex.Message}");
            }
        };

        DataContext = viewModel;
    }

    /// <summary>
    /// Initializes the window with ROM and system information for history lookup.
    /// </summary>
    /// <param name="romName">The name of the ROM.</param>
    /// <param name="systemName">The name of the system.</param>
    /// <param name="searchTerm">The search term for filtering history.</param>
    public void Initialize(string romName, string systemName, string searchTerm)
    {
        ((RomHistoryViewModel)DataContext).Initialize(romName, systemName, searchTerm);
    }

    private void OnHyperlinkRequestNavigate(object sender, RequestNavigateEventArgs e)
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
            _debugLogger.Log($"Failed to open link: {e.Uri} - {ex.Message}");
        }

        e.Handled = true;
    }
}
