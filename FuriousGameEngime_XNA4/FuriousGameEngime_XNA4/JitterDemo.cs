//namespace FuriousGameEngime_XNA4.JitterClasses
//{
//    public class JitterDemo : Microsoft.Xna.Framework.Game
//    {
//        private int _currentScene = 0;

//        public JitterDemo()
//        {
//        }


//        // Store information for drag and drop
//        JVector hitPoint, hitNormal;
//        SingleBodyConstraints.PointOnPoint grabConstraint;
//        RigidBody grabBody;
//        float hitDistance = 0.0f;

//        protected override void Update(GameTime gameTime)
//        {

            #region drag and drop physical objects with the mouse
//            if (mouseState.LeftButton == ButtonState.Pressed &&
//                mousePreviousState.LeftButton == ButtonState.Released)
//            {
//                JVector ray = Conversion.ToJitterVector(RayTo(mouseState.X, mouseState.Y));
//                JVector camp = Conversion.ToJitterVector(Camera.Position);

//                ray = JVector.Normalize(ray) * 100;

//                float fraction;
//                bool result = World.CollisionSystem.Raycast(camp, ray, RaycastCallback, out grabBody, out hitNormal, out fraction);

//                if (result)
//                {
//                    hitPoint = camp + fraction * ray;

//                    if (grabConstraint != null) World.RemoveConstraint(grabConstraint);

//                    JVector lanchor = hitPoint - grabBody.Position;
//                    lanchor = JVector.Transform(lanchor, JMatrix.Transpose(grabBody.Orientation));

//                    grabConstraint = new SingleBodyConstraints.PointOnPoint(grabBody, lanchor);
//                    grabConstraint.Softness = 0.1f;
                    
//                    World.AddConstraint(grabConstraint);
//                    hitDistance = (Conversion.ToXNAVector(hitPoint) - Camera.Position).Length();
//                    scrollWheel = mouseState.ScrollWheelValue;
//                    grabConstraint.Anchor = hitPoint;
//                }
//            }

//            if (mouseState.LeftButton == ButtonState.Pressed)
//            {
//                hitDistance += (mouseState.ScrollWheelValue - scrollWheel) * 0.01f;
//                scrollWheel = mouseState.ScrollWheelValue;

//                if (grabBody != null)
//                {
//                    Vector3 ray = RayTo(mouseState.X, mouseState.Y); ray.Normalize();
//                    grabConstraint.Anchor = Conversion.ToJitterVector(Camera.Position + ray * hitDistance);
//                    grabBody.IsActive = true;
//                }
//            }
//            else
//            {
//                grabBody = null;
//                if (grabConstraint != null) World.RemoveConstraint(grabConstraint);
//            }
            #endregion

//            float step = (float)gameTime.ElapsedGameTime.TotalSeconds;
//            if(step > 1.0f / 60.0f) step = 1.0f /60.0f;

//            World.Step(step, true);

//            // I don't know what the purpose of this is, but I don't see any changes with our without it.
//            //if(!keyboardPreviousState.IsKeyDown(Keys.Space) && keyState.IsKeyDown(Keys.Space))
//            //    World.Step(step, true);
//        }

//        private bool RaycastCallback(RigidBody body, JVector normal, float fraction)
//        {
//            if (body.IsStatic) return false;
//            else return true;
//        }