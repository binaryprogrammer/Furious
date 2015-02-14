using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using FuriousGameEngime_XNA4.HelperClasses;
using Microsoft.Xna.Framework.Graphics;
using FuriousGameEngime_XNA4.Cameras;
using FuriousGameEngime_XNA4.Screens;
using Jitter.Collision.Shapes;
using Jitter.Dynamics;
using Jitter.LinearMath;
using FuriousGameEngime_XNA4.ModelManager;
using System.IO;

namespace FuriousGameEngime_XNA4.GameEntities
{
    public class Entity
    {
        #region Variables
        /// <summary>
        /// a reference to our game
        /// </summary>
        protected GameScreen _gameScreen;
        
        /// <summary>
        /// use the getter property
        /// </summary>
        ModelInstance _modelInstance;

        //public RigidBody body;

        /// <summary>
        /// the name of the entity derrived from the model name
        /// </summary>
        internal string name;

        #endregion

        public Entity(GameScreen gameScreen, ModelBase modelBase, string name, JVector position)
        {
            _gameScreen = gameScreen;
            this.name = name;
            gameScreen.AddToWorld(this);
            _modelInstance = modelBase.CreateInstance();

            _modelInstance.body.Position = position;

            //TODO: Create a custom model importer that calculates all the data needed for a physics body
            // Don't forget to share online!!! :)
        }

        public Entity(GameScreen gameScreen, BinaryReader reader)
        {
            _gameScreen = gameScreen;

            this.name = reader.ReadString();
            gameScreen.AddToWorld(this);
            _modelInstance = _gameScreen.modelManager.nameToModelBase[name].CreateInstance(); //read the name of the model

            Position = new JVector(reader.ReadSingle(), reader.ReadSingle(), reader.ReadSingle()); //read the position of the model

            Rotation = Conversion.ToJitterMatrix(Matrix.CreateFromQuaternion(new Quaternion(reader.ReadSingle(), reader.ReadSingle(), reader.ReadSingle(), reader.ReadSingle())));
        }

        #region Getters and Setters

        internal ModelInstance ModelInstance
        {
            get
            {
                return _modelInstance;
            }
        }

        internal ModelBase ModelBase
        {
            get
            {
                return _modelInstance.modelBase;
            }
        }

        internal RigidBody Body
        {
            get
            {
                return _modelInstance.body;
            }
        }

        internal JVector Position
        {
            get
            {
                return _modelInstance.body.Position;
            }
            set
            {
                _modelInstance.body.Position = value;
            }
        }

        internal JMatrix Rotation
        {
            get
            {
                return _modelInstance.body.Orientation;
            }
            private set
            {
                _modelInstance.body.Orientation = value;
            }
        }

        internal float Scale
        {
            get
            {
                return _modelInstance.body.Mass;
            }
            set
            {
                _modelInstance.body.Mass = value;
            }
        }
        #endregion
        
        #region Helper Functions
        internal void AddYawPitchRoll(float yaw, float pitch, float roll)
        {
            //Quaternion tempRotation = Quaternion.CreateFromYawPitchRoll(yaw, pitch, roll);
            //Rotation = Quaternion.Concatenate(tempRotation, Rotation);
            //Rotation.Normalize();

            Rotation *= JMatrix.CreateFromYawPitchRoll(yaw, pitch, roll);
        }

        internal void YawAroundLocalUp(float amount)
        {
            //Rotation *= Quaternion.CreateFromAxisAngle(Vector3.Up, amount);

            Rotation *= JMatrix.CreateFromAxisAngle(JVector.Up, amount);
        }      
        #endregion

        internal void Dispose()
        {
            _gameScreen.entitiesToBeDisposed.Add(this);
        }

        #region Update
        internal virtual void Update(GameTime gameTime)
        {
            //_modelInstance.body.Position = new JVector(500, 250, 500);
            //_modelInstance.body.LinearVelocity += new JVector(.5f, -1, .5f);
            //_modelInstance.body.AngularVelocity += new JVector(.001f, 0, .000f);
        }

        #endregion

        #region Draw

        internal void Draw(GameTime gameTime, SpriteBatch spriteBatch)
        {
            //TODO: we need lights and shadows
            //TODO: we need normal and spec maps

            //ConvexHullShape hullShape = body.Shape as ConvexHullShape;

            //Matrix world = Conversion.ToXNAMatrix(body.Orientation);

            //// RigidBody.Position gives you the position of the center of mass of the shape.
            //// This is not the center of our graphical represantion, use the "shift" property of the more complex shapes to deal with this.
            //world.Translation = Conversion.ToXNAVector(body.Position + JVector.Transform(hullShape.Shift(), body.Orientation));

            //Matrix[] boneTransforms = new Matrix[_modelInstance.Bones.Count];
            //_modelInstance.CopyAbsoluteBoneTransformsTo(boneTransforms);

            //foreach (ModelMesh mesh in _modelInstance.Meshes)
            //{
            //    foreach (BasicEffect effect in mesh.Effects)
            //    {
            //        effect.View = _gameScreen.camera.ViewMatrix;
            //        //effect.EnableDefaultLighting();
            //        effect.Projection = _gameScreen.camera.ProjectionMatrix;
            //        effect.World = boneTransforms[mesh.ParentBone.Index] * world;
            //    }
            //    mesh.Draw();
            //}

            //TODO: add a section for post render effects
        }
        #endregion

        internal virtual void Save(BinaryWriter writer)
        {

        }
    }
}
