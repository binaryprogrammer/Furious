using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace FuriousGameEngime_XNA4.Screens.EditorScreens
{
    class PhysicsMenuScreen : MenuScreen
    {
        #region Initilization
        
        public PhysicsMenuScreen()
            : base("Editor Menu")
        {
            MenuEntry resumeGameMenuEntry = new MenuEntry(this as MenuScreen, Resources.ResumeGame);
            resumeGameMenuEntry.Selected += OnCancel;
            //MenuEntries.Add(resumeGameMenuEntry);

            //MenuEntry defaultValuesEntry = new MenuEntry(this as MenuScreen, Resources.DefaultValues);
            //defaultValuesEntry.Selected += DefaultValuesSelected;
            //MenuEntries.Add(defaultValuesEntry);

            MenuEntry PhysicsMeshEntry = new MenuEntry(this as MenuScreen, Resources.AddPhysicsMesh);
            PhysicsMeshEntry.Selected += PhysicsMeshEntrySelected;
            //MenuEntries.Add(PhysicsMeshEntry);
        }
        #endregion

        #region Handle Input
        /// <summary>
        /// Event handler for when the Single Player menu entry is selected.
        /// </summary>
        void PhysicsMeshEntrySelected(object sender, PlayerIndexEventArgs e)
        {
            ScreenManager.AddScreen(new PhysicsSubMenuScreen(), e.PlayerIndex);
        }

        void DefaultValuesSelected(object sender, PlayerIndexEventArgs e)
        {
            //TODO: determine and set default values
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
    }
}
