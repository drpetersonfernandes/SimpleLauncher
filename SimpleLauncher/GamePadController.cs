using SharpDX.XInput;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WindowsInput;
using System.Windows.Input;
using System.Threading;

namespace SimpleLauncher
{
    internal class GamePadController
    {
        private const int MovementDivider = 2_000;
        private const int ScrollDivider = 10_000;
        private const int RefreshRate = 60;

        private Timer _timer;
        private Controller _controller;
        private IMouseSimulator _mouseSimulator;
        private IKeyboardSimulator _keyboardSimulator;


        private bool _wasADown;
        private bool _wasBDown;

        float deadzoneX = 0.05f;
        float deadzoneY = 0.02f;
        float leftStickX;
        float leftStickY;
        float rightStickX;
        float rightStickY;
        public GamePadController()
        {
            _controller = new Controller(UserIndex.One);
            _mouseSimulator = new InputSimulator().Mouse;
            _keyboardSimulator = new InputSimulator().Keyboard;
            _timer = new Timer(obj => Update());
        }

        public void Start()
        {
            _timer.Change(0, 1000 / RefreshRate);
        }

        private void Update()
        {

            _controller.GetState(out var state);
            Movement(state);
            Scroll(state);
            LeftButton(state);
            RightButton(state);
        }

        private void RightButton(State state)
        {
            var isBDown = state.Gamepad.Buttons.HasFlag(GamepadButtonFlags.B);
            if (isBDown && !_wasBDown) _mouseSimulator.RightButtonDown();
            if (!isBDown && _wasBDown) _mouseSimulator.RightButtonUp();
            _wasBDown = isBDown;
        }

        private void LeftButton(State state)
        {
            var isADown = state.Gamepad.Buttons.HasFlag(GamepadButtonFlags.A);
            if (isADown && !_wasADown) _mouseSimulator.LeftButtonDown();
            if (!isADown && _wasADown) _mouseSimulator.LeftButtonUp();
            _wasADown = isADown;
        }

        private void Scroll(State state)
        {

            float normRX = Math.Max(-1, (float)state.Gamepad.RightThumbX / 32767);
            float normRY = Math.Max(-1, (float)state.Gamepad.RightThumbY / 32767);

            rightStickX = (Math.Abs(normRX) < deadzoneX ? 0 : (Math.Abs(normRX) - deadzoneX) * (normRX / Math.Abs(normRX)));
            rightStickY = (Math.Abs(normRY) < deadzoneY ? 0 : (Math.Abs(normRY) - deadzoneY) * (normRY / Math.Abs(normRY)));

            if (deadzoneX > 0)
            {
                rightStickX *= 10 / (1 - deadzoneX);
            }
            if (deadzoneY > 0)
            {
                rightStickY *= 10 / (1 - deadzoneY);
            }


            _mouseSimulator.HorizontalScroll((int)rightStickX);
            _mouseSimulator.VerticalScroll((int)rightStickY);

        }

        private void Movement(State state)
        {


            float normLX = Math.Max(-1, (float)state.Gamepad.LeftThumbX / 32767);
            float normLY = Math.Max(-1, (float)state.Gamepad.LeftThumbY / 32767);

            leftStickX = (Math.Abs(normLX) < deadzoneX ? 0 : (Math.Abs(normLX) - deadzoneX) * (normLX / Math.Abs(normLX)));
            leftStickY = (Math.Abs(normLY) < deadzoneY ? 0 : (Math.Abs(normLY) - deadzoneY) * (normLY / Math.Abs(normLY)));

            if (deadzoneX > 0)
            {
                leftStickX *= 10 / (1 - deadzoneX);
            }
            if (deadzoneY > 0)
            {
                leftStickY *= 10 / (1 - deadzoneY);
            }


            _mouseSimulator.MoveMouseBy((int)leftStickX, -(int)leftStickY);
        }

    }
}
