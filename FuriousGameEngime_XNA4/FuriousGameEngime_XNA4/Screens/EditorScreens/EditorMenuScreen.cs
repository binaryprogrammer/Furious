using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Jitter.Dynamics;
using Microsoft.Xna.Framework;

namespace FuriousGameEngime_XNA4.Screens.EditorScreens
{
    class EditorMenuScreen : MenuScreen
    {
        Editor _editor;

        #region Initilization

        public EditorMenuScreen(Editor editor)
            : base("Editor Menu")
        {
            _editor = editor;

            MenuEntry resumeGameMenuEntry = new MenuEntry(this as MenuScreen, Resources.ResumeGame);
            resumeGameMenuEntry.Selected += OnCancel;
            //MenuEntries.Add(resumeGameMenuEntry);

            MenuEntry ParticleEditor = new MenuEntry(this as MenuScreen, "Particle Editor", new Vector2(20, 20));
            ParticleEditor.Selected += ParticleEditorSelected;
            //MenuEntries.Add(ParticleEditor);

            MenuEntry MissionEditor = new MenuEntry(this as MenuScreen, "Mission Editor");
            MissionEditor.Selected += MissionEditorSelected;
            //MenuEntries.Add(MissionEditor);

            MenuEntry quitGameMenuEntry = new MenuEntry(this as MenuScreen, Resources.QuitGame);
            quitGameMenuEntry.Selected += QuitGameMenuEntrySelected;
            //MenuEntries.Add(quitGameMenuEntry);
        }

        #endregion

        #region Handle Input
        void ParticleEditorSelected(object sender, PlayerIndexEventArgs e)
        {
            ScreenManager.AddScreen(new ParticleMenuScreen(_editor), e.PlayerIndex);
        }

        void MissionEditorSelected(object sender, PlayerIndexEventArgs e)
        {
            ScreenManager.AddScreen(new MissionEditorMenuScreen(), e.PlayerIndex);
        }
        #endregion
    }
}
