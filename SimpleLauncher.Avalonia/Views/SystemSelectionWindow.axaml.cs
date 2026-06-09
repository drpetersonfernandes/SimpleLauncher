using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using SimpleLauncher.Core.ViewModels;

namespace SimpleLauncher.Avalonia.Views;

public partial class SystemSelectionWindow : Window
{
    private readonly SystemSelectionViewModel _viewModel;

    public SystemSelectionWindow(SystemSelectionViewModel viewModel)
    {
        InitializeComponent();

        _viewModel = viewModel;
        _viewModel.DialogResultRequested += result =>
        {
            Close(result == true);
        };

        DataContext = _viewModel;
    }

    public void Initialize(string currentGuess)
    {
        _viewModel.Initialize(currentGuess);
    }

    public string SelectedSystem => _viewModel.SelectedSystem;

    private void InitializeComponent()
    {
        AvaloniaXamlLoader.Load(this);
    }
}
