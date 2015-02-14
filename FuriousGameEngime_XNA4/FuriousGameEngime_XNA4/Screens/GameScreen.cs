#region Using Statements
using System;
using System.Threading;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Net;
using FuriousGameEngime_XNA4.GameEntities;
using System.Collections.Generic;
using FuriousGameEngime_XNA4.Cameras;
using Jitter;
using Jitter.Collision;
using System.Reflection;
using FuriousGameEngime_XNA4.Screens.GameScenes;
using Jitter.Collision.Shapes;
using Jitter.LinearMath;
using Jitter.Dynamics;
using FuriousGameEngime_XNA4.Screens.EditorScreens;
using FuriousGameEngime_XNA4.ModelManager;
using System.IO;
using FuriousGameEngime_XNA4.Enviornment;
using FuriousGameEngime_XNA4.HelperClasses;
#endregion

namespace FuriousGameEngime_XNA4.Screens
{
    public class GameScreen : Screen
    {
        #region Fields
        
        ContentManager content;
        SpriteFont gameFont;

        Random random = new Random();

        float pauseAlpha;

        public List<Scene> PhysicScenes { private set; get; }
        public World World { private set; get; }
        internal int currentScene = 0;

        internal QuaternionCamera camera;

        internal DebuggingInformation debug;

        internal bool editorMode = false;
        internal Editor editor;

        internal ModelsManager modelManager;

        string[] _modelNames;

        internal Terrain terrain;
        Effect _terrainEffect;
        #endregion

        #region Properties


        /// <summary>
        /// The logic for deciding whether the game is paused depends on whether
        /// this is a networked or single player game. If we are in a network session,
        /// we should go on updating the game even when the user tabs away from us or
        /// brings up the pause menu, because even though the local player is not
        /// responding to input, other remote players may not be paused. In single
        /// player modes, however, we want everything to pause if the game loses focus.
        /// </summary>
        new bool IsActive
        {
            get
            {
                // Pause behavior for single player games.
                return base.IsActive;
            }
        }


        #endregion

        #region entities
        /// <summary>
        /// A list of all entities
        /// </summary>
        internal List<Entity> entities = new List<Entity>();

        /// <summary>
        /// a list of all objects in the world
        /// </summary>
        internal List<EnviornmentEntity> enviornmentEntities = new List<EnviornmentEntity>();

        /// <summary>
        /// the list of all entites in active cells
        /// </summary>
        internal List<Entity> activeCellEntities = new List<Entity>();

        //TODO: List<Entity> releventMissionEntities = new List<Entity>();

        /// <summary>
        /// the entities to be disposed at the end of the tick
        /// </summary>
        internal List<Entity> entitiesToBeDisposed = new List<Entity>();

        //public Player player;

        public int AllEntitiesCount
        {
            get
            {
                return entities.Count;
            }
        }

        public int ActiveEntitiesCount
        {
            get
            {
                return activeCellEntities.Count;
            }
        }

        public void AddToWorld(Entity entity)
        {
            entities.Add(entity);

            if (entity is EnviornmentEntity)
            {
                enviornmentEntities.Add((EnviornmentEntity)entity);
            }
        }

        public void RemoveFromWorld(Entity entity)
        {
            entity.ModelInstance.Dispose();
            entities.Remove(entity);

            if (entity is EnviornmentEntity)
            {
                enviornmentEntities.Remove((EnviornmentEntity)entity);
            }
        }
        #endregion

        #region Load and Initilize

        /// <summary>
        /// Constructor.
        /// </summary>
        public GameScreen()
        {
            TransitionOnTime = TimeSpan.FromSeconds(1.5);
            TransitionOffTime = TimeSpan.FromSeconds(0.5);
        }

        /// <summary>
        /// Load graphics content for the game.
        /// </summary>
        public override void LoadContent()
        {
            if (content == null)
                content = new ContentManager(ScreenManager.Game.Services, "Content");

            this.ScreenManager.Game.IsMouseVisible = false;
            gameFont = content.Load<SpriteFont>("Fonts//gamefont");
            _terrainEffect = content.Load<Effect>("Effects//Terrain");

            camera = new QuaternionCamera(GraphicsDevice, MathHelper.PiOver4, 1.0f, 10000.0f);
            camera.Position = new Vector3(0, 0, 500);

            debug = new DebuggingInformation(this);
            debug.LoadContent();
            
            CollisionSystem collision = new CollisionSystemSAP();
            World = new World(collision); 
            World.AllowDeactivation = true;
            World.Gravity = new JVector(0, -100, 0);

            //needs World so that the bodies can be added
            modelManager = new ModelsManager(this, content);
            _modelNames = GetAllFileNames("Content", "*Model.xnb");

            for (int i = 0; i < _modelNames.Length; ++i)
            {
                string directory = Path.GetDirectoryName(_modelNames[i]);
                directory = directory.TrimStart("Content\\".ToCharArray());

                _modelNames[i] = directory + "\\" + Path.GetFileNameWithoutExtension(_modelNames[i]);

                modelManager.Load(_modelNames[i]);
            }

            //probably shouldn't load terrain here, what if the scene doesn't need terrain.
            terrain = new Terrain(this, content.Load<Model>("Terrain//heightmap"), content.Load<Texture2D>("Terrain//textureMap"), content.Load<Texture2D>("Terrain//Grass"), content.Load<Texture2D>("Terrain//Sand"), content.Load<Texture2D>("Terrain//Road"), "terrain", Vector3.Zero);
            Draw3DHelper.ChangeEffectUsedByModel(terrain.model, _terrainEffect);

            PhysicScenes = new List<Scene>();

            foreach (Type type in Assembly.GetExecutingAssembly().GetTypes())
            {
                if (type.Namespace == "FuriousGameEngime_XNA4.Screens.GameScenes" && !type.IsAbstract)
                {
                    Scene scene = (Scene)Activator.CreateInstance(type, this);
                    this.PhysicScenes.Add(scene);
                }
            }

            if (PhysicScenes.Count > 0)
                this.PhysicScenes[0].LoadContent(content);
                this.PhysicScenes[0].Build();

            editor = new Editor(this);
            editor.LoadContent();

            // once the load has finished, we use ResetElapsedTime to tell the game's
            // timing mechanism that we have just finished a very long frame, and that
            // it should not try to catch up.
            ScreenManager.Game.ResetElapsedTime();

            ScreenManager.Game.Window.Title = "Furious Game Engine";
        }

        internal string[] GetAllFileNames(string path, string filter)
        {
            List<string> files = new List<string>(Directory.GetFiles(path, filter));

            string[] directories = Directory.GetDirectories(path);
            for (int i = 0; i < directories.Length; ++i)
            {
                files.AddRange(GetAllFileNames(directories[i], filter));
            }
            return files.ToArray();
        }

        /// <summary>
        /// Unload graphics content used by the game.
        /// </summary>
        public override void UnloadContent()
        {
            content.Unload();
        }

        #region ResetScene
        //private void ResetScene()
        //{
        //    List<PhysicObject> toBeRemoved = new List<PhysicObject>();
        //    foreach (GameComponent gc in this.Components)
        //    {
        //        if (gc is PhysicObject && !(gc is HeightmapObject)
        //            && !(gc is CarObject) && !(gc is TriangleMeshObject)
        //            && !(gc is PlaneObject))
        //        {
        //            PhysicObject physObj = gc as PhysicObject;
        //            toBeRemoved.Add(physObj);
        //        }
        //    }

        //    foreach (PhysicObject physObj in toBeRemoved)
        //    {
        //        physObj.PhysicsBody.DisableBody();
        //        this.Components.Remove(physObj);

        //        //seems to be very important. Hold one of the demo keys and
        //        //watch your memory.
        //        physObj.Dispose();
        //    }

        //    int count = physicSystem.Controllers.Count;
        //    for (int i = 0; i < count; i++) physicSystem.Controllers[0].DisableController();
        //    count = physicSystem.Constraints.Count;
        //    for (int i = 0; i < count; i++) physicSystem.RemoveConstraint(physicSystem.Constraints[0]);

        //    //count = physicSystem.Constraints.Count;

        //    //for (int i = 0; i < count; i++)
        //    //    physicSystem.Controllers[0].DisableController();

        //    //count = physicSystem.Constraints.Count;

        //    //for (int i = 0; i < count; i++)
        //    //    physicSystem.Constraints[0].DisableConstraint;


        //}
        #endregion
        
        #endregion

        #region Getters and Setters
        public GraphicsDevice GraphicsDevice
        {
            get
            {
                return ScreenManager.GraphicsDevice;
            }
        }

        public string[] ModelNames
        {
            get
            {
                return _modelNames;
            }
        }

        #endregion

        #region Helper Functions

        /// <summary>
        /// Removes everything from entities
        /// </summary>
        public void PurgeLevel()
        {
            while (entities.Count > 0)
            {
                RemoveFromWorld(entities[0]);
            }
        }

        /// <summary>
        /// clears all entites that have been removed.
        /// </summary>
        public void PurgeGarbage()
        {
            while (entitiesToBeDisposed.Count > 0)
            {
                RemoveFromWorld(entitiesToBeDisposed[0]);
                entitiesToBeDisposed.RemoveAt(0);
            }
        }

        public Entity GetEntityFromBody(RigidBody body)
        {
            for (int i = 0; i < entities.Count; ++i)
            {
                if (entities[i].Body == body)
                {
                    return entities[i];
                }
            }

            throw new Exception("No entity has the following body: " + body);
        }
        #endregion

        #region Update

        /// <summary>
        /// Updates the state of the game.
        /// </summary>
        public override void Update(GameTime gameTime, bool otherScreenHasFocus, bool coveredByOtherScreen)
        {
            base.Update(gameTime, otherScreenHasFocus, false);

            // Gradually fade in or out depending on whether we are covered by the pause screen.
            if (coveredByOtherScreen)
                pauseAlpha = Math.Min(pauseAlpha + 1f / 32, 1);
            else
                pauseAlpha = Math.Max(pauseAlpha - 1f / 32, 0);

            if (IsActive)
            {
                float step = (float)gameTime.ElapsedGameTime.TotalSeconds;
                if (step > 1.0f / 60.0f) step = 1.0f / 60.0f;

                World.Step(step, true);

                if (editorMode)
                {
                    editor.Update(gameTime);
                    debug.Update(gameTime);
                }

                for (int i = 0; i < entities.Count; ++i)
                {
                    entities[i].Update(gameTime);
                }

                PurgeGarbage();
            }
        }
        #endregion

        #region Handle Input
        /// <summary>
        /// Lets the game respond to player input. Unlike the Update method,
        /// this will only be called when the gameplay screen is active.
        /// </summary>
        public override void HandleInput(InputState input)
        {
            if (input == null)
                throw new ArgumentNullException("input");

            if (ControllingPlayer.HasValue)
            {
                // In single player games, handle input for the controlling player.
                HandlePlayerInput(input, ControllingPlayer.Value);
                if (editorMode)
                {
                    debug.HandleInput(input, ControllingPlayer.Value);
                    editor.HandleInput(input, ControllingPlayer.Value);
                }
            }
        }

        /// <summary>
        /// Handles input for the specified player. In local game modes, this is called
        /// just once for the controlling player. In network modes, it can be called
        /// more than once if there are multiple profiles playing on the local machine.
        /// Returns true if we should continue to handle input for subsequent players,
        /// or false if this player has paused the game.
        /// </summary>
        bool HandlePlayerInput(InputState input, PlayerIndex playerIndex)
        {
            // Look up inputs for the specified player profile.
            KeyboardState keyboardState = input.CurrentKeyboardStates[(int)playerIndex];
            GamePadState gamePadState = input.CurrentGamePadStates[(int)playerIndex];
            MouseState mouseState = Mouse.GetState();

            // The game pauses either if the user presses the pause button, or if
            // they unplug the active gamepad. This requires us to keep track of
            // whether a gamepad was ever plugged in, because we don't want to pause
            // on PC if they are playing with a keyboard and have no gamepad at all!
            bool gamePadDisconnected = !gamePadState.IsConnected && input.GamePadWasConnected[(int)playerIndex];

            if ((input.IsPauseGame(playerIndex) && !editorMode) || gamePadDisconnected)
            {
                ScreenManager.AddScreen(new PauseMenuScreen(), playerIndex);
                return false;
            }
            else if (input.IsPauseGame(playerIndex) && editorMode)
            {
                ScreenManager.AddScreen(new EditorMenuScreen(editor), playerIndex);
                return false;
            }

            if (keyboardState.IsKeyDown(Keys.LeftShift) || keyboardState.IsKeyDown(Keys.RightShift))
            {
                if (keyboardState.IsKeyDown(Keys.W))
                {
                    camera.Position += camera.Forward;
                }
                if (keyboardState.IsKeyDown(Keys.A))
                {
                    camera.Position += -camera.Up;
                }
                if (keyboardState.IsKeyDown(Keys.S))
                {
                    camera.Position += -camera.Forward;
                }
                if (keyboardState.IsKeyDown(Keys.D))
                {
                    camera.Position += camera.Up;
                }
            }
            else if (keyboardState.IsKeyDown(Keys.LeftControl) || keyboardState.IsKeyDown(Keys.RightControl))
            {
                if (input.KeyPressed(Keys.S, null))
                {
                    FileStream fileStream = new FileStream(content.RootDirectory + "//savedLevel.lvl", FileMode.Create);
                    BinaryWriter writer = new BinaryWriter(fileStream);
                    Save(writer);
                    writer.Flush();
                    writer.Close();
                }

                if (input.KeyPressed(Keys.L, null))
                {
                    LoadLevel("savedLevel.lvl");
                }
            }
            else if (keyboardState.IsKeyDown(Keys.LeftAlt) || keyboardState.IsKeyDown(Keys.RightAlt))
            {

            }
            else
            {
                if (keyboardState.IsKeyDown(Keys.W))
                {
                    camera.Position += camera.Forward * 10;
                }
                if (keyboardState.IsKeyDown(Keys.A))
                {
                    camera.Position += camera.Up * -10;
                }
                if (keyboardState.IsKeyDown(Keys.S))
                {
                    camera.Position += camera.Forward * -10;
                }
                if (keyboardState.IsKeyDown(Keys.D))
                {
                    camera.Position += camera.Up * 10;
                }

                if (input.KeyPressed(Keys.E, null))
                {
                    editorMode = !editorMode;
                }
            }

            if (editor.rotateCameraThisTick)
                camera.AddYawPitchRoll((GraphicsDevice.Viewport.Width / 2 - mouseState.X) * .001f, (GraphicsDevice.Viewport.Height / 2 - mouseState.Y) * .001f, 0);
            Mouse.SetPosition(GraphicsDevice.Viewport.Width / 2, GraphicsDevice.Viewport.Height / 2);
            camera.AutoUp(Vector3.Up);
            editor.rotateCameraThisTick = true;
            return true;
        }
        #endregion

        #region Draw
        /// <summary>
        /// Draws the gameplay screen.
        /// </summary>
        public override void Draw(GameTime gameTime)
        {
            // This game has a blue background. Why? Because!
            ScreenManager.GraphicsDevice.Clear(ClearOptions.Target, Color.CornflowerBlue, 0, 0);

            GraphicsDevice.BlendState = BlendState.Opaque;
            GraphicsDevice.DepthStencilState = DepthStencilState.Default;

            // Our player and enemy are both actually just text strings.
            SpriteBatch spriteBatch = ScreenManager.SpriteBatch;

            // If the game is transitioning on or off, fade it out to black.
            if (TransitionPosition > 0 || pauseAlpha > 0)
            {
                float alpha = MathHelper.Lerp(1f - TransitionAlpha, 1f, pauseAlpha / 2);

                ScreenManager.FadeBackBufferToBlack(alpha);
            }

            terrain.Draw(camera, Vector3.One);

            PhysicScenes[currentScene].Draw(gameTime, spriteBatch);

            modelManager.Draw(camera);
            if (editorMode)
            {
                editor.Draw(gameTime, spriteBatch);
                debug.Draw(gameTime, spriteBatch);
            }
        }
        #endregion

        #region Saving and Loading
        //TODO: Saving and loading should be dynamic
        // Possobly ask for a file to load at start, or create a new one.
        // Select the terrain to be loaded as the terrain for that particular level.

        internal void Save(BinaryWriter writer)
        {
            writer.Write(enviornmentEntities.Count); //environment entities
            for (int i = 0; i < enviornmentEntities.Count; ++i)
            {
                enviornmentEntities[i].Save(writer);
            }
        }

        internal void LoadLevel(string levelFileName)
        {
            PurgeLevel();

            FileStream fileStream = new FileStream(content.RootDirectory + "//" + levelFileName, FileMode.Open);
            BinaryReader reader = new BinaryReader(fileStream);

            int entityCount = reader.ReadInt32();
            for (int i = 0; i < entityCount; ++i)
            {
                new EnviornmentEntity(this, reader);
            }

            reader.Close();
        }

        #endregion

        //TODO: I could build a GPU based random map generation for the open source engine        
        // Each level file is a new area that can be loaded.
        // Load points can be scripted into the game based on their file names.
        // Heads up could cause a crash if the level file is deleted or renamed.
    }
}
