using SimpleLauncher.ViewModels;

namespace SimpleLauncher;

public partial class SetGamepadDeadZoneWindow
{
    public SetGamepadDeadZoneWindow(SetGamepadDeadZoneViewModel viewModel)
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