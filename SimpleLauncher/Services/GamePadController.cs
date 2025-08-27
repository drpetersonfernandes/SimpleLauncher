using System;
using System.Threading;
using System.Windows;
using SharpDX;
using SharpDX.DirectInput;
using SharpDX.XInput;
using WindowsInput;
using DeviceType = SharpDX.DirectInput.DeviceType;

namespace SimpleLauncher.Services;

public class GamePadController : IDisposable
{
    private static readonly string LogPath = GetLogPath.Path();

    private static readonly Lazy<GamePadController> Instance = new(static () => new GamePadController());
    public static GamePadController Instance2 => Instance.Value;

    private readonly SemaphoreSlim _updateLock = new(1, 1);

    // Add a lock object for synchronizing Start/Stop/Dispose operations
    private readonly Lock _stateLock = new();

    // Add an Action for error logging
    public Action<Exception, string> ErrorLogger { get; set; }

    private const int RefreshRate = 60;

    // Normalize XInput values
    private const float MaxThumbValue = 32767.0f;

    private readonly Timer _timer;
    private Controller _xinputController;
    private Joystick _directInputController;
    private readonly IMouseSimulator _mouseSimulator;

    // DirectInput object needs to be managed for its lifetime
    private DirectInput _directInput;

    // For XInput
    private bool _wasADown;
    private bool _wasBDown;

    // For DirectInput
    private bool _wasCrossDown;
    private bool _wasCircleDown;

    // To Dispose GamePad Instance
    private bool _isDisposed;

    // DeadZone settings
    public float DeadZoneX { get; set; } = 0.05f;
    public float DeadZoneY { get; set; } = 0.02f;

    public bool IsRunning { get; private set; }

    // Handle DirectInput reconnection
    private Guid _playStationControllerGuid; // Store the GUID of the connected PlayStation controller
    private DateTime _lastReconnectAttempt = DateTime.MinValue; // Track the last reconnection attempt
    private const int ReconnectDelayMilliseconds = 5000; // Delay between reconnection attempts

    private const int XInputScalingFactor = 7;
    private const int DirectInputLeftThumbStickScalingFactor = 7;
    private const int DirectInputRightThumbStickScalingFactor = 1;

    private GamePadController()
    {
        // Initialize Xbox Controller using XInput
        _xinputController = new Controller(UserIndex.One);

        // Initialize DirectInput object once
        _directInput = new DirectInput();

        // Initialize PlayStation Controller using DirectInput (find the first gamepad)
        try
        {
            var devices = _directInput.GetDevices(DeviceType.Gamepad, DeviceEnumerationFlags.AllDevices);
            if (devices.Count > 0)
            {
                _directInputController = new Joystick(_directInput, devices[0].InstanceGuid);
                _directInputController.Acquire();
                _playStationControllerGuid = devices[0].InstanceGuid; // Store the GUID
            }
        }
        catch (Exception ex)
        {
            // Log initialization errors but allow the application to continue
            // as XInput might still work or the controller might be connected later.
            // Notify developer
            ErrorLogger?.Invoke(ex, $"Error during initial DirectInput controller setup.\n\n" +
                                    $"Exception type: {ex.GetType().Name}\n" +
                                    $"Exception details: {ex.Message}");

            _directInputController = null; // Ensure it's null if setup failed
            _playStationControllerGuid = Guid.Empty;
        }

        _mouseSimulator = new InputSimulator().Mouse;
        _timer = new Timer(_ => Update(), null, Timeout.Infinite, Timeout.Infinite);
    }

    public void Start()
    {
        try
        {
            lock (_stateLock)
            {
                if (_isDisposed)
                {
                    // Reinitialize Xbox controller
                    _xinputController = new Controller(UserIndex.One);

                    // Reinitialize DirectInput object
                    _directInput?.Dispose(); // Dispose the old one if it exists
                    _directInput = new DirectInput();

                    // Reinitialize PlayStation controller (find the first gamepad)
                    try
                    {
                        var devices = _directInput.GetDevices(DeviceType.Gamepad, DeviceEnumerationFlags.AllDevices);
                        if (devices.Count > 0)
                        {
                            _directInputController?.Unacquire();
                            _directInputController?.Dispose();
                            _directInputController = new Joystick(_directInput, devices[0].InstanceGuid);
                            _directInputController.Acquire();
                            _playStationControllerGuid = devices[0].InstanceGuid; // Store the GUID
                        }
                        else
                        {
                            _directInputController?.Unacquire();
                            _directInputController?.Dispose();
                            _directInputController = null;
                            _playStationControllerGuid = Guid.Empty;
                        }
                    }
                    catch (Exception ex)
                    {
                        // Notify developer
                        ErrorLogger?.Invoke(ex, $"Error during DirectInput controller reinitialization in Start.\n\n" +
                                                $"Exception type: {ex.GetType().Name}\n" +
                                                $"Exception details: {ex.Message}");

                        _directInputController?.Unacquire();
                        _directInputController?.Dispose();
                        _directInputController = null; // Ensure it's null if setup failed
                        _playStationControllerGuid = Guid.Empty;
                    }

                    _isDisposed = false;
                }

                _timer.Change(0, 1000 / RefreshRate);
                IsRunning = true;
            }
        }
        catch (Exception ex)
        {
            // Notify developer
            ErrorLogger?.Invoke(ex, $"Error in GamePadController Start method.\n\n" +
                                    $"Exception type: {ex.GetType().Name}\n" +
                                    $"Exception details: {ex.Message}");

            // Notify user
            Application.Current.Dispatcher.Invoke(static () => MessageBoxLibrary.GamePadErrorMessageBox(LogPath));
        }
    }

    public void Stop()
    {
        try
        {
            lock (_stateLock)
            {
                IsRunning = false;
                // Change timer to stop, but don't dispose it here
                _timer?.Change(Timeout.Infinite, Timeout.Infinite);
            }
        }
        catch (Exception ex)
        {
            // Notify developer
            ErrorLogger?.Invoke(ex, $"Error in GamePadController Stop method.\n\n" +
                                    $"Exception type: {ex.GetType().Name}\n" +
                                    $"Exception details: {ex.Message}");

            // Notify user
            Application.Current.Dispatcher.Invoke(static () => MessageBoxLibrary.GamePadErrorMessageBox(LogPath));
        }
    }

    public void Dispose()
    {
        try
        {
            lock (_stateLock)
            {
                if (_isDisposed) return;

                Stop(); // Stop the timer first
                _timer?.Dispose(); // Dispose the timer
                _directInputController?.Unacquire();
                _directInputController?.Dispose(); // Dispose the joystick
                _directInputController = null;

                _directInput?.Dispose(); // Dispose the DirectInput object
                _directInput = null;

                _isDisposed = true;
            }

            // Tell GC not to call the finalizer since we've already cleaned up
            GC.SuppressFinalize(this);
        }
        catch (Exception ex)
        {
            // Notify developer
            ErrorLogger?.Invoke(ex, $"Error in GamePadController Dispose method.\n\n" +
                                    $"Exception type: {ex.GetType().Name}\n" +
                                    $"Exception details: {ex.Message}");

            // Notify user
            Application.Current.Dispatcher.Invoke(static () => MessageBoxLibrary.GamePadErrorMessageBox(LogPath));
        }
    }

    private async void Update()
    {
        try
        {
            // Skip if another update is in progress
            if (!await _updateLock.WaitAsync(0)) return;

            try
            {
                // *** FIX START: Added lock to prevent race conditions with Dispose/Start/Stop ***
                lock (_stateLock)
                {
                    // Check if disposed or not running before processing
                    if (_isDisposed || !IsRunning)
                    {
                        // Ensure timer is stopped if somehow Update is called while not running/disposed
                        _timer?.Change(Timeout.Infinite, Timeout.Infinite);
                        return;
                    }

                    try
                    {
                        // Prioritize XInput if connected
                        if (_xinputController.IsConnected)
                        {
                            try
                            {
                                // Handle Xbox Controller Input
                                _xinputController.GetState(out var state);
                                HandleXInputMovement(state);
                                HandleXInputScroll(state);
                                HandleXInputLeftButton(state);
                                HandleXInputRightButton(state);

                                // If XInput is connected, ensure DirectInput controller is released
                                if (_directInputController != null)
                                {
                                    _directInputController.Unacquire();
                                    _directInputController.Dispose(); // Dispose the old one
                                    _directInputController = null;
                                    _playStationControllerGuid = Guid.Empty;
                                }
                            }
                            catch (SharpDXException)
                            {
                                // Controller likely disconnected between the IsConnected check and GetState.
                                // The main loop's reconnection logic will handle it on the next tick.
                            }
                        }
                        // If XInput is not connected, try DirectInput
                        else if (_directInputController is { IsDisposed: false }) // Check if DirectInput controller exists and is not disposed
                        {
                            try
                            {
                                // Handle PlayStation Controller Input
                                var state = _directInputController.GetCurrentState();
                                HandleDirectInputMovement(state);
                                HandleDirectInputButtons(state);
                                HandleDirectInputScroll(state);
                            }
                            catch (SharpDXException ex) when (ex.HResult == unchecked((int)0x8007001E)) // DIERR_INPUTLOST
                            {
                                // DirectInput device lost, attempt reconnection
                                // Notify developer
                                // ErrorLogger?.Invoke(ex, "DirectInput device lost (DIERR_INPUTLOST). Attempting reconnection.");
                                _directInputController?.Unacquire();
                                _directInputController?.Dispose();
                                _directInputController = null;
                                _playStationControllerGuid = Guid.Empty;
                                CheckAndReconnectControllers(); // Attempt reconnection immediately
                            }
                            catch (SharpDXException ex) when (ex.HResult == unchecked((int)0x8007000C)) // DIERR_NOTACQUIRED
                            {
                                // DirectInput device not acquired, attempt re-acquisition or reconnection
                                // Notify developer
                                ErrorLogger?.Invoke(ex, "DirectInput device not acquired (DIERR_NOTACQUIRED). Attempting re-acquisition/reconnection.");
                                try
                                {
                                    _directInputController?.Acquire(); // Try acquiring again
                                }
                                catch (Exception acquireEx)
                                {
                                    // Notify developer
                                    ErrorLogger?.Invoke(acquireEx, "Failed to re-acquire DirectInput device. Attempting full reconnection.");

                                    _directInputController?.Unacquire();
                                    _directInputController?.Dispose();
                                    _directInputController = null;
                                    _playStationControllerGuid = Guid.Empty;
                                    CheckAndReconnectControllers(); // Attempt full reconnection
                                }
                            }
                            catch (Exception)
                            {
                                // Catch any other exceptions during DirectInput processing
                                // Notify developer
                                // ErrorLogger?.Invoke(ex, $"Unexpected error during DirectInput processing. Attempting reconnection.\n\n" +
                                //                         $"Exception type: {ex.GetType().Name}\n" +
                                //                         $"Exception details: {ex.Message}");

                                _directInputController?.Unacquire();
                                _directInputController?.Dispose();
                                _directInputController = null;
                                _playStationControllerGuid = Guid.Empty;
                                CheckAndReconnectControllers(); // Attempt reconnection
                            }
                        }
                        // If neither XInput nor DirectInput controller is active, attempt reconnection after delay
                        else
                        {
                            if (!((DateTime.Now - _lastReconnectAttempt).TotalMilliseconds > ReconnectDelayMilliseconds)) return;

                            CheckAndReconnectControllers();
                            _lastReconnectAttempt = DateTime.Now;
                        }
                    }
                    catch (Exception ex)
                    {
                        // This catch block handles exceptions from the outer logic of Update,
                        // like checking _xinputController.IsConnected if _xinputController somehow became null (unlikely with current init).
                        // Or exceptions from the reconnection logic itself if it's called directly here.
                        // The specific NRE caught and ignored previously is now handled more granularly
                        // or should be prevented by better DirectInput management.
                        // Notify developer
                        ErrorLogger?.Invoke(ex, $"Unexpected error in GamePadController Update loop.\n\n" +
                                                $"Exception type: {ex.GetType().Name}\n" +
                                                $"Exception details: {ex.Message}");

                        // Notify user
                        Application.Current.Dispatcher.Invoke(static () => MessageBoxLibrary.GamePadErrorMessageBox(LogPath));

                        // Attempt reconnection as a recovery step
                        CheckAndReconnectControllers();
                    }
                }
                // *** FIX END ***
            }
            catch (Exception ex)
            {
                // Notify developer
                _ = LogErrors.LogErrorAsync(ex, "Error in method Update in class GamePadController");
            }
            finally
            {
                _updateLock.Release();
            }
        }
        catch (Exception ex)
        {
            // Notify developer
            _ = LogErrors.LogErrorAsync(ex, "Error in method Update in class GamePadController");
        }
    }

    public void CheckAndReconnectControllers()
    {
        // *** FIX START: Added lock to prevent race conditions with Dispose/Start/Stop ***
        lock (_stateLock)
        {
            // This method attempts to find and connect *either* an XInput or a DirectInput gamepad.
            // It should not create a new DirectInput instance if one already exists and is valid.
            try
            {
                if (!IsRunning || _isDisposed) return;

                // If XInput is already connected, no need to reconnect DirectInput
                if (_xinputController.IsConnected)
                {
                    // Ensure DirectInput controller is released if XInput is active
                    if (_directInputController == null)
                    {
                        return;
                    }

                    _directInputController?.Unacquire();
                    _directInputController?.Dispose();
                    _directInputController = null;
                    _playStationControllerGuid = Guid.Empty;

                    return; // XInput is active, nothing more to do
                }

                // If DirectInput object is null or disposed, try to recreate it
                if (_directInput == null || _directInput.IsDisposed)
                {
                    try
                    {
                        _directInput = new DirectInput();

                        // Notify developer
                        ErrorLogger?.Invoke(null, "Recreated DirectInput object during reconnection."); // Log successful recreation
                    }
                    catch (Exception diEx)
                    {
                        // Notify developer
                        ErrorLogger?.Invoke(diEx, $"Failed to recreate DirectInput object during reconnection attempt.\n\n" +
                                                  $"Exception type: {diEx.GetType().Name}\n" +
                                                  $"Exception details: {diEx.Message}");

                        _directInput = null; // Ensure it's null if creation failed
                        return; // Cannot proceed without a valid DirectInput object
                    }
                }

                // Check if the previously connected PlayStation controller is attached
                var devices = _directInput.GetDevices(DeviceType.Gamepad, DeviceEnumerationFlags.AttachedOnly);

                var found = false;
                DeviceInstance foundDevice = null;

                // First, try to find the specific controller by GUID if we had one
                if (_playStationControllerGuid != Guid.Empty)
                {
                    foreach (var deviceInstance in devices)
                    {
                        if (deviceInstance.InstanceGuid != _playStationControllerGuid) continue;

                        foundDevice = deviceInstance;
                        found = true;
                        break;
                    }
                }

                // If the specific GUID wasn't found or we didn't have one, just take the first available gamepad
                if (!found && devices.Count > 0)
                {
                    foundDevice = devices[0];
                    found = true;
                }

                if (found && foundDevice != null)
                {
                    // Found a device, try to connect
                    try
                    {
                        // Dispose the old controller if it exists and is different or invalid
                        // Access InstanceGuid via the Information property of the Joystick
                        if (_directInputController != null
                            && (_directInputController.Information.InstanceGuid != foundDevice.InstanceGuid ||
                                _directInputController.IsDisposed))
                        {
                            _directInputController?.Unacquire();
                            _directInputController?.Dispose();
                            _directInputController = null;
                        }

                        // If _directInputController is null (either was null, different, or disposed), create a new one
                        if (_directInputController == null)
                        {
                            _directInputController = new Joystick(_directInput, foundDevice.InstanceGuid);
                            _directInputController.Acquire();
                            _playStationControllerGuid = foundDevice.InstanceGuid; // Update the GUID

                            // Notify developer
                            // ErrorLogger?.Invoke(null, $"Successfully reconnected DirectInput controller: {foundDevice.InstanceName}"); // Log success
                        }
                        else
                        {
                            // If it wasn't null and was the same GUID, try re-acquiring just in case
                            _directInputController.Acquire();

                            // Notify developer
                            // DebugLogger.Log($"Successfully re-acquired DirectInput controller: {foundDevice.InstanceName}");
                        }
                    }
                    catch (Exception acquireEx)
                    {
                        // Failed to acquire the found device
                        // Notify developer
                        ErrorLogger?.Invoke(acquireEx, $"Failed to acquire DirectInput device during reconnection attempt: {foundDevice.InstanceName}.\n\n" +
                                                       $"Exception type: {acquireEx.GetType().Name}\n" +
                                                       $"Exception details: {acquireEx.Message}");

                        _directInputController?.Unacquire();
                        _directInputController?.Dispose();
                        _directInputController = null; // Ensure it's null on failure
                        _playStationControllerGuid = Guid.Empty; // Reset GUID on failure
                    }
                }
                else
                {
                    // No gamepad device found
                    if (_directInputController == null)
                    {
                        return;
                    }

                    _directInputController?.Unacquire();
                    _directInputController?.Dispose();
                    _directInputController = null;
                    _playStationControllerGuid = Guid.Empty;

                    // Notify developer
                    ErrorLogger?.Invoke(null, "DirectInput controller disconnected."); // Log disconnection
                }
            }
            catch (Exception ex)
            {
                // This catch block handles exceptions from the DirectInput object itself (e.g., GetDevices)
                // or other unexpected errors within the reconnection logic.
                // This is the catch block that generated the log message in the bug report.
                // Notify developer
                ErrorLogger?.Invoke(ex, $"Error reconnecting controllers. User was not notified.\n\n" +
                                        $"Exception type: {ex.GetType().Name}\n" +
                                        $"Exception details: {ex.Message}");

                // Clean up potentially invalid state
                _directInputController?.Unacquire();
                _directInputController?.Dispose();
                _directInputController = null;
                _playStationControllerGuid = Guid.Empty;
                // Do NOT dispose _directInput here, let the next attempt recreate it if needed.
            }
        }
        // *** FIX END ***
    }

    private void HandleXInputRightButton(State state)
    {
        var isBDown = state.Gamepad.Buttons.HasFlag(GamepadButtonFlags.B);
        switch (isBDown)
        {
            case true when !_wasBDown:
                _mouseSimulator.RightButtonDown();
                break;
            case false when _wasBDown:
                _mouseSimulator.RightButtonUp();
                break;
        }

        _wasBDown = isBDown;
    }

    private void HandleXInputLeftButton(State state)
    {
        var isADown = state.Gamepad.Buttons.HasFlag(GamepadButtonFlags.A);
        switch (isADown)
        {
            case true when !_wasADown:
                _mouseSimulator.LeftButtonDown();
                break;
            case false when _wasADown:
                _mouseSimulator.LeftButtonUp();
                break;
        }

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

    private (float, float) ProcessThumbStickXInput(short thumbX, short thumbY, float dzX, float dzY)
    {
        var normalizedX = Math.Max(-1, thumbX / MaxThumbValue);
        var normalizedY = Math.Max(-1, thumbY / MaxThumbValue);

        var resultX = Math.Abs(normalizedX) < dzX ? 0 : (Math.Abs(normalizedX) - dzX) * (normalizedX / Math.Abs(normalizedX));
        var resultY = Math.Abs(normalizedY) < dzY ? 0 : (Math.Abs(normalizedY) - dzY) * (normalizedY / Math.Abs(normalizedY));

        // Always apply base scaling, then additional scaling based on the deadzone
        resultX *= XInputScalingFactor;
        resultY *= XInputScalingFactor;

        if (dzX > 0)
        {
            resultX = resultX * (1.0f / (1.0f - dzX)); // Scale up to full range
        }

        if (dzY > 0)
        {
            resultY = resultY * (1.0f / (1.0f - dzY)); // Scale up to full range
        }

        return (resultX, resultY);
    }

    private void HandleDirectInputButtons(JoystickState state)
    {
        // DirectInput button indices can vary. Assuming common layout where 1 is Cross and 2 is Circle.
        const int crossButtonIndex = 1; // Assuming button 1 is Cross
        const int circleButtonIndex = 2; // Assuming button 2 is Circle

        if (state.Buttons.Length > Math.Max(crossButtonIndex, circleButtonIndex))
        {
            var isCrossDown = state.Buttons[crossButtonIndex];
            switch (isCrossDown)
            {
                case true when !_wasCrossDown:
                    _mouseSimulator.LeftButtonDown(); // Cross Button
                    break;
                case false when _wasCrossDown:
                    _mouseSimulator.LeftButtonUp();
                    break;
            }

            _wasCrossDown = isCrossDown;

            var isCircleDown = state.Buttons[circleButtonIndex];
            switch (isCircleDown)
            {
                case true when !_wasCircleDown:
                    _mouseSimulator.RightButtonDown(); // Circle Button
                    break;
                case false when _wasCircleDown:
                    _mouseSimulator.RightButtonUp();
                    break;
            }

            _wasCircleDown = isCircleDown;
        }
        else
        {
            // Notify developer
            // Log a warning or handle controllers with fewer buttons if necessary
            ErrorLogger?.Invoke(null, $"DirectInput controller has fewer than {Math.Max(crossButtonIndex, circleButtonIndex) + 1} buttons.");
        }
    }

    private void HandleDirectInputMovement(JoystickState state)
    {
        // Normalize DirectInput values from [0, 65535] to [-32767, 32767]
        // Check if axes exist before accessing
        if (state.X == int.MinValue || state.Y == int.MinValue) return; // Check if X and Y axes are reported

        var thumbX = (short)(state.X - 32767); // Convert absolute to relative
        var thumbY = (short)(state.Y - 32767); // Convert absolute to relative

        // Invert the X and Y-axis (DirectInput typically has an inverted axis relative to screen coordinates)
        thumbY = (short)-thumbY;
        thumbX = (short)-thumbX;

        // Process the thumbstick values with the dead zone
        var (x, y) = ProcessLeftThumbStickDirectInput(thumbX, thumbY, DeadZoneX, DeadZoneY);

        // Move the mouse based on processed values
        // The original code used -(int)x and -(int)y for MoveMouseBy. This seems counter-intuitive
        // given the inversion above. Let's use (int)x and (int)y and see if that feels right,
        // or revert if the original inversion + negative move was intentional for some reason.
        // Reverting to original logic for minimal change:
        _mouseSimulator.MoveMouseBy(-(int)x, -(int)y);
    }

    private void HandleDirectInputScroll(JoystickState state)
    {
        var thumbX = (short)(state.RotationZ - 32767); // Horizontal axis
        var thumbY = (short)-(state.RotationZ - 32767); // Inverted Y

        var (x, y) = ProcessRightThumbStickDirectInput(thumbX, thumbY, DeadZoneX, DeadZoneY);

        _mouseSimulator.HorizontalScroll((int)x); // Assuming this is correct for your controller
        _mouseSimulator.VerticalScroll((int)y);
    }

    private static (float, float) ProcessLeftThumbStickDirectInput(short thumbX, short thumbY, float dzX, float dzY)
    {
        var normalizedX = thumbX / MaxThumbValue;
        var normalizedY = thumbY / MaxThumbValue;

        float resultX = 0;
        if (Math.Abs(normalizedX) > dzX)
        {
            resultX = (Math.Abs(normalizedX) - dzX) * (normalizedX / Math.Abs(normalizedX));
        }

        float resultY = 0;
        if (Math.Abs(normalizedY) > dzY)
        {
            resultY = (Math.Abs(normalizedY) - dzY) * (normalizedY / Math.Abs(normalizedY));
        }

        // Always apply base scaling, then additional scaling based on the deadzone
        resultX *= DirectInputLeftThumbStickScalingFactor;
        resultY *= DirectInputLeftThumbStickScalingFactor;

        if (dzX > 0)
        {
            resultX = resultX * (1.0f / (1.0f - dzX)); // Scale up to full range
        }

        if (dzY > 0)
        {
            resultY = resultY * (1.0f / (1.0f - dzY)); // Scale up to full range
        }

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
        resultX *= DirectInputRightThumbStickScalingFactor;
        resultY *= DirectInputRightThumbStickScalingFactor;

        if (dzX > 0)
        {
            resultX = resultX * (1.0f / (1.0f - dzX)); // Scale up to full range
        }

        if (dzY > 0)
        {
            resultY = resultY * (1.0f / (1.0f - dzY)); // Scale up to full range
        }

        return (resultX, resultY);
    }
}
