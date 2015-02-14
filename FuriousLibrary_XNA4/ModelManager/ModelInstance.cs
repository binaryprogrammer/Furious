using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using FuriousLibrary_XNA4.Collision;

namespace FuriousLibrary_XNA4.ModelManager
{
    class ModelInstance
    {
        /// <summary>
        /// a reference to our game
        /// </summary>
        Game _game;

        /// <summary>
        /// the base model we are an instance of
        /// </summary>
        internal readonly ModelBase modelBase;

        /// <summary>
        /// the position of this instance
        /// </summary>
        Vector3 _position;

        /// <summary>
        /// the scale of this instance
        /// </summary>
        float _scale = 1;

        /// <summary>
        /// The rotation of this instance
        /// </summary>
        Quaternion _rotation = Quaternion.Identity;

        /// <summary>
        /// the world matrix for this model instance
        /// </summary>
        Matrix _transform;

        /// <summary>
        /// the offset of this model instance
        /// </summary>
        internal Vector3 offset = Vector3.Zero;

        internal ModelInstance(Game game, ModelBase model)
        {
            _game = game;
            this.modelBase = model;
        }

        /// <summary>
        /// 
        /// </summary>
        internal BoundingOrientedBox OrientedBoundingBox
        {
            get
            {
                BoundingOrientedBox transformed = modelBase.OrientedBoundingBox;
                //because the center is pulled from the center of the base model class I should be good
                transformed.Center = Vector3.Transform(modelBase.OrientedBoundingBox.Center, Transform);
                //this scale and rotation is unique for each instance it is derived from the scale and rotation of the base model
                transformed.HalfExtent *= Scale;
                transformed.Orientation *= Rotation;
                return transformed;
            }
        }

        internal Matrix Transform
        {
            get
            {
                //return Matrix.CreateFromQuaternion(_rotation) * Matrix.CreateScale(_scale) * Matrix.CreateTranslation(_position);
                return _transform * Matrix.CreateTranslation(offset);
            }
        }

        public Vector3 Position
        {
            get { return _position; }
            set
            {
                _position = value;
                _transform = Matrix.CreateFromQuaternion(_rotation) * Matrix.CreateScale(_scale) * Matrix.CreateTranslation(_position);
                modelBase.UpdateDrawTransforms(this);
            }
        }

        internal Quaternion Rotation
        {
            get { return _rotation; }
            set
            {
                _rotation = value;
                _transform = Matrix.CreateFromQuaternion(_rotation) * Matrix.CreateScale(_scale) * Matrix.CreateTranslation(_position);
                modelBase.UpdateDrawTransforms(this);
            }
        }

        public float Scale
        {
            get { return _scale; }
            set
            {
                _scale = value;
                _transform = Matrix.CreateFromQuaternion(_rotation) * Matrix.CreateScale(_scale) * Matrix.CreateTranslation(_position);
                modelBase.UpdateDrawTransforms(this);
            }
        }

        public Vector3 Forward
        {
            get
            {
                return Vector3.Transform(Vector3.Forward, Rotation);
            }
        }

        public Vector3 Left
        {
            get
            {
                return Vector3.Transform(Vector3.Left, Rotation);
            }
        }

        public Vector3 Up
        {
            get
            {
                return Vector3.Transform(Vector3.Up, Rotation);
            }
        }

        /// <summary>
        /// Disposes this instance
        /// </summary>
        internal void Dispose()
        {
            if (!modelBase.instances.Remove(this))
            {
                return;
                //throw new Exception("Couldn't dispose " + this);
            }
            modelBase.UpdateDrawTransforms();
        }
    }
}
