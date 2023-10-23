using SharpDX.XInput;
using System.Threading;
using System;
using WindowsInput;

namespace SimpleLauncher
{
    public class GamePadController : IDisposable
    {
        private readonly Action<Exception, string> _errorLogger;
        private const int RefreshRate = 60;
        private const float MaxThumbValue = 32767.0f;  // Maximum thumbstick value for normalization.

        private readonly Timer _timer;
        private readonly Controller _controller;
        private readonly IMouseSimulator _mouseSimulator;

        private bool _wasADown;
        private bool _wasBDown;
        private bool _isDisposed = false;

        readonly float deadZoneX = 0.05f;
        readonly float deadZoneY = 0.02f;

        public GamePadController(Action<Exception, string> errorLogger = null)
        {
            _controller = new Controller(UserIndex.One);
            _mouseSimulator = new InputSimulator().Mouse;
            _timer = new Timer(obj => Update());
            _errorLogger = errorLogger;
        }

        public void Start() => _timer.Change(0, 1000 / RefreshRate);

        public void Stop()
        {
            if (_timer != null && !_isDisposed)
                _timer.Change(Timeout.Infinite, Timeout.Infinite);
        }

        public void Dispose()
        {
            if (_isDisposed) return;

            Stop();
            _timer?.Dispose();
            _isDisposed = true;
        }

        private void Update()
        {
            try
            {
                _controller.GetState(out var state);
                HandleMovement(state);
                HandleScroll(state);
                HandleLeftButton(state);
                HandleRightButton(state);
            }
            catch (Exception ex)
            {
                _errorLogger?.Invoke(ex, "Error in GamePadController Update.");
            }
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
            var (x, y) = ProcessThumbStick(state.Gamepad.RightThumbX, state.Gamepad.RightThumbY, deadZoneX, deadZoneY);
            _mouseSimulator.HorizontalScroll((int)x);
            _mouseSimulator.VerticalScroll((int)y);
        }

        private void HandleMovement(State state)
        {
            var (x, y) = ProcessThumbStick(state.Gamepad.LeftThumbX, state.Gamepad.LeftThumbY, deadZoneX, deadZoneY);
            _mouseSimulator.MoveMouseBy((int)x, -(int)y);
        }

        private (float, float) ProcessThumbStick(short thumbX, short thumbY, float dzX, float dzY)
        {
            float normalizedX = Math.Max(-1, (float)thumbX / MaxThumbValue);
            float normalizedY = Math.Max(-1, (float)thumbY / MaxThumbValue);

            float resultX = (Math.Abs(normalizedX) < dzX ? 0 : (Math.Abs(normalizedX) - dzX) * (normalizedX / Math.Abs(normalizedX)));
            float resultY = (Math.Abs(normalizedY) < dzY ? 0 : (Math.Abs(normalizedY) - dzY) * (normalizedY / Math.Abs(normalizedY)));

            if (dzX > 0) resultX *= 10 / (1 - dzX);
            if (dzY > 0) resultY *= 10 / (1 - dzY);

            return (resultX, resultY);
        }
    }
}
