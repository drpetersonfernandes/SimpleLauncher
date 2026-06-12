using System.Windows;
using SimpleLauncher.ViewModels;

namespace SimpleLauncher;

/// <summary>
/// Window for configuring gamepad dead zone sensitivity settings.
/// </summary>
public partial class SetGamepadDeadZoneWindow
{
    private readonly Action _saveCompletedHandler;

    /// <summary>
    /// Initializes a new instance of the <see cref="SetGamepadDeadZoneWindow"/> class.
    /// </summary>
    /// <param name="viewModel">The view model providing dead zone configuration logic.</param>
    public SetGamepadDeadZoneWindow(SetGamepadDeadZoneViewModel viewModel)
    {
        InitializeComponent();
        App.ApplyThemeToWindow(this);
        Owner = Application.Current.MainWindow;

        _saveCompletedHandler = () =>
        {
            if (IsLoaded) DialogResult = true;
            Close();
        };

        viewModel.SaveCompleted += _saveCompletedHandler;
        viewModel.CloseRequested += Close;

        Closing += (_, _) =>
        {
            viewModel.SaveCompleted -= _saveCompletedHandler;
            viewModel.CloseRequested -= Close;
        };

        DataContext = viewModel;
    }
}
