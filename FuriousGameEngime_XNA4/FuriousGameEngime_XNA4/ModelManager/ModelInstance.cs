using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Jitter.Collision.Shapes;
using Jitter.LinearMath;
using Jitter.Dynamics;
using FuriousGameEngime_XNA4.GameEntities;
using FuriousGameEngime_XNA4.Screens;

namespace FuriousGameEngime_XNA4.ModelManager
{
    //public enum BodyTag { DrawMe, DontDrawMe }

    class ModelInstance
    {
        /// <summary>
        /// a reference to our game
        /// </summary>
        GameScreen _gameScreen;

        /// <summary>
        /// the base model we are an instance of
        /// </summary>
        internal readonly ModelBase modelBase;

        /// <summary>
        /// the world matrix for this model instance
        /// </summary>
        Matrix _transform;

        /// <summary>
        /// the physics body of this entity
        /// </summary>
        public readonly RigidBody body;

        /// <summary>
        /// an instance of the model base, a copy of the base model and placed in the world 
        /// </summary>
        /// <param name="gameScreen"></param>
        /// <param name="modelBase"></param>
        internal ModelInstance(GameScreen gameScreen, ModelBase modelBase)
        {
            _gameScreen = gameScreen;
            this.modelBase = modelBase;
            
            body = new RigidBody(modelBase.CollisionShape);

            body.AffectedByGravity = modelBase.bodyBase.AffectedByGravity;
            body.AllowDeactivation = modelBase.bodyBase.AllowDeactivation;
            body.DynamicFriction = modelBase.bodyBase.DynamicFriction;
            body.IsGhost = modelBase.bodyBase.IsGhost;
            body.IsStatic = modelBase.bodyBase.IsStatic;
            body.Restitution = modelBase.bodyBase.Restitution;
            body.StaticFriction = modelBase.bodyBase.StaticFriction;
            body.Tag = modelBase.name;

            gameScreen.World.AddBody(body);
        }
        
        internal Matrix Transform
        {
            get
            {
                ConvexHullShape hullShape = body.Shape as ConvexHullShape;

                _transform = Conversion.ToXNAMatrix(body.Orientation);

                // RigidBody.Position gives you the position of the center of mass of the shape.
                // This is not the center of our graphical represantion, use the "shift" property of the more complex shapes to deal with this.
                _transform.Translation = Conversion.ToXNAVector(body.Position + JVector.Transform(hullShape.Shift(), body.Orientation));

                return _transform;
            }
        }

        /// <summary>
        /// Disposes this instance
        /// </summary>
        internal void Dispose()
        {
            modelBase.RemoveInstance(this);
            _gameScreen.World.RemoveBody(body);
        }
    }
}
