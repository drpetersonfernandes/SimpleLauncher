using SharpDX.DirectInput;
using SharpDX.XInput;
using System;
using System.Threading;
using System.Windows;
using WindowsInput;

namespace SimpleLauncher;

public class GamePadController : IDisposable
{
    private static readonly Lazy<GamePadController> Instance = new(() => new GamePadController());
    public static GamePadController Instance2 => Instance.Value;

    // Add an Action for error logging
    public Action<Exception, string> ErrorLogger { get; set; }

    private const int RefreshRate = 60;
    private const float MaxThumbValue = 32767.0f;  // Maximum thumbstick value for normalization.

    private readonly Timer _timer;
    private Controller _xboxController;
    private Joystick _playStationController;
    private readonly IMouseSimulator _mouseSimulator;

    private bool _wasADown;
    private bool _wasBDown;
    private bool _isDisposed;

    readonly float _deadZoneX = 0.05f;
    readonly float _deadZoneY = 0.02f;

    public bool IsRunning { get; private set; }

    private GamePadController()
    {
        // Initialize Xbox Controller
        _xboxController = new Controller(UserIndex.One);

        // Initialize PlayStation Controller using DirectInput
        var directInput = new DirectInput();
        var devices = directInput.GetDevices(SharpDX.DirectInput.DeviceType.Gamepad, DeviceEnumerationFlags.AllDevices);
        if (devices.Count > 0)
        {
            _playStationController = new Joystick(directInput, devices[0].InstanceGuid);
            _playStationController.Acquire();
        }

        _mouseSimulator = new InputSimulator().Mouse;
        _timer = new Timer(_ => Update());
    }

    public void Start()
    {
        try
        {
            if (_isDisposed)
            {
                // Reinitialize Xbox controller only if null or disconnected
                if (_xboxController == null || !_xboxController.IsConnected)
                {
                    _xboxController = new Controller(UserIndex.One);
                }

                // Reinitialize PlayStation controller
                if (_playStationController == null)
                {
                    var directInput = new DirectInput();
                    var devices = directInput.GetDevices(SharpDX.DirectInput.DeviceType.Gamepad, DeviceEnumerationFlags.AllDevices);
                    if (devices.Count > 0)
                    {
                        _playStationController = new Joystick(directInput, devices[0].InstanceGuid);
                        _playStationController.Acquire();
                    }
                }

                _isDisposed = false;
            }

            _timer.Change(0, 1000 / RefreshRate);
            IsRunning = true;
        }
        catch (Exception ex)
        {
            // Notify developer
            ErrorLogger?.Invoke(ex, $"Error in GamePadController Start method.\n\n" +
                                    $"Exception type: {ex.GetType().Name}\n" +
                                    $"Exception details: {ex.Message}");

            // Notify user
            GamePadErrorMessageBox();
        }
    }

    public void Stop()
    {
        try
        {
            IsRunning = false;
            if (_timer != null && !_isDisposed)
                _timer.Change(Timeout.Infinite, Timeout.Infinite);
        }
        catch (Exception ex)
        {
            // Notify developer
            ErrorLogger?.Invoke(ex, $"Error in GamePadController Stop method.\n\n" +
                                    $"Exception type: {ex.GetType().Name}\n" +
                                    $"Exception details: {ex.Message}");

            // Notify user
            GamePadErrorMessageBox();
        }
    }

    public void Dispose()
    {
        try
        {
            if (_isDisposed) return;

            Stop();
            _timer?.Dispose();
            _playStationController?.Unacquire();
            _playStationController = null;

            _isDisposed = true;

            GC.SuppressFinalize(this);
        }
        catch (Exception ex)
        {
            // Notify developer
            ErrorLogger?.Invoke(ex, $"Error in GamePadController Dispose method.\n\n" +
                                    $"Exception type: {ex.GetType().Name}\n" +
                                    $"Exception details: {ex.Message}");

            // Notify user
            GamePadErrorMessageBox();
        }
    }
    
    private void Restart()
    {
        Stop();
        Dispose();
        Start();
    }

    private void Update()
    {
        try
        {
            if (_xboxController.IsConnected)
            {
                // Handle Xbox Controller Input
                _xboxController.GetState(out var state);
                HandleMovement(state);
                HandleScroll(state);
                HandleLeftButton(state);
                HandleRightButton(state);
            }
            else if (_playStationController != null)
            {
                // Handle PlayStation Controller Input
                var state = _playStationController.GetCurrentState();
                HandlePlayStationMovement(state);
                HandlePlayStationButtons(state);
            }
        }
        catch (Exception ex)
        {
            // Notify developer
            ErrorLogger?.Invoke(ex, $"Error in GamePadController Update method.\n\n" +
                                    $"Exception type: {ex.GetType().Name}\n" +
                                    $"Exception details: {ex.Message}");

            // Notify user
            GamePadErrorWithRestartMessageBox();

            // Restart GamePad instance
            Restart();
        }

        void GamePadErrorWithRestartMessageBox()
        {
            MessageBox.Show("There was an error with the GamePad Controller.\n\n" +
                            "Running 'Simple Launcher' with administrative access may fix this problem.\n\n" +
                            "'Simple Launcher' will restart the GamePad instance",
                "Error", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }
    
    private static void GamePadErrorMessageBox()
    {
        MessageBox.Show("There was an error with the GamePad Controller.\n\n" +
                        "Running 'Simple Launcher' with administrative access may fix this problem.\n\n" +
                        "The error was reported to the developer that will try to fix the issue.",
            "Error", MessageBoxButton.OK, MessageBoxImage.Error);
    }

    private void HandleRightButton(State state)
    {
        var isBDown = state.Gamepad.Buttons.HasFlag(GamepadButtonFlags.B);
        if (isBDown && !_wasBDown) _mouseSimulator.RightButtonDown();
        if (!isBDown && _wasBDown) _mouseSimulator.RightButtonUp();
        _wasBDown = isBDown;
    }

    private void HandleLeftButton(State state)
    {
        var isADown = state.Gamepad.Buttons.HasFlag(GamepadButtonFlags.A);
        if (isADown && !_wasADown) _mouseSimulator.LeftButtonDown();
        if (!isADown && _wasADown) _mouseSimulator.LeftButtonUp();
        _wasADown = isADown;
    }

    private void HandleScroll(State state)
    {
        var (x, y) = ProcessThumbStick(state.Gamepad.RightThumbX, state.Gamepad.RightThumbY, _deadZoneX, _deadZoneY);
        _mouseSimulator.HorizontalScroll((int)x);
        _mouseSimulator.VerticalScroll((int)y);
    }

    private void HandleMovement(State state)
    {
        var (x, y) = ProcessThumbStick(state.Gamepad.LeftThumbX, state.Gamepad.LeftThumbY, _deadZoneX, _deadZoneY);
        _mouseSimulator.MoveMouseBy((int)x, -(int)y);
    }
    
    private void HandlePlayStationMovement(JoystickState state)
    {
        // Map PlayStation thumbstick inputs
        var x = state.X / MaxThumbValue;
        var y = state.Y / MaxThumbValue;
        _mouseSimulator.MoveMouseBy((int)x, -(int)y);
    }
    
    private void HandlePlayStationButtons(JoystickState state)
    {
        // Map PlayStation buttons
        if (state.Buttons[0]) _mouseSimulator.LeftButtonDown();  // Cross Button
        if (!state.Buttons[0]) _mouseSimulator.LeftButtonUp();
        if (state.Buttons[1]) _mouseSimulator.RightButtonDown(); // Circle Button
        if (!state.Buttons[1]) _mouseSimulator.RightButtonUp();
    }

    private static (float, float) ProcessThumbStick(short thumbX, short thumbY, float dzX, float dzY)
    {
        float normalizedX = Math.Max(-1, thumbX / MaxThumbValue);
        float normalizedY = Math.Max(-1, thumbY / MaxThumbValue);

        float resultX = (Math.Abs(normalizedX) < dzX ? 0 : (Math.Abs(normalizedX) - dzX) * (normalizedX / Math.Abs(normalizedX)));
        float resultY = (Math.Abs(normalizedY) < dzY ? 0 : (Math.Abs(normalizedY) - dzY) * (normalizedY / Math.Abs(normalizedY)));

        if (dzX > 0) resultX *= 10 / (1 - dzX);
        if (dzY > 0) resultY *= 10 / (1 - dzY);

        return (resultX, resultY);
    }
}