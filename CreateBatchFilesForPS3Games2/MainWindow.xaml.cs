using CreateBatchFilesForPS3Games2.ViewModels;

namespace CreateBatchFilesForPS3Games2
{
    public partial class MainWindow
    {
        private readonly MainViewModel _viewModel;

        public MainWindow(MainViewModel viewModel)
        {
            InitializeComponent();
            _viewModel = viewModel;
            DataContext = _viewModel;
        }
    }
}