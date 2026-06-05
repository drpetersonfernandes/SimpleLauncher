using SimpleLauncher.ViewModels;

namespace SimpleLauncher;

public partial class SetLinksWindow
{
    public SetLinksWindow(SetLinksViewModel viewModel)
    {
        InitializeComponent();
        App.ApplyThemeToWindow(this);

        viewModel.SaveCompleted += () =>
        {
            DialogResult = true;
            Close();
        };
        viewModel.CloseRequested += Close;

        DataContext = viewModel;
    }
}