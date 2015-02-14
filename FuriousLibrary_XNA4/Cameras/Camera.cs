using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace FuriousLibrary_XNA4.Cameras
{
    public abstract class Camera
    {
        /// <summary>
        /// Determines: aspect ratio, field of view, min, max
        /// </summary>
        protected Matrix _projection;

        protected float _fieldOfView;
        protected float _defaultFieldOfView = 3f / 4f;

        protected float _aspectRatio;

        protected float _minViewDistance;

        protected float _maxViewDistance;

        protected int _zoomLevel;

        /// <summary>
        /// Dertermines: position, direction(facing), up
        /// </summary>
        protected Matrix _view;

        /// <summary>
        /// The position of the camera
        /// </summary>
        protected Vector3 _position;

        GraphicsDevice _graphicsDevice;

        /// <summary>
        /// Constructs a Camera
        /// </summary>
        /// <param name="graphicsDevice"></param>
        /// <param name="aspectRatio"></param>
        /// <param name="fieldOfView"></param>
        /// <param name="minimumViewDistance"></param>
        /// <param name="maximumViewDistance"></param>
        internal Camera(GraphicsDevice graphicsDevice, float aspectRatio, float fieldOfView, float minimumViewDistance, float maximumViewDistance)
        {
            _graphicsDevice = graphicsDevice;
            _aspectRatio = aspectRatio;
            _fieldOfView = fieldOfView;
            _minViewDistance = minimumViewDistance;
            _maxViewDistance = maximumViewDistance;
            _projection = Matrix.CreatePerspectiveFieldOfView(fieldOfView, aspectRatio, minimumViewDistance, maximumViewDistance);
        }

        /// <summary>
        /// Gets or sets the positioin of the camera
        /// </summary>
        public Vector3 Position
        {
            get
            {
                return _position;
            }
            set
            {
                _position = value;
                UpdateViewMatrix();
            }
        }

        /// <summary>
        /// Gets the matrix view of the camera
        /// </summary>
        public Matrix ViewMatrix
        {
            get
            {
                return _view;
            }
        }

        /// <summary>
        /// Gets the perspective of the camera
        /// </summary>
        public Matrix ProjectionMatrix
        {
            get
            {
                return _projection;
            }
        }

        public BoundingFrustum BoundingFrustum
        {
            get
            {
                return new BoundingFrustum(_view * _projection);
            }
        }

        public abstract Vector3 Forward { get; }

        public abstract Vector3 Up { get; }

        /// <summary>
        /// Zooms in on current view by 2x the current value (by restricting the <see cref="_fieldOfView"/>).
        /// </summary>
        public void ZoomPerspectiveIn()
        {
            _zoomLevel *= 2;
            _fieldOfView = _defaultFieldOfView / _zoomLevel;
            UpdateProjectionMatrix();
        }

        /// <summary>
        /// Zooms out on current view (by widening the <see cref="_fieldOfView"/>)
        /// </summary>
        public void ZoomPerspectiveOut()
        {
            _zoomLevel /= 2;
            _fieldOfView = _defaultFieldOfView / _zoomLevel;
            UpdateProjectionMatrix();
        }

        public Ray PickRay(Point mousePosition)
        {
            Vector3 nearPosition = _graphicsDevice.Viewport.Unproject(new Vector3(mousePosition.X, mousePosition.Y, 0),
                ProjectionMatrix, ViewMatrix, Matrix.Identity);
            Vector3 farPosition = _graphicsDevice.Viewport.Unproject(new Vector3(mousePosition.X, mousePosition.Y, 1),
                ProjectionMatrix, ViewMatrix, Matrix.Identity);

            Vector3 direction = Vector3.Normalize(farPosition - nearPosition);

            Ray ret = new Ray(nearPosition, direction);
            return ret;
        }

        protected virtual void UpdateProjectionMatrix()
        {
            _projection = Matrix.CreatePerspectiveFieldOfView(_fieldOfView, _aspectRatio, _minViewDistance, _maxViewDistance);
        }

        protected abstract void UpdateViewMatrix();
    }
}