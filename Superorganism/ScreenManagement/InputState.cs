// Adapted from the MonoGame port of the original XNA GameStateExample 
// https://github.com/tomizechsterson/game-state-management-monogame

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;

namespace Superorganism.ScreenManagement
{
    /// <summary>
    /// Helper for reading input from keyboard, gamepad, and touch input. This class 
    /// tracks both the current and previous state of the input devices, and implements 
    /// query methods for high level input actions such as "move up through the menu"
    /// or "pause the game".
    /// </summary> 
    public class InputState
    {
        private const int MaxInputs = 4;

        public readonly KeyboardState[] CurrentKeyboardStates;
        public readonly GamePadState[] CurrentGamePadStates;
        public MouseState CurrentMouseState { get; private set; }
        public MouseState LastMouseState { get; private set; }

        private readonly KeyboardState[] _lastKeyboardStates;
        private readonly GamePadState[] _lastGamePadStates;

        public readonly bool[] GamePadWasConnected;

        /// <summary>
        /// Constructs a new InputState
        /// </summary>
        public InputState()
        {
            CurrentKeyboardStates = new KeyboardState[MaxInputs];
            CurrentGamePadStates = new GamePadState[MaxInputs];

            _lastKeyboardStates = new KeyboardState[MaxInputs];
            _lastGamePadStates = new GamePadState[MaxInputs];

            GamePadWasConnected = new bool[MaxInputs];

            CurrentMouseState = Mouse.GetState();
            LastMouseState = CurrentMouseState;
        }

        // Reads the latest user input state.
        public void Update()
        {
            for (int i = 0; i < MaxInputs; i++)
            {
                _lastKeyboardStates[i] = CurrentKeyboardStates[i];
                _lastGamePadStates[i] = CurrentGamePadStates[i];

                CurrentKeyboardStates[i] = Keyboard.GetState();
                CurrentGamePadStates[i] = GamePad.GetState((PlayerIndex)i);

                // Keep track of whether a gamepad has ever been
                // connected, so we can detect if it is unplugged.
                if (CurrentGamePadStates[i].IsConnected)
                    GamePadWasConnected[i] = true;
            }

            // Update mouse state
            LastMouseState = CurrentMouseState;
            CurrentMouseState = Mouse.GetState();
        }

        /// <summary>
        /// Helper for checking if a key was pressed during this update. The
        /// controllingPlayer parameter specifies which player to read input for.
        /// If this is null, it will accept input from any player. When a keypress
        /// is detected, the output playerIndex reports which player pressed it.
        /// </summary>
        public bool IsKeyPressed(Keys key, PlayerIndex? controllingPlayer, out PlayerIndex playerIndex)
        {
            if (controllingPlayer.HasValue)
            {
                // Read input from the specified player.
                playerIndex = controllingPlayer.Value;

                int i = (int)playerIndex;

                return CurrentKeyboardStates[i].IsKeyDown(key);
            }

            // Accept input from any player.
            return IsKeyPressed(key, PlayerIndex.One, out playerIndex) ||
                    IsKeyPressed(key, PlayerIndex.Two, out playerIndex) ||
                    IsKeyPressed(key, PlayerIndex.Three, out playerIndex) ||
                    IsKeyPressed(key, PlayerIndex.Four, out playerIndex);
        }

        /// <summary>
        /// Helper for checking if a button was pressed during this update.
        /// The controllingPlayer parameter specifies which player to read input for.
        /// If this is null, it will accept input from any player. When a button press
        /// is detected, the output playerIndex reports which player pressed it.
        /// </summary>
        public bool IsButtonPressed(Buttons button, PlayerIndex? controllingPlayer, out PlayerIndex playerIndex)
        {
            if (controllingPlayer.HasValue)
            {
                // Read input from the specified player.
                playerIndex = controllingPlayer.Value;

                int i = (int)playerIndex;

                return CurrentGamePadStates[i].IsButtonDown(button);
            }

            // Accept input from any player.
            return IsButtonPressed(button, PlayerIndex.One, out playerIndex) ||
                    IsButtonPressed(button, PlayerIndex.Two, out playerIndex) ||
                    IsButtonPressed(button, PlayerIndex.Three, out playerIndex) ||
                    IsButtonPressed(button, PlayerIndex.Four, out playerIndex);
        }


        /// <summary>
        /// Helper for checking if a key was newly pressed during this update. The
        /// controllingPlayer parameter specifies which player to read input for.
        /// If this is null, it will accept input from any player. When a keypress
        /// is detected, the output playerIndex reports which player pressed it.
        /// </summary> 
        public bool IsNewKeyPress(Keys key, PlayerIndex? controllingPlayer, out PlayerIndex playerIndex)
        {
            if (controllingPlayer.HasValue)
            {
                // Read input from the specified player.
                playerIndex = controllingPlayer.Value;

                int i = (int)playerIndex;

                return (CurrentKeyboardStates[i].IsKeyDown(key) &&
                        _lastKeyboardStates[i].IsKeyUp(key));
            }

            // Accept input from any player.
            return IsNewKeyPress(key, PlayerIndex.One, out playerIndex) ||
                    IsNewKeyPress(key, PlayerIndex.Two, out playerIndex) ||
                    IsNewKeyPress(key, PlayerIndex.Three, out playerIndex) ||
                    IsNewKeyPress(key, PlayerIndex.Four, out playerIndex);
        }

        // Helper for checking if a button was newly pressed during this update.
        // The controllingPlayer parameter specifies which player to read input for.
        // If this is null, it will accept input from any player. When a button press
        // is detected, the output playerIndex reports which player pressed it.
        public bool IsNewButtonPress(Buttons button, PlayerIndex? controllingPlayer, out PlayerIndex playerIndex)
        {
            if (controllingPlayer.HasValue)
            {
                // Read input from the specified player.
                playerIndex = controllingPlayer.Value;

                int i = (int)playerIndex;

                return CurrentGamePadStates[i].IsButtonDown(button) &&
                        _lastGamePadStates[i].IsButtonUp(button);
            }

            // Accept input from any player.
            return IsNewButtonPress(button, PlayerIndex.One, out playerIndex) ||
                    IsNewButtonPress(button, PlayerIndex.Two, out playerIndex) ||
                    IsNewButtonPress(button, PlayerIndex.Three, out playerIndex) ||
                    IsNewButtonPress(button, PlayerIndex.Four, out playerIndex);
        }

        /// <summary>
        /// Helper for checking if a mouse button was newly pressed during this update.
        /// </summary>
        /// <param name="button">The mouse button to check</param>
        /// <returns>True if the button was pressed this update, but not the previous update</returns>
        public bool IsNewMouseButtonPress(MouseButtons button)
        {
            return button switch
            {
                MouseButtons.Left => CurrentMouseState.LeftButton == ButtonState.Pressed &&
                                    LastMouseState.LeftButton == ButtonState.Released,
                MouseButtons.Right => CurrentMouseState.RightButton == ButtonState.Pressed &&
                                     LastMouseState.RightButton == ButtonState.Released,
                MouseButtons.Middle => CurrentMouseState.MiddleButton == ButtonState.Pressed &&
                                      LastMouseState.MiddleButton == ButtonState.Released,
                MouseButtons.XButton1 => CurrentMouseState.XButton1 == ButtonState.Pressed &&
                                        LastMouseState.XButton1 == ButtonState.Released,
                MouseButtons.XButton2 => CurrentMouseState.XButton2 == ButtonState.Pressed &&
                                        LastMouseState.XButton2 == ButtonState.Released,
                _ => false
            };
        }

        /// <summary>
        /// Helper for checking if a mouse button is currently pressed.
        /// </summary>
        /// <param name="button">The mouse button to check</param>
        /// <returns>True if the button is currently pressed</returns>
        public bool IsMouseButtonPressed(MouseButtons button)
        {
            return button switch
            {
                MouseButtons.Left => CurrentMouseState.LeftButton == ButtonState.Pressed,
                MouseButtons.Right => CurrentMouseState.RightButton == ButtonState.Pressed,
                MouseButtons.Middle => CurrentMouseState.MiddleButton == ButtonState.Pressed,
                MouseButtons.XButton1 => CurrentMouseState.XButton1 == ButtonState.Pressed,
                MouseButtons.XButton2 => CurrentMouseState.XButton2 == ButtonState.Pressed,
                _ => false
            };
        }
    }

    /// <summary>
    /// Enumeration of mouse buttons
    /// </summary>
    public enum MouseButtons
    {
        Left,
        Right,
        Middle,
        XButton1,
        XButton2
    }
}