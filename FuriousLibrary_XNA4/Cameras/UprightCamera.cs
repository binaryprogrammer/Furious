using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace FuriousLibrary_XNA4.Cameras
{
    /// <summary>
    /// http://www.xnawiki.com/index.php?title=Easy_Camera_Movement_Class
    /// </summary>
    public class UprightCamera : Camera
    {
        GraphicsDevice _graphicsDevice;
        
        /// <summary>
        /// This Vector3 holds the general camera movements
        /// </summary>
        public Vector3 cameraResult;

        /// <summary>
        /// This Vector3 holds the movements of the LookAt and only them.
        /// </summary>
        public Vector3 lookAtResult;
        
        /// <summary>
        /// What position the camera is looking at
        /// </summary>
        Vector3 _lookAtPosition;

        /// <summary>
        /// the camera's up
        /// </summary>
        Vector3 _up = Vector3.Up;

        /// <summary>
        /// This Vector3 defines the relative X-axis of the view (Forward).
        /// </summary>
        Vector3 RelativeX;

        /// <summary>
        /// The RelativeY axis holds the direction Up. Again it will be used for movements.
        /// </summary>
        Vector3 RelativeY;

        /// <summary>
        /// The RelativeZ axis holds the driection Right. It will be used for movements.
        /// </summary>
        Vector3 RelativeZ;
        
        /// <summary>
        /// AlphaY holds the rotation of RelativeX around the absolute Y-axis, starting at the absolute X-axis.
        /// </summary
        float AlphaY = 0.0f;
        
        /// <summary>
        /// Uses Martrices so it has clipping
        /// </summary>
        /// <param name="graphicsDevice"></param>
        /// <param name="aspectRatio"></param>
        /// <param name="fieldOfView"></param>
        /// <param name="minimumViewDistance"></param>
        /// <param name="maximumViewDistance"></param>
        public UprightCamera(GraphicsDevice graphicsDevice, float aspectRatio, float fieldOfView, float minimumViewDistance, float maximumViewDistance)
            : base(graphicsDevice, aspectRatio, fieldOfView, minimumViewDistance, maximumViewDistance)
        {
            _graphicsDevice = graphicsDevice;
            _projection = Matrix.CreatePerspectiveFieldOfView(fieldOfView, aspectRatio, minimumViewDistance, maximumViewDistance);
        }

        #region Getters and Setters
        
        /// <summary>
        /// Gets or sets the target position of the camera
        /// </summary>
        public Vector3 LookAtPosition
        {
            get
            {
                return _lookAtPosition;
            }
            set
            {
                _lookAtPosition = value;
                UpdateViewMatrix();
            }
        }

        public override Vector3 Up
        {
            get
            {
                return _up;
            }
        }

        public override Vector3 Forward
        {
            get { return Vector3.Normalize(_lookAtPosition - Position); }
        }
        #endregion

        #region Movement
        public void MoveX(float speed)
        {
            cameraResult = Forward * speed;
        }

        public void MoveY(float speed)
        {
            //reset Y
            RelativeY = new Vector3(0, 1, 0);

            RelativeY.X = -RelativeX.Y;
            RelativeY.Y = (float)Math.Sqrt(Math.Pow(RelativeX.X, 2f) + Math.Pow(RelativeX.Z, 2f));

            RelativeY = Vector3.Transform(RelativeY, Matrix.CreateRotationY(-AlphaY));

            RelativeY.Normalize();

            cameraResult = RelativeY * speed;
        }

        public void MoveZ(float speed)
        {
            RelativeZ.X = RelativeX.Z;
            RelativeZ.Y = 0.0f;
            RelativeZ.Z = -RelativeX.X;

            RelativeZ.Normalize();

            cameraResult = RelativeZ * speed;
        }
        #endregion

        #region Facing
        public void UpdateLook(float mouseMoveX, float mouseMoveY)
        {
            RelativeX = Forward;

            if (RelativeX.Z >= 0)
            {
                AlphaY = (float)Math.Atan2(RelativeX.Z, RelativeX.X);
            }
            else if (RelativeX.Z < 0)
            {
                AlphaY = (float)Math.Atan2(RelativeX.Z, RelativeX.X) + (2 * (float)Math.PI);
            }

            //AlphaZ holds the rotation of RelativeX around the RelativeZ axis (Right). RelativeZ will be defined later, based on RelativeX.
            float AlphaZ = -(float)Math.Atan(RelativeX.Y / (float)Math.Sqrt(Math.Pow(RelativeX.X, 2f) + Math.Pow(RelativeX.Z, 2f)));
            //The RelativeZ axis holds the driection Right. It will be used for movements.
            RelativeZ.X = RelativeX.Z;
            RelativeZ.Y = 0.0f;
            RelativeZ.Z = -RelativeX.X;

            RelativeZ.Normalize();

            // This float holds the Angle the camera has moved around the Z.axis since last frame.
            float AngleAddZ = 0;

            // This float holds the Angle the camera has moved around the Y.axis since last frame.
            float AngleAddY = 0;

            //This Vector holds the relative position of the LookAt based on the CameraPosition,
            //It will be importent to calculate the new LookAt position.
            Vector3 Before = _position - _lookAtPosition;

            //After is the vector hodling the new relative position of the LookAt.
            Vector3 After = Vector3.Zero;

            //This Vector will hold the movement of the LookAt that, when applied, will cause the LookAt to
            //rotate around the camera.
            Vector3 Rotation;


            AngleAddY = -(float)MathHelper.ToRadians(-mouseMoveX / 3);
            AngleAddZ = (float)MathHelper.ToRadians(-mouseMoveY / 3);

            Matrix RotationMatrix = Matrix.CreateFromAxisAngle(RelativeZ, AngleAddZ) * Matrix.CreateRotationY(AngleAddY);
            
            After = Vector3.Transform(Before, RotationMatrix);

            Rotation = Before - After;

            lookAtResult = Rotation;

            //after calculating new facing, update the view matrix
            UpdateViewMatrix();
        }
        #endregion

        public void SetUp(Vector3 value)
        {
            _up = value;
            UpdateViewMatrix();
        }

        protected override void  UpdateViewMatrix()
        {
            _position += cameraResult;
            _lookAtPosition += lookAtResult + cameraResult;
            _view = Matrix.CreateLookAt(_position, _lookAtPosition, _up);
        }
    }
}
