using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace FuriousLibrary_XNA4.Cameras
{
    public class QuaternionCamera : Camera
    {
        Quaternion _quaternion = Quaternion.Identity;

        public QuaternionCamera(GraphicsDevice graphicsDevice, float aspectRatio, float fieldOfView, float minViewDistance, float maxViewDistance)
            : base(graphicsDevice, aspectRatio, fieldOfView, minViewDistance, maxViewDistance)
        {

        }

        public override Vector3 Forward
        {
            get
            {
                return Vector3.TransformNormal(Vector3.Forward, Matrix.CreateFromQuaternion(_quaternion));
            }
        }

        public Vector3 Left
        {
            get
            {
                return Vector3.TransformNormal(Vector3.Left, Matrix.CreateFromQuaternion(_quaternion));
            }
        }

        public override Vector3 Up
        {
            get
            {
                return Vector3.TransformNormal(Vector3.Up, Matrix.CreateFromQuaternion(_quaternion));
            }
        }

        protected override void UpdateViewMatrix()
        {
            Matrix translation = Matrix.CreateTranslation(_position);
            Quaternion rotatedQuaternion = _quaternion;
            rotatedQuaternion *= Quaternion.CreateFromYawPitchRoll(0, 0, 1.57f);
            Matrix rotation = Matrix.CreateFromQuaternion(rotatedQuaternion);
            _view = Matrix.Invert(rotation * translation);
        }

        /// <summary>
        /// Makes the camera's up match the one passed in 
        /// </summary>
        /// <param name="up">the angle you want the camera's up to be</param>
        public void UprightCamera(Vector3 up)
        {
            // A matrix that represents the camera's rotation
            Matrix rotation = Matrix.CreateFromQuaternion(_quaternion);

            //float currentAngle = (float)Math.Asin(rotation.Up.Y);
            //determines the angle in radians that the camera has to go to match the desired up
            float angleToRotateBy = Vector3.Dot(rotation.Up, up);

            //makes the rotation around the camera's forward vector
            rotation *= Matrix.CreateFromAxisAngle(rotation.Forward, angleToRotateBy);

            //sets the camera's quaternion to match the new rotation
            Quaternion.CreateFromRotationMatrix(ref rotation, out _quaternion);

            _quaternion.Normalize();

            UpdateViewMatrix();
        }
        
        public void AddYawPitchRoll(float yaw, float pitch, float roll)
        {
            Quaternion rotation = Quaternion.CreateFromYawPitchRoll(pitch, -yaw, roll);
            _quaternion = Quaternion.Concatenate(rotation, _quaternion);
            _quaternion.Normalize();

            UpdateViewMatrix();
        }
    }
}
