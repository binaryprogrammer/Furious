#region File Description
//-----------------------------------------------------------------------------
// InputState.cs
//
// Microsoft XNA Community Game Platform
// Copyright (C) Microsoft Corporation. All rights reserved.
//-----------------------------------------------------------------------------
#endregion

#region Using Statements
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Input.Touch;
using System.Collections.Generic;
#endregion

namespace FuriousGameEngime_XNA4
{
    /// <summary>
    /// Helper for reading input from keyboard, gamepad, and touch input. This class 
    /// tracks both the current and previous state of the input devices, and implements 
    /// query methods for high level input actions such as "move up through the menu"
    /// or "pause the game".
    /// </summary>
    public class InputState
    {
        #region Fields

        public const int MaxInputs = 4;

        public readonly KeyboardState[] CurrentKeyboardStates;
        public readonly GamePadState[] CurrentGamePadStates;

        public readonly KeyboardState[] LastKeyboardStates;
        public readonly GamePadState[] LastGamePadStates;
        
        public readonly bool[] GamePadWasConnected;

        public MouseState CurrentMouseState;
        public MouseState LastMouseState;

        public TouchCollection TouchState;

        public readonly List<GestureSample> Gestures = new List<GestureSample>();

        #endregion

        #region Initialization


        /// <summary>
        /// Constructs a new input state.
        /// </summary>
        public InputState()
        {
            CurrentKeyboardStates = new KeyboardState[MaxInputs];
            CurrentGamePadStates = new GamePadState[MaxInputs];

            LastKeyboardStates = new KeyboardState[MaxInputs];
            LastGamePadStates = new GamePadState[MaxInputs];

            GamePadWasConnected = new bool[MaxInputs];
        }


        #endregion

        #region Public Methods

        /// <summary>
        /// Reads the latest state of the keyboard and gamepad.
        /// </summary>
        public void Update()
        {
            for (int i = 0; i < MaxInputs; i++)
            {
                LastKeyboardStates[i] = CurrentKeyboardStates[i];
                LastGamePadStates[i] = CurrentGamePadStates[i];

                CurrentKeyboardStates[i] = Keyboard.GetState((PlayerIndex)i);
                CurrentGamePadStates[i] = GamePad.GetState((PlayerIndex)i);

                // Keep track of whether a gamepad has ever been
                // connected, so we can detect if it is unplugged.
                if (CurrentGamePadStates[i].IsConnected)
                {
                    GamePadWasConnected[i] = true;
                }
            }

            LastMouseState = CurrentMouseState;
            CurrentMouseState = Mouse.GetState();

            TouchState = TouchPanel.GetState();

            Gestures.Clear();
            while (TouchPanel.IsGestureAvailable)
            {
                Gestures.Add(TouchPanel.ReadGesture());
            }
        }
        #endregion

        #region Keyboard Helper Functions
        /// <summary>
        /// Helper for checking if a key was newly pressed during this update. The controllingPlayer parameter specifies which player to read input for.
        /// If this is null, it will accept input from any player. When a keypress is detected, the output playerIndex reports which player pressed it.
        /// </summary>
        public bool KeyPressed(Keys key, PlayerIndex? controllingPlayer, out PlayerIndex playerIndex)
        {
            if (controllingPlayer.HasValue)
            {
                // Read input from the specified player.
                playerIndex = controllingPlayer.Value;

                int i = (int)playerIndex;

                return (CurrentKeyboardStates[i].IsKeyDown(key) && LastKeyboardStates[i].IsKeyUp(key));
            }
            else
            {
                // Accept input from any player.
                return (KeyPressed(key, PlayerIndex.One, out playerIndex) || KeyPressed(key, PlayerIndex.Two, out playerIndex) ||
                        KeyPressed(key, PlayerIndex.Three, out playerIndex) || KeyPressed(key, PlayerIndex.Four, out playerIndex));
            }
        }
        
        /// <summary>
        /// Checks if a key was newly pressed during this update. If this is null, it will accept input from any player
        /// </summary>
        /// <param name="key"></param>
        /// <param name="controllingPlayer"> Which player to read input for.</param>
        /// <returns></returns>
        public bool KeyPressed(Keys key, PlayerIndex? controllingPlayer)
        {
            PlayerIndex playerIndex;

            if (controllingPlayer.HasValue)
            {
                // Read input from the specified player.
                playerIndex = controllingPlayer.Value;

                int i = (int)playerIndex;

                return (CurrentKeyboardStates[i].IsKeyDown(key) && LastKeyboardStates[i].IsKeyUp(key));
            }
            else
            {
                // Accept input from any player.
                return (KeyPressed(key, PlayerIndex.One) || KeyPressed(key, PlayerIndex.Two) ||
                        KeyPressed(key, PlayerIndex.Three) || KeyPressed(key, PlayerIndex.Four));
            }
        }

        /// <summary>
        /// save a function call by doing this yourself
        /// </summary>
        /// <param name="key">the key to check for</param>
        /// <param name="controllingPlayer">the player to check against. null checks all players</param>
        /// <returns>true if the specified player is holding the requested key</returns>
        public bool KeyDown(Keys key, PlayerIndex? controllingPlayer)
        {
            PlayerIndex playerIndex;

            if (controllingPlayer.HasValue)
            {
                // Read input from the specified player.
                playerIndex = controllingPlayer.Value;

                int i = (int)playerIndex;

                return (CurrentKeyboardStates[i].IsKeyDown(key));
            }
            else
            {
                // Accept input from any player.
                return (KeyDown(key, PlayerIndex.One) || KeyDown(key, PlayerIndex.Two) ||
                        KeyDown(key, PlayerIndex.Three) || KeyDown(key, PlayerIndex.Four));
            }
        }
        #endregion
        
        #region Xbox Helper Functions
        /// <summary>
        /// Helper for checking if a button was newly pressed during this update. The controllingPlayer parameter specifies which player to read input for.
        /// If this is null, it will accept input from any player. When a button press is detected, the output playerIndex reports which player pressed it.
        /// </summary>
        bool ButtonPressed(Buttons button, PlayerIndex? controllingPlayer, out PlayerIndex playerIndex)
        {
            if (controllingPlayer.HasValue)
            {
                // Read input from the specified player.
                playerIndex = controllingPlayer.Value;

                int i = (int)playerIndex;

                return (CurrentGamePadStates[i].IsButtonDown(button) && LastGamePadStates[i].IsButtonUp(button));
            }
            else
            {
                // Accept input from any player.
                return (ButtonPressed(button, PlayerIndex.One, out playerIndex) || ButtonPressed(button, PlayerIndex.Two, out playerIndex) ||
                        ButtonPressed(button, PlayerIndex.Three, out playerIndex) || ButtonPressed(button, PlayerIndex.Four, out playerIndex));
            }
        }

        bool ButtonPressed(Buttons button, PlayerIndex? controllingPlayer)
        {
            PlayerIndex playerIndex;

            if (controllingPlayer.HasValue)
            {
                // Read input from the specified player.
                playerIndex = controllingPlayer.Value;

                int i = (int)playerIndex;

                return (CurrentGamePadStates[i].IsButtonDown(button) && LastGamePadStates[i].IsButtonUp(button));
            }
            else
            {
                // Accept input from any player.
                return (ButtonPressed(button, PlayerIndex.One) || ButtonPressed(button, PlayerIndex.Two) ||
                        ButtonPressed(button, PlayerIndex.Three) || ButtonPressed(button, PlayerIndex.Four));
            }
        }

        bool TriggerPressed(GamePadTriggers trigger, PlayerIndex? controllingPlayer)
        {
            PlayerIndex playerIndex;

            if (controllingPlayer.HasValue)
            {
                // Read input from the specified player.
                playerIndex = controllingPlayer.Value;

                int i = (int)playerIndex;

                return (CurrentGamePadStates[i].Triggers == trigger);
            }
            else
            {
                // Accept input from any player.
                return (TriggerPressed(trigger, PlayerIndex.One) || TriggerPressed(trigger, PlayerIndex.Two) ||
                        TriggerPressed(trigger, PlayerIndex.Three) || TriggerPressed(trigger, PlayerIndex.Four));
            }
        }
        #endregion

        #region Mouse Helper Functions
        public bool MouseLeftPressed
        {
            get
            {
                return CurrentMouseState.LeftButton == ButtonState.Pressed && LastMouseState.LeftButton == ButtonState.Released;
            }
        }

        public bool MouseRightPressed
        {
            get
            {
                return CurrentMouseState.RightButton == ButtonState.Pressed && LastMouseState.RightButton == ButtonState.Released;
            }
        }

        /// <summary>
        /// the scroll wheel difference between the last tick and this tick
        /// </summary>
        public int MouseScrollWheel
        {
            get
            {
                return CurrentMouseState.ScrollWheelValue - LastMouseState.ScrollWheelValue;
            }
        }
        #endregion

        #region Generic Menu Controls
        /// <summary>
        /// Checks for a "menu select" input action. The controllingPlayer parameter specifies which player to read input for.
        /// If this is null, it will accept input from any player. When the action is detected, the output playerIndex reports which player pressed it.
        /// </summary>
        public bool IsMenuSelect(PlayerIndex? controllingPlayer, out PlayerIndex playerIndex)
        {
            return KeyPressed(Keys.Space, controllingPlayer, out playerIndex) || KeyPressed(Keys.Enter, controllingPlayer, out playerIndex) ||
                   ButtonPressed(Buttons.A, controllingPlayer, out playerIndex) || ButtonPressed(Buttons.Start, controllingPlayer, out playerIndex);
        }


        /// <summary>
        /// Checks for a "menu cancel" input action. The controllingPlayer parameter specifies which player to read input for.
        /// If this is null, it will accept input from any player. When the action is detected, the output playerIndex reports which player pressed it.
        /// </summary>
        public bool IsMenuCancel(PlayerIndex? controllingPlayer, out PlayerIndex playerIndex)
        {
            return KeyPressed(Keys.Escape, controllingPlayer, out playerIndex) || ButtonPressed(Buttons.B, controllingPlayer, out playerIndex) ||
                   ButtonPressed(Buttons.Back, controllingPlayer, out playerIndex);
        }


        /// <summary>
        /// Checks for a "menu up" input action. The controllingPlayer parameter specifies which player to read
        /// input for. If this is null, it will accept input from any player.
        /// </summary>
        public bool IsMenuUp(PlayerIndex? controllingPlayer)
        {
            PlayerIndex playerIndex;

            return KeyPressed(Keys.Up, controllingPlayer, out playerIndex) || ButtonPressed(Buttons.DPadUp, controllingPlayer, out playerIndex) ||
                   ButtonPressed(Buttons.LeftThumbstickUp, controllingPlayer, out playerIndex);
        }


        /// <summary>
        /// Checks for a "menu down" input action. The controllingPlayer parameter specifies which player to read
        /// input for. If this is null, it will accept input from any player.
        /// </summary>
        public bool IsMenuDown(PlayerIndex? controllingPlayer)
        {
            PlayerIndex playerIndex;

            return KeyPressed(Keys.Down, controllingPlayer, out playerIndex) || ButtonPressed(Buttons.DPadDown, controllingPlayer, out playerIndex) ||
                   ButtonPressed(Buttons.LeftThumbstickDown, controllingPlayer, out playerIndex);
        }


        /// <summary>
        /// Checks for a "pause the game" input action. The controllingPlayer parameter specifies which player to read
        /// input for. If this is null, it will accept input from any player.
        /// </summary>
        public bool IsPauseGame(PlayerIndex? controllingPlayer)
        {
            PlayerIndex playerIndex;

            return KeyPressed(Keys.Escape, controllingPlayer, out playerIndex) || ButtonPressed(Buttons.Back, controllingPlayer, out playerIndex) ||
                   ButtonPressed(Buttons.Start, controllingPlayer, out playerIndex);
        }
        #endregion
    }
}
