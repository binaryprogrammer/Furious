using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;
using System.Diagnostics;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Input;
using FuriousGameEngime_XNA4.GameEntities;
using Jitter.LinearMath;
using Jitter;
using Jitter.Collision;
using Jitter.Dynamics;

namespace FuriousGameEngime_XNA4.Screens
{
    class DebuggingInformation
    {
        ContentManager content;

        /// <summary>
        /// arial spritefont 
        /// </summary>
        SpriteFont _debugFont;

        /// <summary>
        /// Debugging information
        /// </summary>
        public static StringBuilder text = new StringBuilder();

        /// <summary>
        /// to draw or not to draw, that is the question
        /// </summary>
        bool _viewDebuggingInformation;

        /// <summary>
        /// keeps track of the number of polys drawn in the last tick
        /// </summary>
        public static int polygonCount = 0;

        /// <summary>
        /// a reference to the game we are debugging
        /// </summary>
        GameScreen _gameScreen;

        private int frameRate = 0;
        private int frameCounter = 0;
        private TimeSpan elapsedTime = TimeSpan.Zero;

        bool showPhysicsInfo = false;
        bool showSceneInfo = false;
        bool showEngineInfo = false;

        internal DebuggingInformation(GameScreen gameScreen)
        {
            _gameScreen = gameScreen;
        }
        
        #region Initialization
        /// <summary>
        /// Load graphics content for the game.
        /// </summary>
        public void LoadContent()
        {
            if (content == null)
                content = new ContentManager(_gameScreen.ScreenManager.Game.Services, "Content");

            _debugFont = content.Load<SpriteFont>("Fonts//gamefont");
            
            DebugShapeRenderer.Initialize(_gameScreen.GraphicsDevice);
        }

        /// <summary>
        /// Unload graphics content used by the game.
        /// </summary>
        public void UnloadContent()
        {
            content.Unload();
        }
        #endregion

        #region Helper Functions
        void AddPhysicsInformation()
        {
            int entries = (int)Jitter.World.DebugType.Num;
            double total = 0;

            for (int i = 0; i < entries; i++)
            {
                World.DebugType type = (World.DebugType)i;

                text.Append(type.ToString() + ": " + ((double)_gameScreen.World.DebugTimes[i]).ToString("0.00"));
                text.AppendLine();

                total += _gameScreen.World.DebugTimes[i];
            }

            text.AppendLine();
            text.Append("Total Physics Time: " + total.ToString("0.00"));
            text.AppendLine();
            text.Append("Physics Framerate: " + (1000.0d / total).ToString("0") + " fps");
            text.AppendLine();
        }

        void AddSceneInformation()
        {
            //Collects all the information to draw to the screen
            text.Append(_gameScreen.World.CollisionSystem);
            text.AppendLine();

            string scene = _gameScreen.PhysicScenes[_gameScreen.currentScene].ToString();
            scene.TrimStart(".".ToCharArray()); // should remove most of the usless parts of the string, but it's not. Wonder why?
            text.Append("Current Scene: " + scene);
            text.AppendLine();
            text.Append("Island count: " + _gameScreen.World.Islands.Count);
            text.AppendLine();
            text.Append("Body count: " + _gameScreen.World.RigidBodies.Count);
            text.AppendLine();
            text.Append("Entites: " + _gameScreen.entities.Count);
            text.AppendLine();
            text.Append("Camera Position: " + _gameScreen.camera.Position);
            text.AppendLine();
        }

        void AddEngineInformation()
        {
            text.Append("gen0: " + GC.CollectionCount(0) + "  gen1: " + GC.CollectionCount(1) + "  gen2: " + GC.CollectionCount(2));
            text.AppendLine();
        }
        #endregion

        #region Handle Input

        /// <summary>
        /// Handles input for the specified player. In local game modes, this is called
        /// just once for the controlling player. In network modes, it can be called
        /// more than once if there are multiple profiles playing on the local machine.
        /// Returns true if we should continue to handle input for subsequent players,
        /// or false if this player has paused the game.
        /// </summary>
        internal bool HandleInput(InputState input, PlayerIndex playerIndex)
        {
            // Look up inputs for the specified player profile.
            KeyboardState keyboardState = input.CurrentKeyboardStates[(int)playerIndex];

            if (keyboardState.IsKeyDown(Keys.LeftAlt) || keyboardState.IsKeyDown(Keys.RightAlt))
            {
                if (input.KeyPressed(Keys.V, null))
                {
                    _viewDebuggingInformation = !_viewDebuggingInformation;
                }

                if (input.KeyPressed(Keys.D1, null))
                {
                    showSceneInfo = !showSceneInfo;
                }
                else if (input.KeyPressed(Keys.D2, null))
                {
                    showPhysicsInfo = !showPhysicsInfo;
                }
                else if (input.KeyPressed(Keys.D3, null))
                {
                    showEngineInfo = !showEngineInfo;
                }
            }

            return true;
        }
        #endregion

        #region Update

        /// <summary>
        /// Updates the state of the game.
        /// </summary>
        public void Update(GameTime gameTime)
        {
            elapsedTime += gameTime.ElapsedGameTime;

            if (elapsedTime > TimeSpan.FromSeconds(1))
            {
                elapsedTime -= TimeSpan.FromSeconds(1);
                frameRate = frameCounter;
                frameCounter = 0;
            }
        }
        #endregion

        #region Draw

        /// <summary>
        /// Displays debugging information
        /// </summary>
        void DrawDebuggingText(SpriteBatch spriteBatch)
        {
            #region Old debugging information
            //_debuggingInformation.Append("Camera's local X and Z position on terrain: " + new Vector2(_gameScreen.camera.Position.X - _gameScreen.terrain.mapInfo.heightmapPosition.X, _gameScreen.camera.Position.Z - _gameScreen.terrain.mapInfo.heightmapPosition.Z));
            //_debuggingInformation.AppendLine();

            //_debuggingInformation.Append("Undos Left: " + _undo.Count());
            //_debuggingInformation.AppendLine();
            //_debuggingInformation.Append("Spatial Cell Count: " + _gameScreen.spatialPartitioning.collisionCells.Count);
            //_debuggingInformation.AppendLine();

            //_debuggingInformation.Append("Player's Position: " + _gameScreen.player.Position);
            //_debuggingInformation.AppendLine();
            //_debuggingInformation.Append("Player's Cell(s): ");
            //for (int i = 0; i < _gameScreen.player.Tag.keys.Count; ++i)
            //{
            //    _debuggingInformation.Append(_gameScreen.player.Tag.keys[i]);
            //    if (_gameScreen.player.Tag.keys.Count > 1 && i < _gameScreen.player.Tag.keys.Count - 1)
            //    {
            //        _debuggingInformation.Append("  +  ");
            //    }
            //}
            //_debuggingInformation.AppendLine();
            //_debuggingInformation.Append("Active Cell Entity Count: " + _gameScreen.activeCellEntities.Count);
            //_debuggingInformation.AppendLine();
            #endregion

            if (showSceneInfo) AddSceneInformation();
            if (showPhysicsInfo) AddPhysicsInformation();
            if (showEngineInfo) AddEngineInformation();

            //Draws all the debugging information
            spriteBatch.Begin();
            spriteBatch.DrawString(_debugFont, text, new Vector2(1, 30), Color.Black);
            spriteBatch.DrawString(_debugFont, text, new Vector2(0, 29), Color.White);
            spriteBatch.End();

            //Clears all the old informations so when it recollects the infromation wont stack and will be up to date
            text.Clear();
        }

        public void Draw(GameTime gameTime, SpriteBatch spriteBatch)
        {
            frameCounter++;
            string fps = frameRate.ToString();

            spriteBatch.Begin();

            spriteBatch.DrawString(_debugFont, fps, new Vector2(_gameScreen.GraphicsDevice.Viewport.Width - 50, 0), Color.Black);
            spriteBatch.DrawString(_debugFont, fps, new Vector2(_gameScreen.GraphicsDevice.Viewport.Width - 49, 1), Color.White);

            spriteBatch.End();

            if (!_viewDebuggingInformation)
                return;

            #region failed draw of collision mesh
            //GraphicsDevice device = _gameScreen.GraphicsDevice;
            //BasicEffect effect = new BasicEffect(device);

            //device.BlendState = BlendState.Opaque;
            //device.DepthStencilState = DepthStencilState.Default;

            //RasterizerState rs = new RasterizerState();
            //rs.CullMode = CullMode.None;
            //rs.FillMode = FillMode.WireFrame;
            //device.RasterizerState = rs;
            
            //effect.CurrentTechnique = effect.Techniques["BasicEffect"];
            //effect.Projection = _gameScreen.camera.ProjectionMatrix;
            //effect.View = _gameScreen.camera.ViewMatrix;
            //effect.DiffuseColor = Vector3.Zero;

            //for (int e = 0; e < _gameScreen.entities.Count; ++e)
            //{
            //    effect.World = _gameScreen.entities[e].ModelInstance.Transform;

            //    PhysicsMesh mesh = _gameScreen.entities[e].ModelInstance.modelBase.model.Tag as PhysicsMesh;

            //    short[] indices = mesh.indices.ToArray();

            //    Vector3[] vertices = mesh.vertices.ToArray();

            //    #region nothing
            //    //VertexPositionColor[] vertices = new VertexPositionColor[mesh.vertices.Count];

            //    //for (int v = 0; v < vertices.Length; ++v)
            //    //{
            //    //    vertices[v].Color = Color.DimGray;
            //    //    vertices[v].Position = mesh.vertices[v];
            //    //}
            //    #endregion

            //    foreach (EffectPass pass in effect.CurrentTechnique.Passes)
            //    {
            //        pass.Apply();

            //        device.DrawUserIndexedPrimitives(PrimitiveType.TriangleList, vertices, 0, vertices.Count(), indices.ToArray(), 0, indices.Length / 3, VertexPositionColor.VertexDeclaration);
            //    }
            //}

            //rs = new RasterizerState();
            //rs.CullMode = CullMode.CullClockwiseFace;
            //rs.FillMode = FillMode.Solid;
            //device.RasterizerState = rs;
            #endregion

            spriteBatch.Begin();
            spriteBatch.DrawString(_debugFont, "Poly Count This frame: " + polygonCount.ToString(), new Vector2(0, 0), Color.Black);            
            spriteBatch.End();

            polygonCount = 0;

            JBBox box;

            foreach (RigidBody body in _gameScreen.World.RigidBodies)
            {
                box = body.BoundingBox;

                DebugShapeRenderer.AddBoundingBox(new BoundingBox(Conversion.ToXNAVector(box.Min), Conversion.ToXNAVector(box.Max)), Color.Black);
            }

            foreach (CollisionIsland island in _gameScreen.World.Islands)
            {
                box = JBBox.SmallBox;

                foreach (RigidBody body in island.Bodies)
                {
                    box = JBBox.CreateMerged(box, body.BoundingBox);
                }

                DebugShapeRenderer.AddBoundingBox(new BoundingBox(Conversion.ToXNAVector(box.Min), Conversion.ToXNAVector(box.Max)), island.IsActive() ? Color.Red : Color.Green);
            }

            //temp testing
            //for (int x = 0; x < 127; ++x)
            //{
            //    for (int z = 0; z < 127; ++z)
            //    {
            //        DebugShapeRenderer.AddBoundingSphere(new BoundingSphere(new Vector3(x * _gameScreen.terrain.mapInfo.scale * 4, _gameScreen.terrain.shapeHeights[x,z], z * _gameScreen.terrain.mapInfo.scale * 4), 3), Color.Black);
            //    }
            //}

            DebugShapeRenderer.Draw(gameTime, _gameScreen.camera.View, _gameScreen.camera.Projection);

            DrawDebuggingText(spriteBatch);
        }
        #endregion
    }
}
