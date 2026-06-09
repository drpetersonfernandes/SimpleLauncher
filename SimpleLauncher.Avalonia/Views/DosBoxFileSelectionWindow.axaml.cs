using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Markup.Xaml;
using SimpleLauncher.Core.ViewModels;

namespace SimpleLauncher.Avalonia.Views;

public partial class DosBoxFileSelectionWindow : Window
{
    private readonly DosBoxFileSelectionViewModel _viewModel;

    public DosBoxFileSelectionWindow(DosBoxFileSelectionViewModel viewModel)
    {
        InitializeComponent();

        _viewModel = viewModel;
        _viewModel.DialogResultRequested += result =>
        {
            Close(result == true);
        };

        DataContext = _viewModel;
    }

    public void Initialize(List<string> filePaths, string baseDirectory)
    {
        _viewModel.Initialize(filePaths, baseDirectory);
    }

    public string SelectedFilePath => _viewModel.SelectedFilePath;

    private void OnFileListBoxDoubleTapped(object? sender, TappedEventArgs e)
    {
        _viewModel.OnItemDoubleClicked();
    }

    private void InitializeComponent()
    {
        AvaloniaXamlLoader.Load(this);
    }
}
