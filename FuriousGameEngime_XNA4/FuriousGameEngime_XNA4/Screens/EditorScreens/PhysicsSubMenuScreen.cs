using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace FuriousGameEngime_XNA4.Screens.EditorScreens
{
    class PhysicsSubMenuScreen : MenuScreen
    {
        #region Initilization

        public PhysicsSubMenuScreen()
            : base("Physics-Editor Menu")
        {
            MenuEntry IsStaticEntry = new MenuEntry(this as MenuScreen, Resources.IsStatic);
            IsStaticEntry.Selected += IsStaticEntrySelected;
            //MenuEntries.Add(IsStaticEntry);

            MenuEntry IsGhostEntry = new MenuEntry(this as MenuScreen, Resources.IsGhost);
            IsGhostEntry.Selected += IsGhostEntrySelected;
            //MenuEntries.Add(IsGhostEntry);

            MenuEntry GravityEntry = new MenuEntry(this as MenuScreen, Resources.AffectedByGravity);
            GravityEntry.Selected += GravityEntrySelected;
            //MenuEntries.Add(GravityEntry);
        }

        #endregion

        #region Handle Input

        void IsStaticEntrySelected(object sender, PlayerIndexEventArgs e)
        {

        }

        void GravityEntrySelected(object sender, PlayerIndexEventArgs e)
        {

        }

        void IsGhostEntrySelected(object sender, PlayerIndexEventArgs e)
        {

        }

        #endregion
    }
}
