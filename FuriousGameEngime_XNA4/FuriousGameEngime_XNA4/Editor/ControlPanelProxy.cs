using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using System.Windows.Threading;

namespace FuriousGameEngime_XNA4
{
    class ControlPanelProxy
    {
        private ControlPanel _panel;
        private Dispatcher _disp;
        
        public ControlPanelProxy(ControlPanel p)
        {
            _panel = p;
            _disp = p.Dispatcher;
        }

        public bool AddPhysicsMesh()
        {
            return (bool)_disp.Invoke((Func<bool>)(() => _panel.AddPhysicsMesh.IsChecked.Value));
        }

        public bool IsGhost()
        {
            return (bool)_disp.Invoke((Func<bool>)(() => _panel.IsGhost.IsChecked.Value));
        }

        public bool AllowDeactivation()
        {
            return (bool)_disp.Invoke((Func<bool>)(() => _panel.AllowDeactivation.IsChecked.Value));
        }

        public bool IsStatic()
        {
            return (bool)_disp.Invoke((Func<bool>)(() => _panel.IsStatic.IsChecked.Value));
        }

        public bool AffectedByGravity()
        {
            return (bool)_disp.Invoke((Func<bool>)(() => _panel.AffectedByGravity.IsChecked.Value));
        }

        public string CollisionType
        {
            get
            {
                return _disp.Invoke((Func<string>)(() => _panel.CollisionType.SelectedItem.ToString())) as string;
            }
        }

        public void Exit()
        {
            _disp.InvokeShutdown();
        }
    }
}
