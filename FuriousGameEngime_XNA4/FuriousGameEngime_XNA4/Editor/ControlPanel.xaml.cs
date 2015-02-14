using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Windows.Interop;

namespace FuriousGameEngime_XNA4
{
    /// <summary>
    /// Interaction logic for ControlPanel.xaml
    /// </summary>
    public partial class ControlPanel : Window
    {
        IntPtr masterHandle;
        IntPtr thisHandle;

        public ControlPanel(IntPtr handle)
        {
            masterHandle = handle;

            InitializeComponent();

            this.Loaded += new RoutedEventHandler(Window_Loaded);
            this.WindowState = WindowState.Minimized;
        }
        
        private void AddPhysicsMesh_Checked(object sender, RoutedEventArgs e)
        {
            
        }

        private void IsGhost_Checked(object sender, RoutedEventArgs e)
        {

        }

        private void AllowDeactivation_Checked(object sender, RoutedEventArgs e)
        {

        }

        private void AffectedByGravity_Checked(object sender, RoutedEventArgs e)
        {

        }

        private void IsStatic_Checked(object sender, RoutedEventArgs e)
        {

        }

        private void CollisionType_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {

        }

        #region Window Maintnence
        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            thisHandle = (new WindowInteropHelper(this)).Handle;

            Interop.SetWindowListener(masterHandle, new int[] { 0x5, 0x06, 0x216, 0x07, 0x08 }, Dispatcher, ClingToMaster);
            Interop.SetToolWindowStyle(thisHandle);
            Interop.FocusWindow(masterHandle);
            this.ClingToMaster(0x05);
        }

        void ClingToMaster(uint msg)
        {
            if (Interop.IsWindow(masterHandle))
            {
                try
                {
                    switch (msg)
                    {
                        case 0x07:
                            this.Topmost = true;
                            this.Topmost = false;
                            Interop.SetForegroundWindow(masterHandle);
                            break;
                        case 0x08:
                            break;
                        default:
                            if (Interop.IsIconic(masterHandle))
                                this.WindowState = WindowState.Minimized;
                            else
                            {
                                this.WindowState = WindowState.Normal;
                                WindowRect r = Interop.GetWindowBounds(masterHandle);
                                Interop.SetWindowBounds(thisHandle, new WindowRect(r.X2, r.Y1, r.X2 + (int)this.Width, r.Y2));
                                this.Opacity = 1.0f;
                            }
                            break;
                    }
                }
                catch
                {
                    Dispatcher.InvokeShutdown();
                }
            }
        }
        #endregion
    }
}
