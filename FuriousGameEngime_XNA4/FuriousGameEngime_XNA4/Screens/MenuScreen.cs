#region File Description
//-----------------------------------------------------------------------------
// MenuScreen.cs
//
// XNA Community Game Platform
// Copyright (C) Microsoft Corporation. All rights reserved.
//-----------------------------------------------------------------------------
#endregion

#region Using Statements
using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input.Touch;
using Microsoft.Xna.Framework.Input;
#endregion

namespace FuriousGameEngime_XNA4.Screens
{
    /// <summary>
    /// Base class for screens that contain a menu of options. The user can
    /// move up and down to select an entry, or cancel to back out of the screen.
    /// </summary>
    abstract class MenuScreen : Screen
    {
        #region Fields

        internal List<MenuEntry> _menuEntries = new List<MenuEntry>();
        int _selectedEntry = 0;
        string _menuTitle;

        public List<int> hitEntries = new List<int>();

        internal List<Rectangle> _textCollision = new List<Rectangle>();

        #endregion

        #region Properties

        ///// <summary>
        ///// Gets the list of menu entries, so derived classes can add
        ///// or change the menu contents.
        ///// </summary>
        //protected IList<MenuEntry> MenuEntries
        //{
        //    get { return _menuEntries; }
        //}

        //protected IList<Rectangle> collisionRectangles
        //{
        //    get { return _textCollision; }
        //}

        #endregion

        #region Initialization
        /// <summary>
        /// Constructor.
        /// </summary>
        public MenuScreen(string menuTitle)
        {
            this._menuTitle = menuTitle;

            TransitionOnTime = TimeSpan.FromSeconds(0.5);
            TransitionOffTime = TimeSpan.FromSeconds(0.5);
        }

        public override void LoadContent()
        {
            base.LoadContent();

            //Connot add during the load content due to the fact that screenmanager is not instantiated 
            //for (int i = 0; i < _menuEntries.Count; ++i)
            //{
            //    _menuEntries[i].AddCollisionRectangle();
            //}
        }

        #endregion

        /// <summary>
        /// returns -1 if no entry was found
        /// </summary>
        /// <param name="mousePosition"></param>
        /// <returns></returns>
        int GetEntryIndex(Vector2 mousePosition)
        {
            for (int i = 0; i < _menuEntries.Count; ++i)
            {
                if (_menuEntries[i].Rectangle.Contains((int)mousePosition.X, (int)mousePosition.Y))
                {
                    return i;
                }
            }
            return -1;
        }

        #region Handle Input
        /// <summary>
        /// Responds to user input, changing the selected entry and accepting
        /// or cancelling the menu.
        /// </summary>
        public override void HandleInput(InputState input)
        {
            if (!ScreenManager.Game.IsMouseVisible)
                ScreenManager.Game.IsMouseVisible = true;

            if (input.MouseLeftPressed)
            {

            }

            // Move to the previous menu entry?
            if (input.IsMenuUp(ControllingPlayer))
            {
                _selectedEntry--;

                if (_selectedEntry < 0)
                    _selectedEntry = _menuEntries.Count - 1;
            }

            // Move to the next menu entry?
            if (input.IsMenuDown(ControllingPlayer))
            {
                _selectedEntry++;

                if (_selectedEntry >= _menuEntries.Count)
                    _selectedEntry = 0;
            }

            // Accept or cancel the menu? We pass in our ControllingPlayer, which may
            // either be null (to accept input from any player) or a specific index.
            // If we pass a null controlling player, the InputState helper returns to
            // us which player actually provided the input. We pass that through to
            // OnSelectEntry and OnCancel, so they can tell which player triggered them.
            PlayerIndex playerIndex;

            if (input.IsMenuSelect(ControllingPlayer, out playerIndex))
            {
                OnSelectEntry(_selectedEntry, playerIndex);
                hitEntries.Add(_selectedEntry);

                for (int e = 0; e < hitEntries.Count; ++e)
                {
                    if (hitEntries[e] == _selectedEntry)
                    {
                        for (int d = e + 1; d < hitEntries.Count; ++d)
                        {
                            if (hitEntries[d] == _selectedEntry)
                            {
                                hitEntries.Remove(_selectedEntry);
                                hitEntries.Remove(_selectedEntry);
                                break;
                            }
                        }
                    }
                }
            }
            else if (input.IsMenuCancel(ControllingPlayer, out playerIndex))
            {
                OnCancel(playerIndex);
            }
        }
        
        /// <summary>
        /// Handler for when the user has chosen a menu entry.
        /// </summary>
        protected virtual void OnSelectEntry(int entryIndex, PlayerIndex playerIndex)
        {
            _menuEntries[entryIndex].OnSelectEntry(playerIndex);
        }
        
        /// <summary>
        /// Handler for when the user has cancelled the menu.
        /// </summary>
        protected virtual void OnCancel(PlayerIndex playerIndex)
        {
            ExitScreen();
        }
        
        /// <summary>
        /// Helper overload makes it easy to use OnCancel as a MenuEntry event handler.
        /// </summary>
        protected void OnCancel(object sender, PlayerIndexEventArgs e)
        {
            OnCancel(e.PlayerIndex);
        }
        
        /// <summary>
        /// Event handler for when the Quit Game menu entry is selected.
        /// </summary>
        protected void QuitGameMenuEntrySelected(object sender, PlayerIndexEventArgs e)
        {
            MessageBoxScreen confirmQuitMessageBox = new MessageBoxScreen(Resources.ConfirmQuitGame);

            confirmQuitMessageBox.Accepted += ConfirmQuitMessageBoxAccepted;

            ScreenManager.AddScreen(confirmQuitMessageBox, ControllingPlayer);
        }
        
        /// <summary>
        /// Event handler for when the user selects ok on the "are you sure
        /// you want to quit" message box. This uses the loading screen to
        /// transition from the game back to the main menu screen.
        /// </summary>
        void ConfirmQuitMessageBoxAccepted(object sender, PlayerIndexEventArgs e)
        {
            LoadingScreen.Load(ScreenManager, false, null, new BackgroundScreen(), new MainMenuScreen());
        }
        #endregion

        #region Update and Draw
        /// <summary>
        /// Allows the screen the chance to position the menu entries. By default
        /// all menu entries are lined up in a vertical list, centered on the screen.
        /// </summary>
        protected virtual void UpdateMenuEntryLocations()
        {
            // Make the menu slide into place during transitions, using a
            // power curve to make things look more interesting (this makes
            // the movement slow down as it nears the end).
            float transitionOffset = (float)Math.Pow(TransitionPosition, 2);

            // start at Y = 175; each X value is generated per entry
            Vector2 position = new Vector2(0f, 175f);

            // update each menu entry's location in turn
            for (int i = 0; i < _menuEntries.Count; i++)
            {                
                MenuEntry menuEntry = _menuEntries[i];

                if (menuEntry.center)
                {
                    // each entry is to be centered horizontally
                    position.X = ScreenManager.GraphicsDevice.Viewport.Width / 2 - menuEntry.GetWidth(this) / 2;

                    if (ScreenState == ScreenState.TransitionOn)
                        position.X -= transitionOffset * 256;
                    else
                        position.X += transitionOffset * 512;

                    // set the entry's position
                    menuEntry.Position = position;

                    // move down for the next entry the size of this entry
                    position.Y += menuEntry.GetHeight(this);
                }
                else
                {
                    //TODO: make it slide into place

                    //Vector2 p = menuEntry.Position;

                    //if (ScreenState == ScreenState.TransitionOn)
                    //    p.X -= transitionOffset * 256;
                    //else
                    //    p.X += transitionOffset * 512;

                    //menuEntry.Position = p;
                }
            }
        }


        /// <summary>
        /// Updates the menu.
        /// </summary>
        public override void Update(GameTime gameTime, bool otherScreenHasFocus, bool coveredByOtherScreen)
        {
            base.Update(gameTime, otherScreenHasFocus, coveredByOtherScreen);

            // Update each nested MenuEntry object.
            for (int i = 0; i < _menuEntries.Count; i++)
            {
                bool isSelected = IsActive && (i == _selectedEntry);

                _menuEntries[i].Update(this, isSelected, gameTime);
            }
        }


        /// <summary>
        /// Draws the menu.
        /// </summary>
        public override void Draw(GameTime gameTime)
        {
            // make sure our entries are in the right place before we draw them
            UpdateMenuEntryLocations();

            GraphicsDevice graphics = ScreenManager.GraphicsDevice;
            SpriteBatch spriteBatch = ScreenManager.SpriteBatch;
            SpriteFont font = ScreenManager.Font;

            spriteBatch.Begin();

            // Draw each menu entry in turn.
            for (int i = 0; i < _menuEntries.Count; i++)
            {
                MenuEntry menuEntry = _menuEntries[i];

                bool isSelected = IsActive && (i == _selectedEntry);
                bool hitEntry = false;
                
                for (int e = 0; e < hitEntries.Count; ++ e)
                {
                    hitEntry = IsActive && (i == hitEntries[e]);
                    if (hitEntry) break;
                }

                menuEntry.Draw(this, isSelected, hitEntry, gameTime);
            }

            // Make the menu slide into place during transitions, using a
            // power curve to make things look more interesting (this makes
            // the movement slow down as it nears the end).
            float transitionOffset = (float)Math.Pow(TransitionPosition, 2);

            // Draw the menu title centered on the screen
            Vector2 titlePosition = new Vector2(graphics.Viewport.Width / 2, 80);
            Vector2 titleOrigin = font.MeasureString(_menuTitle) / 2;
            Color titleColor = new Color(192, 192, 192) * TransitionAlpha;
            float titleScale = 1.25f;

            titlePosition.Y -= transitionOffset * 100;

            spriteBatch.DrawString(font, _menuTitle, titlePosition, titleColor, 0,
                                   titleOrigin, titleScale, SpriteEffects.None, 0);

            spriteBatch.End();
        }


        #endregion

        //TODO: Add Menu Text that doesn't do anything.
        //TODO: Mouse selects text.
        
        //TODO: Menu Editor
        //- could use recursion like the scene loader to get all the menu entries/screens load them all in a side panel and have the player drag and drop them where they want them.
        //- choose transition type, speed, duration
    }
}
