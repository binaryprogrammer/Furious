//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Text;
//using Microsoft.Xna.Framework;
//using Microsoft.Xna.Framework.Graphics;

//namespace FuriousLibrary_XNA4.Cameras
//{
//    /// <summary>
//    /// http://www.xnawiki.com/index.php?title=Arc-Ball_Camera
//    /// </summary>
//    public class ArcBallCamera : Camera
//    {
//        public Matrix rotation = Matrix.Identity;
//        public Vector3 position = Vector3.Zero;

//        // Simply feed this camera the position of whatever you want its target to be
//        public Vector3 targetPosition = Vector3.Zero;

//        public Matrix viewMatrix = Matrix.Identity;
//        public Matrix projectionMatrix = Matrix.Identity;
//        private float zoom = 30.0f;
//        public float Zoom
//        {
//            get
//            {
//                return zoom;
//            }
//            set
//            {    // Keep zoom within range
//                zoom = MathHelper.Clamp(value, zoomMin, zoomMax);
//            }

//        }

//        public override Vector3 Up
//        {
//            get
//            {
//                return rotation.Up;
//            }
//        }

//        public override Vector3 Forward
//        {
//            get { return rotation.Forward; }
//        }

//        private float horizontalAngle = MathHelper.PiOver2;
//        public float HorizontalAngle
//        {
//            get
//            {
//                return horizontalAngle;
//            }
//            set
//            {    // Keep horizontalAngle between -pi and pi.
//                horizontalAngle = value % MathHelper.Pi;
//            }
//        }

//        private float verticalAngle = MathHelper.PiOver2;
//        public float VerticalAngle
//        {
//            get
//            {
//                return verticalAngle;
//            }
//            set
//            {    // Keep vertical angle within tolerances
//                verticalAngle = MathHelper.Clamp(value, verticalAngleMin, verticalAngleMax);
//            }
//        }

//        private const float verticalAngleMin = 0.01f;
//        private const float verticalAngleMax = MathHelper.Pi - 0.01f;
//        private const float zoomMin = 0.1f;
//        private const float zoomMax = 50.0f;


//        // FOV is in radians
//        // screenWidth and screenHeight are pixel values. They're floats because we need to divide them to get an aspect ratio.
//        public ArcBallCamera(GraphicsDevice graphicsDevice, float FOV, float screenWidth, float screenHeight, float nearPlane, float farPlane)
//            :base(graphicsDevice, screenWidth/screenHeight, FOV, nearPlane, farPlane)
//        {
//            if (screenHeight < float.Epsilon)
//                throw new Exception("screenHeight cannot be zero or a negative value");

//            if (screenWidth < float.Epsilon)
//                throw new Exception("screenWidth cannot be zero or a negative value");

//            if (nearPlane < 0.1f)
//                throw new Exception("nearPlane must be greater than 0.1");

//            this.projectionMatrix = Matrix.CreatePerspectiveFieldOfView(MathHelper.ToRadians(FOV), screenWidth / screenHeight,
//                                                                        nearPlane, farPlane);
//        }

//        public void Update(GameTime gameTime)
//        {
//            // Start with an initial offset
//            Vector3 cameraPosition = new Vector3(0.0f, zoom, 0.0f);

//            // Rotate vertically
//            cameraPosition = Vector3.Transform(cameraPosition, Matrix.CreateRotationX(verticalAngle));

//            // Rotate horizontally
//            cameraPosition = Vector3.Transform(cameraPosition, Matrix.CreateRotationY(horizontalAngle));

//            position = cameraPosition + targetPosition;
//            this.LookAt(targetPosition);

//            // Compute view matrix
//            this.viewMatrix = Matrix.CreateLookAt(this.position,
//                                                    this.position + this.rotation.Forward,
//                                                    this.rotation.Up);
//        }

//        /// <summary>
//        /// Points camera in direction of any position.
//        /// </summary>
//        /// <param name="targetPos">Target position for camera to face.</param>
//        public void LookAt(Vector3 targetPos)
//        {
//            Vector3 newForward = targetPos - this.position;
//            newForward.Normalize();
//            this.rotation.Forward = newForward;

//            Vector3 referenceVector = Vector3.UnitY;

//            // On the slim chance that the camera is pointer perfectly parallel with the Y Axis, we cannot
//            // use cross product with a parallel axis, so we change the reference vector to the forward axis (Z).
//            if (this.rotation.Forward.Y == referenceVector.Y || this.rotation.Forward.Y == -referenceVector.Y)
//            {
//                referenceVector = Vector3.UnitZ;
//            }

//            this.rotation.Right = Vector3.Cross(this.rotation.Forward, referenceVector);
//            this.rotation.Up = Vector3.Cross(this.rotation.Right, this.rotation.Forward);
//        }

//        protected override void UpdateViewMatrix()
//        {
//            // Compute view matrix
//            this.viewMatrix = Matrix.CreateLookAt(this.position, this.position + this.rotation.Forward, this.rotation.Up);
//        }
//    }
//}
