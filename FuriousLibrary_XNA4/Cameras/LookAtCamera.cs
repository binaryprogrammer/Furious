using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace FuriousLibrary_XNA4.Cameras
{
    public class LookAtCamera : Camera
    {
        /// <summary>
        /// The game's GraphicsDevice
        /// </summary>
        GraphicsDevice _graphicsDevice;

        /// <summary>
        /// What position the camera is looking at
        /// </summary>
        Vector3 _lookAtPosition;

        /// <summary>
        /// Directional vector representing the camera's up
        /// </summary>
        Vector3 _up = Vector3.Up;

        /// <summary>
        /// A target that orbits the camera
        /// </summary>
        Vector3 _point = Vector3.One;

        /// <summary>
        /// the rotation of the <see cref="_point"/>
        /// </summary>
        Matrix rotation = Matrix.Identity;

        /// <summary>
        /// This Vector3 defines the relative X-axis of the view (Forward).
        /// </summary>
        Vector3 RelativeX;

        /// <summary>
        /// The RelativeZ axis holds the driection Right. It will be used for movements.
        /// </summary>
        Vector3 RelativeZ;

        /// <summary>
        /// AlphaY holds the rotation of RelativeX around the absolute Y-axis, starting at the absolute X-axis.
        /// </summary
        float AlphaY = 0.0f;

        //Quaternion quaternion = Quaternion.Identity;

        /// <summary>
        /// Just looks at a given position
        /// </summary>
        /// <param name="graphicsDevice"></param>
        /// <param name="aspectRatio"></param>
        /// <param name="fieldOfView"></param>
        /// <param name="minimumViewDistance"></param>
        /// <param name="maximumViewDistance"></param>
        public LookAtCamera(GraphicsDevice graphicsDevice, float aspectRatio, float fieldOfView, float minimumViewDistance, float maximumViewDistance)
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

        /// <summary>
        /// how far the camera is to it's look at position
        /// </summary>
        //public float DistanceSquaredToTarget
        //{
        //    get
        //    {
        //        return Vector3.DistanceSquared(_lookAtPosition, _position);
        //    }
        //    set
        //    {
        //        _position = value * -Forward;
        //    }
        //}
        
        public override Vector3 Forward
        {
            get
            {
                return Vector3.Normalize(_lookAtPosition - Position);
            }
        }
        
        /// <summary>
        /// Gets or sets the up directional vector on the camera
        /// </summary>
        public override Vector3 Up
        {
            get
            {
                return _up;
            }
        }
        #endregion

        public void SetUp(Vector3 value)
        {
            _up = value;
            UpdateViewMatrix();
        }

        /// <summary>
        /// Fucking Bullshit. Has lots of phantom movement
        /// </summary>
        /// <param name="mouseMoveX"></param>
        /// <param name="mouseMoveY"></param>
        /// <param name="sensitivity"></param>
        /// <returns></returns>
        public Vector3 LookAtPoint(float mouseMoveX, float mouseMoveY, float sensitivity)
        {
            //_point = _position;

            //Quaternion tempQuaternion = Quaternion.CreateFromYawPitchRoll(mouseMoveY, mouseMoveX, 0);
            //quaternion = Quaternion.Concatenate(tempQuaternion, quaternion);
            //quaternion.Normalize();


            ////Matrix translation = Matrix.CreateTranslation(_position);
            //Matrix rotation = Matrix.CreateFromQuaternion(quaternion);

            //rotation *= Matrix.CreateFromAxisAngle(rotation.Forward, amount);

            //return _point = Vector3.Transform(_point, rotation);

            //Matrix tempRotation = Matrix.CreateFromYawPitchRoll(-mouseMoveY, -mouseMoveX, 0);
            //Matrix.Negate(ref tempRotation, out tempRotation);
            //Matrix.Multiply(ref tempRotation, ref this.rotation, out this.rotation);
            //return _point = Vector3.Transform(_point, Matrix.Multiply(Matrix.CreateTranslation(_point), this.rotation));

            if (mouseMoveX != 0 || mouseMoveY != 0)
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

                //_point = Rotation;
                _point += Rotation + Position;

            }
            return _point;
        }


        /// <summary>
        /// Points camera in direction of any position.
        /// </summary>
        /// <param name="targetPos">Target position for camera to face.</param>
        //public void LookAt(Vector3 targetPos)
        //{
        //    Vector3 newForward = targetPos - Position;
        //    newForward.Normalize();

        //    Matrix rotation = Matrix.Identity;
        //    rotation.Forward = newForward;

        //    Vector3 referenceVector = Vector3.UnitY;

        //    // On the slim chance that the camera is pointer perfectly parallel with the Y Axis, we cannot
        //    // use cross product with a parallel axis, so we change the reference vector to the forward axis (Z).
        //    if (rotation.Forward.Y == referenceVector.Y || rotation.Forward.Y == -referenceVector.Y)
        //    {
        //        referenceVector = Vector3.UnitZ;
        //    }

        //    rotation.Right = Vector3.Cross(rotation.Forward, referenceVector);
        //    rotation.Up = Vector3.Cross(rotation.Right, rotation.Forward);
        //}

        protected override void UpdateViewMatrix()
        {
            _view = Matrix.CreateLookAt(_position, _lookAtPosition, _up);
        }
    }
}

