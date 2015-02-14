#region File Description
//-----------------------------------------------------------------------------
// Game.cs
//
// Microsoft XNA Community Game Platform
// Copyright (C) Microsoft Corporation. All rights reserved.
//-----------------------------------------------------------------------------
#endregion

#region Using Statements
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.GamerServices;
using Microsoft.Xna.Framework.Net;
using FuriousGameEngime_XNA4.Screens;
using System.Threading;
using System.Windows.Threading;
using System.Windows;
using System;
#endregion

namespace FuriousGameEngime_XNA4
{
    /// <summary>
    /// This is the main type for your game
    /// </summary>
    public class FuriousGame : Microsoft.Xna.Framework.Game
    {
        #region Fields

        GraphicsDeviceManager graphics;
        ScreenManager screenManager;


        // By preloading any assets used by UI rendering, we avoid framerate glitches
        // when they suddenly need to be loaded in the middle of a menu transition.
        static readonly string[] preloadAssets =
        {
            "gradient",
            "cat",
            "chat_ready",
            "chat_able",
            "chat_talking",
            "chat_mute",
        };
                
        #endregion

        #region Initialization
        
        /// <summary>
        /// The main game constructor.
        /// </summary>
        public FuriousGame()
        {
            Content.RootDirectory = "Content";

            graphics = new GraphicsDeviceManager(this);

            graphics.PreferredBackBufferWidth = 1067;
            graphics.PreferredBackBufferHeight = 600;

            graphics.GraphicsProfile = GraphicsProfile.HiDef;
            graphics.PreferMultiSampling = true;

#if PROFILE
            this.ScreenManager.Game.IsFixedTimeStep = false;
            this.ScreenManager.Game.SynchronizeWithVerticalRetrace = false;
#endif
            graphics.SynchronizeWithVerticalRetrace = true;
            this.IsFixedTimeStep = true;

            this.Window.AllowUserResizing = false;

            // Create components.
            screenManager = new ScreenManager(this);

            Components.Add(screenManager);
            Components.Add(new MessageDisplayComponent(this));
            Components.Add(new GamerServicesComponent(this));

            // Activate the first screens.
            screenManager.AddScreen(new BackgroundScreen(), null);
            screenManager.AddScreen(new MainMenuScreen(), null);
            
            // To test the trial mode behavior while developing your game,
            // uncomment this line:

            // Guide.SimulateTrialMode = true;
        }


        /// <summary>
        /// Loads graphics content.
        /// </summary>
        protected override void LoadContent()
        {
            foreach (string asset in preloadAssets)
            {
                Content.Load<object>("MenuContent//" + asset);
            }
        }


        #endregion

        #region Draw
        /// <summary>
        /// This is called when the game should draw itself.
        /// </summary>
        protected override void Draw(GameTime gameTime)
        {            
            // The real drawing happens inside the screen manager component.
            base.Draw(gameTime);
        }
        #endregion
    }
    
    #region Entry Point
    /// <summary>
    /// The main entry point for the application.
    /// </summary>
    static class Program
    {
#if WINDOWS
        private static Dispatcher _ctlDisp;
        private static ControlPanelProxy _proxy;
        private static ManualResetEvent _ready = new ManualResetEvent(false);

        public static ControlPanelProxy Proxy { get { return _proxy; } }
        public static Dispatcher Dispatcher { get { return _ctlDisp; } }

        private static void DoControlPanel(object o)
        {
            _ctlDisp = Dispatcher.CurrentDispatcher;
            Application ctlApp = new Application();

            ControlPanel panel = new ControlPanel((IntPtr)o);
            _proxy = new ControlPanelProxy(panel);

            _ready.Set(); // go!
            ctlApp.Run(panel);
        }

        private static void Main(string[] args)
        {
            using (FuriousGame game = new FuriousGame())
            {
                //starts the side window with the options
                //Thread ctlThread = new Thread(new ParameterizedThreadStart(DoControlPanel));
                //ctlThread.SetApartmentState(ApartmentState.STA);
                //ctlThread.Start(game.Window.Handle);

                //_ready.WaitOne();

                game.Run();
                //_proxy.Exit();
			}
		}
#else

        static void Main()
        {
            using (FuriousGame game = new FuriousGame())
            {
                game.Run();
            }
        }
    
#endif
    }
    #endregion
}