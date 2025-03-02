using CreateBatchFilesForPS3Games.ViewModels;

namespace CreateBatchFilesForPS3Games
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