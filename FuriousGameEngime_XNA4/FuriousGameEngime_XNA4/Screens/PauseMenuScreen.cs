#region File Description
//-----------------------------------------------------------------------------
// PauseMenuScreen.cs
//
// Microsoft XNA Community Game Platform
// Copyright (C) Microsoft Corporation. All rights reserved.
//-----------------------------------------------------------------------------
#endregion

#region Using Statements
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Net;
#endregion

namespace FuriousGameEngime_XNA4.Screens
{
    /// <summary>
    /// The pause menu comes up over the top of the game,
    /// giving the player options to resume or quit.
    /// </summary>
    class PauseMenuScreen : MenuScreen
    {
        #region Initialization
        
        /// <summary>
        /// Constructor.
        /// </summary>
        public PauseMenuScreen()
            : base(Resources.Paused)
        {
            // Add the Resume Game menu entry.
            MenuEntry resumeGameMenuEntry = new MenuEntry(this as MenuScreen, Resources.ResumeGame);
            resumeGameMenuEntry.Selected += OnCancel;
            //MenuEntries.Add(resumeGameMenuEntry);

            // If this is a single player game, add the Quit menu entry.
            MenuEntry quitGameMenuEntry = new MenuEntry(this as MenuScreen, Resources.QuitGame);
            quitGameMenuEntry.Selected += QuitGameMenuEntrySelected;
            //MenuEntries.Add(quitGameMenuEntry);
        }
        #endregion
    }
}