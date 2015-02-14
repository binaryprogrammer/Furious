/* Copyright (C) <2009-2011> <Thorben Linneweber, Jitter Physics>
* 
*  This software is provided 'as-is', without any express or implied
*  warranty.  In no event will the authors be held liable for any damages
*  arising from the use of this software.
*
*  Permission is granted to anyone to use this software for any purpose,
*  including commercial applications, and to alter it and redistribute it
*  freely, subject to the following restrictions:
*
*  1. The origin of this software must not be misrepresented; you must not
*      claim that you wrote the original software. If you use this software
*      in a product, an acknowledgment in the product documentation would be
*      appreciated but is not required.
*  2. Altered source versions must be plainly marked as such, and must not be
*      misrepresented as being the original software.
*  3. This notice may not be removed or altered from any source distribution. 
*/

#region Using Statements
using System;
using System.Collections.Generic;

using Jitter.Dynamics;
using Jitter.LinearMath;
using Jitter.Collision.Shapes;
#endregion

namespace Jitter.Collision
{
    /// <summary>
    /// O(n^2) Broadphase detection. Every body is checked against each other body.
    /// This is pretty fast for scene containing just a few(~30) bodies.
    /// </summary>
    public class CollisionSystemBrute : CollisionSystem
    {
        private List<RigidBody> bodyList = new List<RigidBody>();
        private Action<object> detectCallback;

        /// <summary>
        /// Creates a new instance of the CollisionSystemBrute class.
        /// </summary>
        public CollisionSystemBrute()
        {
            detectCallback = new Action<object>(DetectCallback);
        }

        /// <summary>
        /// Remove a body from the collision system. Removing a body from the world
        /// does automatically remove it from the collision system.
        /// </summary>
        /// <param name="body">The body to remove.</param>
        /// <returns>Returns true if the body was successfully removed, otherwise false.</returns>
        public override bool RemoveBody(RigidBody body)
        {
            // just keep our internal list in sync
            return bodyList.Remove(body);
        }

        /// <summary>
        /// Add a body to the collision system. Adding a body to the world
        /// does automatically add it to the collision system.
        /// </summary>
        /// <param name="body">The body to remove.</param>
        public override void AddBody(RigidBody body)
        {
            if (bodyList.Contains(body))
                throw new ArgumentException("The body was already added to the collision system.","body");

            // just keep our internal list in sync
            bodyList.Add(body);
        }

       

        /// <summary>
        /// Tells the collisionsystem to check all bodies for collisions. Hook into the 
        /// <see cref="CollisionSystem.PassedBroadphase"/>
        /// and <see cref="CollisionSystem.CollisionDetected"/> events to get the results.
        /// </summary>
        /// <param name="multiThreaded">If true internal multithreading is used.</param>
        #region public override void Detect(bool multiThreaded)
        public override void Detect(bool multiThreaded)
        {
            int count = bodyList.Count;

            if (multiThreaded)
            {
                for (int i = 0; i < count; i++)
                {
                    for (int e = i + 1; e < count; e++)
                    {
                        if (!this.CheckBothStaticOrInactive(bodyList[i], bodyList[e]) && this.CheckBoundingBoxes(bodyList[i], bodyList[e]))
                        {
                            if (RaisePassedBroadphase(bodyList[i], bodyList[e]))
                            {
                                Pair pair = Pair.Pool.GetNew();

                                if (swapOrder) { pair.body1 = bodyList[i]; pair.body2 = bodyList[e]; }
                                else { pair.body2 = bodyList[e]; pair.body1 = bodyList[i]; }
                                swapOrder = !swapOrder;

                                ThreadManager.internalInstance.AddTask(detectCallback, pair);
                            }
                        }
                    }
                }

                ThreadManager.internalInstance.Execute();
            }
            else
            {
                for (int i = 0; i < count; i++)
                {
                    for (int e = i + 1; e < count; e++)
                    {
                        if (!this.CheckBothStaticOrInactive(bodyList[i], bodyList[e]) && this.CheckBoundingBoxes(bodyList[i], bodyList[e]))
                        {
                            if (RaisePassedBroadphase(bodyList[i], bodyList[e]))
                            {
                                if (swapOrder) Detect(bodyList[i], bodyList[e]);
                                else Detect(bodyList[e], bodyList[i]);
                                swapOrder = !swapOrder;
                            }
                        }
                    }
                }
            }
        }
        #endregion

        /// <summary>
        /// Sends a ray (definied by start and direction) through the scene (all bodies added).
        /// NOTE: For performance reasons terrain and trianglemeshshape aren't checked
        /// against rays (rays are of infinite length). They are checked against segments
        /// which start at rayOrigin and end in rayOrigin + rayDirection.
        /// </summary>
        #region public override bool Raycast(JVector rayOrigin, JVector rayDirection, out JVector normal,out float fraction)
        public override bool Raycast(JVector rayOrigin, JVector rayDirection, RaycastCallback raycast, out RigidBody body, out JVector normal, out float fraction)
        {
            JVector tempNormal = rayDirection; float tempFraction = float.MaxValue; body = null;
            fraction = float.MaxValue; normal = rayDirection;

            bool result = false;

            foreach (RigidBody b in bodyList)
            {
                if (!b.boundingBox.RayIntersect(ref rayOrigin, ref rayDirection)) continue;

                if (b.Shape is Multishape)
                {
                    JVector tempTempNormal;
                    float tempTempFraction;

                    bool multiShapeCollides = false;

                    Multishape ms = (b.Shape as Multishape);

                    JVector transformedOrigin; JVector.Subtract(ref rayOrigin, ref b.position, out transformedOrigin);
                    JVector.Transform(ref transformedOrigin, ref b.invOrientation, out transformedOrigin);
                    JVector transformedDirection; JVector.Transform(ref rayDirection, ref b.invOrientation, out transformedDirection);

                    int msLength = ms.Prepare(ref transformedOrigin, ref transformedDirection);

                    for (int i = 0; i < msLength; i++)
                    {
                        ms.SetCurrentShape(i);

                        if (GJKCollide.Raycast(b.Shape, ref b.orientation, ref b.invOrientation, ref b.position,
                            ref rayOrigin, ref rayDirection, out tempTempFraction, out tempTempNormal))
                        {
                            if (tempTempFraction < tempFraction)
                            {
                                if (useTerrainNormal && ms is TerrainShape)
                                {
                                    (ms as TerrainShape).CollisionNormal(out tempTempNormal);
                                    JVector.Transform(ref tempTempNormal, ref b.orientation, out tempTempNormal);
                                    tempTempNormal.Negate();
                                }
                                else if (useTriangleMeshNormal && ms is TriangleMeshShape)
                                {
                                    (ms as TriangleMeshShape).CollisionNormal(out tempTempNormal);
                                    JVector.Transform(ref tempTempNormal, ref b.orientation, out tempTempNormal);
                                    tempTempNormal.Negate();
                                }

                                tempNormal = tempTempNormal;
                                tempFraction = tempTempFraction;
                                multiShapeCollides = true;
                            }
                        }
                    }

                    if (multiShapeCollides && (tempFraction < fraction) && (raycast == null || raycast(b, tempNormal, tempFraction)))
                    {
                        normal = tempNormal;
                        fraction = tempFraction;
                        body = b;
                        result = true;
                    }

                }
                else
                {
                    if (GJKCollide.Raycast(b.Shape, ref b.orientation, ref b.invOrientation, ref b.position,
                        ref rayOrigin, ref rayDirection, out tempFraction, out tempNormal))
                    {
                        if (tempFraction < fraction && (raycast == null || raycast(b, tempNormal, tempFraction)))
                        {
                            normal = tempNormal;
                            fraction = tempFraction;
                            body = b;
                            result = true;
                        }
                    }
                }
            }

            return result;
        }
        #endregion

        /// <summary>
        /// Raycasts a single body. NOTE: For performance reasons terrain and trianglemeshshape aren't checked
        /// against rays (rays are of infinite length). They are checked against segments
        /// which start at rayOrigin and end in rayOrigin + rayDirection.
        /// </summary>
        #region public override bool Raycast(RigidBody body, JVector rayOrigin, JVector rayDirection, out JVector normal, out float fraction)
        public override bool Raycast(RigidBody body, JVector rayOrigin, JVector rayDirection, out JVector normal, out float fraction)
        {
            fraction = float.MaxValue; normal = rayDirection;

            if (!body.boundingBox.RayIntersect(ref rayOrigin, ref rayDirection)) return false;

            if (body.Shape is Multishape)
            {
                JVector tempNormal;
                float tempFraction;

                bool multiShapeCollides = false;

                Multishape ms = (body.Shape as Multishape);

                JVector transformedOrigin; JVector.Subtract(ref rayOrigin, ref body.position, out transformedOrigin);
                JVector.Transform(ref transformedOrigin, ref body.invOrientation, out transformedOrigin);
                JVector transformedDirection; JVector.Transform(ref rayDirection, ref body.invOrientation, out transformedDirection);

                int msLength = ms.Prepare(ref transformedOrigin, ref transformedDirection);

                for (int i = 0; i < msLength; i++)
                {
                    ms.SetCurrentShape(i);

                    if (GJKCollide.Raycast(body.Shape, ref body.orientation, ref body.invOrientation, ref body.position,
                        ref rayOrigin, ref rayDirection, out tempFraction, out tempNormal))
                    {
                        if (tempFraction < fraction)
                        {
                            if (useTerrainNormal && ms is TerrainShape)
                            {
                                (ms as TerrainShape).CollisionNormal(out tempNormal);
                                JVector.Transform(ref tempNormal, ref body.orientation, out tempNormal);
                                tempNormal.Negate();
                            }
                            else if (useTriangleMeshNormal && ms is TriangleMeshShape)
                            {
                                (ms as TriangleMeshShape).CollisionNormal(out tempNormal);
                                JVector.Transform(ref tempNormal, ref body.orientation, out tempNormal);
                                tempNormal.Negate();
                            }

                            normal = tempNormal;
                            fraction = tempFraction;
                            multiShapeCollides = true;
                        }
                    }
                }

                return multiShapeCollides;
            }
            else
            {
                return (GJKCollide.Raycast(body.Shape, ref body.orientation, ref body.invOrientation, ref body.position,
                    ref rayOrigin, ref rayDirection, out fraction, out normal));
            }


        }
        #endregion
        private bool swapOrder = false;

        private void DetectCallback(object obj)
        {
            Pair pair = obj as Pair;
            base.Detect(pair.body1, pair.body2);
            Pair.Pool.GiveBack(pair);
        }
    }
}
