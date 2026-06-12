using SimpleLauncher.ViewModels;

namespace SimpleLauncher;

/// <summary>
/// Window for configuring gamepad dead zone sensitivity settings.
/// </summary>
public partial class SetGamepadDeadZoneWindow
{
    /// <summary>
    /// Initializes a new instance of the <see cref="SetGamepadDeadZoneWindow"/> class.
    /// </summary>
    /// <param name="viewModel">The view model providing dead zone configuration logic.</param>
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
