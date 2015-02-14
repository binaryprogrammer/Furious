using System;
using System.IO;
using System.Linq;
using System.Text;
using System.Timers;
using System.Collections.Generic;
using FuriousGameEngime_XNA4.Screens;
using FuriousGameEngime_XNA4.ModelManager;
using FuriousGameEngime_XNA4.GameEntities;
using FuriousGameEngime_XNA4.HelperClasses;
using FuriousGameEngime_XNA4.Screens.EditorScreens;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Jitter.Dynamics;
using Jitter.LinearMath;
using Jitter.Dynamics.Constraints.SingleBody;

namespace FuriousGameEngime_XNA4
{
    public enum EditorType
    {
        objectEditor,
        particleEditor,
        missionEditor,
        
        count
    }

    class Editor
    {
        #region Variables
        GameScreen _gameScreen;

        ContentManager content;

        /// <summary>
        /// arial spritefont 
        /// </summary>
        SpriteFont _debugFont;

        Texture2D _cursor;

        //the index of the model/object we will create or access from our list of models and modelNames
        int _selectedModel;

        /// <summary>
        /// A the model of the object that is selected
        /// </summary>
        ModelInstance _modelViewer;

        /// <summary>
        /// the object from the world we currently have selected
        /// </summary>
        EnviornmentEntity _selectedObject;

        float _distanceOfViewer = 25;
        float _distanceOfSelectedObject;

        JVector hitPoint, hitNormal;
        PointOnWorldPoint grabConstraint;
        RigidBody grabBody;

        TimeSpan timeSpan = new TimeSpan(0, 0, 2);

        internal bool rotateCameraThisTick = true;
        #endregion

        internal Editor(GameScreen gameScreen)
        {
            _gameScreen = gameScreen;
        }

        #region Load Content
        internal void LoadContent()
        {
            if (content == null)
                content = new ContentManager(_gameScreen.ScreenManager.Game.Services, "Content");
            
            _debugFont = content.Load<SpriteFont>("Fonts//gamefont");
            _cursor = content.Load<Texture2D>("MenuContent//cursor");
        }

        internal void UnloadContent()
        {
            content.Unload();
        }
        #endregion

        #region Getters and Setters

        #endregion

        #region Helper Functions
        /// <summary>
        /// Pointless?
        /// </summary>
        Vector3 RayTo(int x, int y)
        {
            Vector3 nearSource = new Vector3(x, y, 0);
            Vector3 farSource = new Vector3(x, y, 1);

            Matrix world = Matrix.Identity;

            Vector3 nearPoint = _gameScreen.GraphicsDevice.Viewport.Unproject(nearSource, _gameScreen.camera.Projection, _gameScreen.camera.View, world);
            Vector3 farPoint = _gameScreen.GraphicsDevice.Viewport.Unproject(farSource, _gameScreen.camera.Projection, _gameScreen.camera.View, world);

            Vector3 direction = farPoint - nearPoint;
            return direction;
        }
        
        /// <summary>
        /// pointless?
        /// </summary>
        bool RaycastCallback(RigidBody body, JVector normal, float fraction)
        {
            //if (body.IsStatic) return false;
            //else return true;

            if (body.IsGhost) return false;
            else return true;
        }

        /// <summary>
        /// Creates a _modelViewer
        /// </summary>
        void CreateViewer()
        {
            if (_modelViewer != null)
                _modelViewer.Dispose();

            _modelViewer = _gameScreen.modelManager.nameToModelBase[_gameScreen.ModelNames[_selectedModel]].CreateInstance();
            _modelViewer.body.IsGhost = true;
            _modelViewer.body.AffectedByGravity = false;
        }

        /// <summary>
        /// places new object with base properties and sets it to _selectedObject.
        /// </summary>
        void CreateObject()
        {
            if (_modelViewer != null)
            {
                //could set it to _selectedObject, Wouldn't hurt to have access to it when it's created
                _selectedObject = new EnviornmentEntity(_gameScreen, _gameScreen.modelManager.nameToModelBase[_gameScreen.ModelNames[_selectedModel]], _gameScreen.ModelNames[_selectedModel], Conversion.ToJitterVector(_gameScreen.camera.Position + _gameScreen.camera.Forward * _distanceOfViewer));
            }
            else
                CreateViewer();
        }

        /// <summary>
        /// Creates a new Object, sets it to _selectedObject and adds the editorMenuScreen
        /// </summary>
        void CustomCreateObject()
        {
            if (_modelViewer != null)
            {
                //creates a new object
                CreateObject();
                //removes viwer
                _modelViewer.Dispose();
                _modelViewer = null;
                //adds editor screen so player can customize stats
                _gameScreen.ScreenManager.AddScreen(new PhysicsMenuScreen(), null);
            }
            else
                CreateViewer();
        }

        /// <summary>
        /// checks to see if camera.Forward collides with any bodies
        /// </summary>
        void SelectObject()
        {
            _selectedObject = null;

            //JVector ray = Conversion.ToJitterVector(RayTo(input.CurrentMouseState.X, input.CurrentMouseState.Y)); //original
            JVector camForwardRay = Conversion.ToJitterVector(_gameScreen.camera.Forward);
            JVector camPos = Conversion.ToJitterVector(_gameScreen.camera.Position);

            camForwardRay = JVector.Normalize(camForwardRay) * 100; // not sure why the original had me multiply by 100;


            if (grabConstraint != null) // if there already exists a grab constraint we need to get rid of it
                _gameScreen.World.RemoveConstraint(grabConstraint);

            float fraction;
            _gameScreen.World.CollisionSystem.Raycast(camPos, camForwardRay, RaycastCallback, out grabBody, out hitNormal, out fraction);

            if (grabBody != null)
            {
                if (!grabBody.IsStatic)
                {
                    hitPoint = camPos + camForwardRay * fraction; //find the location of the point where we will create our grab constraint

                    JVector lAnchor = hitPoint - grabBody.Position; //anchor in local coordinates
                    lAnchor = JVector.Transform(lAnchor, JMatrix.Transpose(grabBody.Orientation));

                    //new physics constraint
                    grabConstraint = new PointOnWorldPoint(grabBody, lAnchor);
                    grabConstraint.Softness = .0001f;
                    grabConstraint.BiasFactor = 7f;
                    
                    _gameScreen.World.AddConstraint(grabConstraint); //add to world
                    //hitDistance = (Conversion.ToXNAVector(hitPoint) - _gameScreen.camera.Position).Length(); // how far the anchor is from the camera
                    grabConstraint.Anchor = hitPoint; //where to anchor the constraint in the world
                }

                if (grabBody.Tag != null)
                {
                    _selectedObject = (EnviornmentEntity)_gameScreen.GetEntityFromBody(grabBody);
                    _distanceOfSelectedObject = Vector3.Distance(_gameScreen.camera.Position, Conversion.ToXNAVector(_selectedObject.Position));
                    if (_modelViewer != null)
                    {
                        _modelViewer.Dispose();
                        _modelViewer = null;
                    }
                }
            }
        }
        #endregion

        #region Handle Input
        internal void HandleInput(InputState input, PlayerIndex playerIndex)
        {
            // Look up inputs for the specified player profile.
            KeyboardState keyboardState = input.CurrentKeyboardStates[(int)playerIndex];

            #region Keyboard Input
            if (keyboardState.IsKeyDown(Keys.LeftShift) || keyboardState.IsKeyDown(Keys.RightShift))// alternate use of same functionality
            {

            }
            else if (keyboardState.IsKeyDown(Keys.LeftControl) || keyboardState.IsKeyDown(Keys.RightControl)) //Editor Functionality
            {

            }
            else if (keyboardState.IsKeyDown(Keys.LeftAlt) || keyboardState.IsKeyDown(Keys.RightAlt)) //Core Engine Access
            {

            }
            else // basic functionality
            {
                if (input.KeyPressed(Keys.Space, null))
                {
                    //new Entity(_gameScreen, _gameScreen.modelManager.nameToModelBase[_gameScreen.ModelNames[_selectedObject]], "entity", Conversion.ToJitterVector(_gameScreen.camera.Position));
                    //_gameScreen.entities[_gameScreen.entities.Count - 1].Body.LinearVelocity = Conversion.ToJitterVector(_gameScreen.camera.Forward * 1000);
                }

                if (input.KeyPressed(Keys.Left, null))
                {
                    if (_modelViewer != null)
                    {
                        --_selectedModel;

                        if (_selectedModel < 0)
                        {
                            _selectedModel = _gameScreen.ModelNames.Length - 1;
                        }
                    }

                    CreateViewer();
                }
                else if (input.KeyPressed(Keys.Right, null))
                {
                    if (_modelViewer != null)
                    {
                        ++_selectedModel;

                        if (_selectedModel > _gameScreen.ModelNames.Length - 1)
                        {
                            _selectedModel = 0;
                        }
                    }

                    CreateViewer();
                }
            }
            #endregion

            #region Mouse Input
            //Shift - Alternate use of same functionality
            //Crtl - Editor Functionality
            //Alt - Core Engine Access

            if (input.MouseLeftPressed) //are placeing an object or grabbing an existing one?
            {
                //set timer
                timeSpan = TimeSpan.FromSeconds(.2);
                _selectedObject = null; // we need to set our previously selected object to null because we don't know if we are grabbing a new one.
            }
            else if (input.MouseRightPressed) //We are deleting an existing model and/or getting rid of the viewer
            {
                float fraction;
                _gameScreen.World.CollisionSystem.Raycast(Conversion.ToJitterVector(_gameScreen.camera.Position),
                    Conversion.ToJitterVector(_gameScreen.camera.Forward), RaycastCallback, out grabBody, out hitNormal, out fraction);

                if (grabBody != null && grabBody.Tag != null)
                {
                    _selectedObject = (EnviornmentEntity)_gameScreen.GetEntityFromBody(grabBody); // get object from the body we grabbed
                    _selectedObject.Dispose(); //dispose that entity
                    _selectedObject = null; // null this reference to this entity so the GC will get it.
                }

                if (_modelViewer != null)
                    _modelViewer.Dispose(); _modelViewer = null; // removes the viewer from the game
            }

            if (input.CurrentMouseState.LeftButton == ButtonState.Pressed && timeSpan <= TimeSpan.Zero) // we are holding down the mousebutton
            {
                if (_selectedObject != null) // if we have a selected object, it's time to drag it around screen.
                {
                    if (input.KeyDown(Keys.LeftShift, null) || input.KeyDown(Keys.RightShift, null))
                    {
                        if (_selectedObject.Body.IsStatic)
                            return; // we can't move static objects around with physics

                        if (grabBody != null)
                        {
                            grabConstraint.Anchor = Conversion.ToJitterVector(_gameScreen.camera.Position + _gameScreen.camera.Forward * _distanceOfSelectedObject);
                            grabBody.IsActive = true;
                        }
                    }
                    else if (input.KeyDown(Keys.Y, null)) // rolls the object selected without rotating the camera's view
                    {
                        _selectedObject.AddYawPitchRoll((_gameScreen.GraphicsDevice.Viewport.Width / 2 - input.CurrentMouseState.X) * .001f, 0, 0);
                        rotateCameraThisTick = false;
                    }
                    else if (input.KeyDown(Keys.U, null)) // rolls the object selected without rotating the camera's view
                    {
                        _selectedObject.AddYawPitchRoll(0, (_gameScreen.GraphicsDevice.Viewport.Width / 2 - input.CurrentMouseState.X) * .001f, 0);
                        rotateCameraThisTick = false;
                    }
                    else if (input.KeyDown(Keys.I, null)) // rolls the object selected without rotating the camera's view
                    {
                        _selectedObject.AddYawPitchRoll(0, 0, (_gameScreen.GraphicsDevice.Viewport.Width / 2 - input.CurrentMouseState.X) * .001f);
                        rotateCameraThisTick = false;
                    }
                    else if (input.KeyDown(Keys.Z, null))
                    {
                        //TODO: figure out how to scale bodies.
                    }
                    else // drag object around with you
                    {
                        _selectedObject.Body.IsStatic = true;
                        _selectedObject.Position = Conversion.ToJitterVector(_gameScreen.camera.Position + _gameScreen.camera.Forward * _distanceOfSelectedObject);
                    }

                    //TODO: Add Particles here 
                }
                else
                    SelectObject(); // we need to grab our object 
            }
            else if (input.CurrentMouseState.LeftButton == ButtonState.Released && timeSpan > TimeSpan.Zero)//we let go of the mouse button quickly, we want to create a new object
            {
                if (input.KeyDown(Keys.LeftShift, null) || input.KeyDown(Keys.RightShift, null)) //shift is held
                    CustomCreateObject(); //we want to create a custom attribute to the object we are placing
                else
                    CreateObject(); // we need to create a new object
                timeSpan = TimeSpan.Zero; // we set the time span to zero because there's no point in waiting for the timer we know we are going to create a new object. Also we don't want to create an object more than once.
            }
            else
            {
                grabBody = null; //remove the constraints if they exist
                if (grabConstraint != null) _gameScreen.World.RemoveConstraint(grabConstraint);
                if (_selectedObject != null) _selectedObject.Body.IsStatic = _selectedObject.ModelBase.bodyBase.IsStatic;
            }

            #endregion
            
            _distanceOfViewer -= input.MouseScrollWheel/ 10;

            if (_distanceOfViewer < 0)
            {
                _distanceOfViewer = 0;
            }            
        }
        #endregion

        #region Update
        internal void Update(GameTime gameTime)
        {
            timeSpan -= gameTime.ElapsedGameTime;

            if (_modelViewer != null)
                _modelViewer.body.Position = Conversion.ToJitterVector(_gameScreen.camera.Position + _gameScreen.camera.Forward * _distanceOfViewer);
        }
        #endregion

        #region Draw
        internal void Draw(GameTime gameTime, SpriteBatch spriteBatch)
        {
            spriteBatch.Begin();
            spriteBatch.Draw(_cursor, new Vector2(_gameScreen.GraphicsDevice.Viewport.Width / 2 - _cursor.Width / 2, _gameScreen.GraphicsDevice.Viewport.Height / 2 - _cursor.Height / 2), Color.White);
            spriteBatch.End();
        }
        #endregion

        //TODO: Particle Effects and Editor
        
        //TODO: Mission Editor

        //TODO: Pathfinding

        //TODO: AI

        //TODO Dialog tree
    }
}
