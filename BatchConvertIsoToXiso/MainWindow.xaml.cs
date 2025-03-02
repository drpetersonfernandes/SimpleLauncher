using System.Windows;

namespace BatchConvertIsoToXiso
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow
    {
        private readonly MainViewModel _viewModel;

        public MainWindow()
        {
            // Register resources before initializing components
            Application.Current.Resources.Add("BooleanToVisibilityConverter", new BooleanToVisibilityConverter());

            InitializeComponent();

            // Create the view model and pass the log document
            _viewModel = new MainViewModel(LogViewer.Document);

            // Set the DataContext for data binding
            DataContext = _viewModel;
        }

        // Renamed to avoid name collision with the control
        public System.Windows.Controls.RichTextBox LogViewerControl => this.LogViewer;
    }
}