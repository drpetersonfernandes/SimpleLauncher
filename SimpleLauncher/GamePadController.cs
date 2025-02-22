using SharpDX.DirectInput;
using SharpDX.XInput;
using System;
using System.IO;
using System.Threading;
using WindowsInput;
using DeviceType = SharpDX.DirectInput.DeviceType;

namespace SimpleLauncher;

public class GamePadController : IDisposable
{
    private static readonly string LogPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "error_user.log");

    private static readonly Lazy<GamePadController> Instance = new(() => new GamePadController());
    public static GamePadController Instance2 => Instance.Value;

    // Add an Action for error logging
    public Action<Exception, string> ErrorLogger { get; set; }

    private const int RefreshRate = 60;

    // Normalize XInput values
    private const float MaxThumbValue = 32767.0f;

    private readonly Timer _timer;
    private Controller _xinputController;
    private Joystick _directInputController;
    private readonly IMouseSimulator _mouseSimulator;

    // For XInput
    private bool _wasADown;
    private bool _wasBDown;

    // For DirectInput
    private bool _wasCrossDown;
    private bool _wasCircleDown;

    // To Dispose GamePad Instance
    private bool _isDisposed;

    // DeadZone settings
    // private const float DeadZoneX = 0.05f;
    // private const float DeadZoneY = 0.02f;

    // Add public properties with default values:
    public float DeadZoneX { get; set; } = 0.05f;
    public float DeadZoneY { get; set; } = 0.02f;

    public bool IsRunning { get; private set; }

    // Handle DirectInput reconnection
    private Guid _playStationControllerGuid; // Store the GUID of the connected PlayStation controller
    private DateTime _lastReconnectAttempt = DateTime.MinValue; // Track the last reconnection attempt
    private const int ReconnectDelayMilliseconds = 5000; // Delay between reconnection attempts

    private GamePadController()
    {
        // Initialize Xbox Controller using XInput
        _xinputController = new Controller(UserIndex.One);

        // Initialize PlayStation Controller using DirectInput
        var directInput = new DirectInput();
        var devices = directInput.GetDevices(DeviceType.Gamepad, DeviceEnumerationFlags.AllDevices);
        if (devices.Count > 0)
        {
            _directInputController = new Joystick(directInput, devices[0].InstanceGuid);
            _directInputController.Acquire();
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
                // Reinitialize Xbox controller
                _xinputController = new Controller(UserIndex.One);

                // Reinitialize PlayStation controller
                var directInput = new DirectInput();
                var devices = directInput.GetDevices(DeviceType.Gamepad, DeviceEnumerationFlags.AllDevices);
                if (devices.Count > 0)
                {
                    _directInputController = new Joystick(directInput, devices[0].InstanceGuid);
                    _directInputController.Acquire();
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
            MessageBoxLibrary.GamePadErrorMessageBox(LogPath);
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
            MessageBoxLibrary.GamePadErrorMessageBox(LogPath);
        }
    }

    public void Dispose()
    {
        try
        {
            if (_isDisposed) return;

            Stop();
            _timer?.Dispose();
            _directInputController?.Unacquire();
            _directInputController = null;

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
            MessageBoxLibrary.GamePadErrorMessageBox(LogPath);
        }
    }

    private void Update()
    {
        try
        {
            if (!_xinputController.IsConnected && _directInputController == null)
            {
                // Attempt to reconnect controllers if not already attempting
                if (!((DateTime.Now - _lastReconnectAttempt).TotalMilliseconds > ReconnectDelayMilliseconds)) return;

                CheckAndReconnectControllers();
                _lastReconnectAttempt = DateTime.Now;
                return; // Exit Update to avoid further processing until next cycle.
            }

            if (_xinputController.IsConnected)
            {
                // Handle Xbox Controller Input
                _xinputController.GetState(out var state);
                HandleXInputMovement(state);
                HandleXInputScroll(state);
                HandleXInputLeftButton(state);
                HandleXInputRightButton(state);
            }
            else if (_directInputController != null)
            {
                // Handle PlayStation Controller Input
                var state = _directInputController.GetCurrentState();
                HandleDirectInputMovement(state);
                HandleDirectInputButtons(state);
                HandleDirectInputScroll(state);
            }
        }
        catch (SharpDX.SharpDXException ex) when (ex.HResult == unchecked((int)0x8007001E)) // DIERR_INPUTLOST
        {
            // Ignore this specific error (DIERR_INPUTLOST)
            // No need to log or notify the developer
            CheckAndReconnectControllers(); // Attempt to reconnect the controller
        }
        catch (NullReferenceException)
        {
            // Ignore NullReferenceException
            // No need to log or notify the developer
            CheckAndReconnectControllers(); // Attempt to reconnect the controller
        }
        catch (SharpDX.SharpDXException ex) when (ex.HResult == unchecked((int)0x8007000C)) // DIERR_NOTACQUIRED
        {
            // Ignore this specific error (DIERR_NOTACQUIRED)
            // No need to log or notify the developer
            CheckAndReconnectControllers(); // Attempt to reconnect the controller
        }
        catch (Exception ex)
        {
            // Notify developer
            ErrorLogger?.Invoke(ex, $"Error in GamePadController Update method.\n\n" +
                                    $"Exception type: {ex.GetType().Name}\n" +
                                    $"Exception details: {ex.Message}");

            CheckAndReconnectControllers();

            // Notify user
            MessageBoxLibrary.GamePadErrorMessageBox(LogPath);
        }
    }

    public void CheckAndReconnectControllers()
    {
        try
        {
            if (!Instance2.IsRunning || _isDisposed) return;

            // Reinitialize Xbox controller
            _xinputController = new Controller(UserIndex.One);

            // Check if the PlayStation controller is connected
            var directInput = new DirectInput();
            var devices = directInput.GetDevices(DeviceType.Gamepad, DeviceEnumerationFlags.AttachedOnly);

            var found = false;
            foreach (var deviceInstance in devices)
            {
                // Check if the device matches the previously connected controller's GUID
                if (_playStationControllerGuid == Guid.Empty || deviceInstance.InstanceGuid == _playStationControllerGuid)
                {
                    _directInputController?.Unacquire();
                    _directInputController?.Dispose();

                    _directInputController = new Joystick(directInput, deviceInstance.InstanceGuid);
                    _directInputController.Acquire();
                    _playStationControllerGuid = deviceInstance.InstanceGuid; // Update the GUID
                    found = true;

                    break;
                }
            }

            if (!found)
            {
                _directInputController = null;
                _playStationControllerGuid = Guid.Empty; // Reset the GUID if the device is not found
            }
        }
        catch (Exception ex)
        {
            // Notify developer
            ErrorLogger?.Invoke(ex, $"Error reconnecting controllers. User was not notified.\n\n" +
                                    $"Exception type: {ex.GetType().Name}\n" +
                                    $"Exception details: {ex.Message}");

            // Notify user
            // Ignore
        }
    }

    private void HandleXInputRightButton(State state)
    {
        var isBDown = state.Gamepad.Buttons.HasFlag(GamepadButtonFlags.B);
        if (isBDown && !_wasBDown) _mouseSimulator.RightButtonDown();
        if (!isBDown && _wasBDown) _mouseSimulator.RightButtonUp();
        _wasBDown = isBDown;
    }

    private void HandleXInputLeftButton(State state)
    {
        var isADown = state.Gamepad.Buttons.HasFlag(GamepadButtonFlags.A);
        if (isADown && !_wasADown) _mouseSimulator.LeftButtonDown();
        if (!isADown && _wasADown) _mouseSimulator.LeftButtonUp();
        _wasADown = isADown;
    }

    private void HandleXInputMovement(State state)
    {
        var (x, y) = ProcessThumbStickXInput(state.Gamepad.LeftThumbX, state.Gamepad.LeftThumbY, DeadZoneX, DeadZoneY);
        _mouseSimulator.MoveMouseBy((int)x, -(int)y);
    }

    private void HandleXInputScroll(State state)
    {
        var (x, y) = ProcessThumbStickXInput(state.Gamepad.RightThumbX, state.Gamepad.RightThumbY, DeadZoneX, DeadZoneY);
        _mouseSimulator.HorizontalScroll((int)x);
        _mouseSimulator.VerticalScroll((int)y);
    }

    private static (float, float) ProcessThumbStickXInput(short thumbX, short thumbY, float dzX, float dzY)
    {
        var normalizedX = Math.Max(-1, thumbX / MaxThumbValue);
        var normalizedY = Math.Max(-1, thumbY / MaxThumbValue);

        var resultX = (Math.Abs(normalizedX) < dzX ? 0 : (Math.Abs(normalizedX) - dzX) * (normalizedX / Math.Abs(normalizedX)));
        var resultY = (Math.Abs(normalizedY) < dzY ? 0 : (Math.Abs(normalizedY) - dzY) * (normalizedY / Math.Abs(normalizedY)));

        if (dzX > 0) resultX *= 10 / (1 - dzX);
        if (dzY > 0) resultY *= 10 / (1 - dzY);

        return (resultX, resultY);
    }

    private void HandleDirectInputButtons(JoystickState state)
    {
        var isCrossDown = state.Buttons[1];
        if (isCrossDown && !_wasCrossDown) _mouseSimulator.LeftButtonDown(); // Cross Button
        if (!isCrossDown && _wasCrossDown) _mouseSimulator.LeftButtonUp();
        _wasCrossDown = isCrossDown;

        var isCircleDown = state.Buttons[2];
        if (isCircleDown && !_wasCircleDown) _mouseSimulator.RightButtonDown(); // Circle Button
        if (!isCircleDown && _wasCircleDown) _mouseSimulator.RightButtonUp();
        _wasCircleDown = isCircleDown;
    }

    private void HandleDirectInputMovement(JoystickState state)
    {
        // Normalize DirectInput values from [0, 65535] to [-32767, 32767]
        var thumbX = (short)(state.X - 32767); // Convert absolute to relative
        var thumbY = (short)(state.Y - 32767); // Convert absolute to relative

        // Invert the X and Y-axis (DirectInput typically has an inverted axis)
        thumbY = (short)-thumbY;
        thumbX = (short)-thumbX;

        // Process the thumbstick values with the dead zone
        var (x, y) = ProcessLeftThumbStickDirectInput(thumbX, thumbY, DeadZoneX, DeadZoneY);

        // Move the mouse based on processed values
        _mouseSimulator.MoveMouseBy(-(int)x, -(int)y);
    }

    private void HandleDirectInputScroll(JoystickState state)
    {
        var thumbX = (short)(state.RotationZ - 32767); // Horizontal axis
        var thumbY = (short)-(state.RotationZ - 32767); // Inverted Y

        var (x, y) = ProcessRightThumbStickDirectInput(thumbX, thumbY, DeadZoneX, DeadZoneY); // Use same processing

        _mouseSimulator.HorizontalScroll((int)x); // Assuming this is correct for your controller
        _mouseSimulator.VerticalScroll((int)y);
    }

    private static (float, float) ProcessLeftThumbStickDirectInput(short thumbX, short thumbY, float dzX, float dzY)
    {
        // Normalize the thumbstick values to the range [-1, 1]
        var normalizedX = thumbX / MaxThumbValue;
        var normalizedY = thumbY / MaxThumbValue;

        // Apply the dead zone for X
        float resultX = 0;
        if (Math.Abs(normalizedX) > dzX)
        {
            resultX = (Math.Abs(normalizedX) - dzX) * (normalizedX / Math.Abs(normalizedX));
        }

        // Apply the dead zone for Y
        float resultY = 0;
        if (Math.Abs(normalizedY) > dzY)
        {
            resultY = (Math.Abs(normalizedY) - dzY) * (normalizedY / Math.Abs(normalizedY));
        }

        // Scale the values after dead zone adjustment
        if (dzX > 0) resultX *= 7 / (1 - dzX);
        if (dzY > 0) resultY *= 7 / (1 - dzY);

        return (resultX, resultY);
    }

    private static (float, float) ProcessRightThumbStickDirectInput(short thumbX, short thumbY, float dzX, float dzY)
    {
        // Normalize the thumbstick values to the range [-1, 1]
        var normalizedX = thumbX / MaxThumbValue;
        var normalizedY = thumbY / MaxThumbValue;

        // Apply the dead zone for X
        float resultX = 0;
        if (Math.Abs(normalizedX) > dzX)
        {
            resultX = (Math.Abs(normalizedX) - dzX) * (normalizedX / Math.Abs(normalizedX));
        }

        // Apply the dead zone for Y
        float resultY = 0;
        if (Math.Abs(normalizedY) > dzY)
        {
            resultY = (Math.Abs(normalizedY) - dzY) * (normalizedY / Math.Abs(normalizedY));
        }

        // Scale the values after dead zone adjustment
        if (dzX > 0) resultX *= 1 / (1 - dzX);
        if (dzY > 0) resultY *= 1 / (1 - dzY);

        return (resultX, resultY);
    }
}