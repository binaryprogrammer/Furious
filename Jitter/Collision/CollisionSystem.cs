﻿/* Copyright (C) <2009-2011> <Thorben Linneweber, Jitter Physics>
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
    /// A delegate for collision detection.
    /// </summary>
    /// <param name="body1">The first body colliding with the second one.</param>
    /// <param name="body2">The second body colliding with the first one.</param>
    /// <param name="point">The point on body in world coordinates, where collision occur.</param>
    /// <param name="normal">The normal pointing from body2 to body1.</param>
    /// <param name="penetration">Estimated penetration depth of the collision.</param>
    /// <seealso cref="CollisionSystem.Detect(bool)"/>
    /// <seealso cref="CollisionSystem.Detect(RigidBody,RigidBody)"/>
    public delegate void CollisionDetectedHandler(RigidBody body1,RigidBody body2, 
                    JVector point1, JVector point2, JVector normal,float penetration);

    /// <summary>
    /// A delegate to inform the user that a pair of bodies passed the broadsphase
    /// system of the engine.
    /// </summary>
    /// <param name="body1">The first body.</param>
    /// <param name="body2">The second body.</param>
    /// <returns>If false is returned the collision information is dropped. The CollisionDetectedHandler
    /// is never called.</returns>
    public delegate bool PassedBroadphaseHandler(RigidBody body1,RigidBody body2);

    /// <summary>
    /// A delegate to inform the user that a pair of bodies passed the narrowphase
    /// system of the engine.
    /// </summary>
    /// <param name="body1">The first body.</param>
    /// <param name="body2">The second body.</param>
    /// <returns>If false is returned the collision information is dropped. The CollisionDetectedHandler
    /// is never called.</returns>
    public delegate bool PassedNarrowphaseHandler(RigidBody body1,RigidBody body2, 
                    ref JVector point, ref JVector normal,float penetration);

    /// <summary>
    /// A delegate for raycasting.
    /// </summary>
    /// <param name="body">The body for which collision with the ray is detected.</param>
    /// <param name="normal">The normal of the collision.</param>
    /// <param name="fraction">The fraction which gives information where at the 
    /// ray the collision occured. The hitPoint is calculated by: rayStart+friction*direction.</param>
    /// <returns>If false is returned the collision information is dropped.</returns>
    public delegate bool RaycastCallback(RigidBody body,JVector normal, float fraction);

    /// <summary>
    /// CollisionSystem. Used by the world class to detect all collisions. 
    /// Can be used seperatly from the physics.
    /// </summary>
    public abstract class CollisionSystem
    {

        /// <summary>
        /// Helper class which holds two bodies. Mostly used
        /// for multithreaded detection. (Passing this as
        /// the object parameter to ThreadManager.Instance.AddTask)
        /// </summary>
        #region protected class Pair
        protected class Pair
        {
            /// <summary>
            /// The first body.
            /// </summary>
            public RigidBody body1;
            /// <summary>
            /// The second body.
            /// </summary>
            public RigidBody body2;

            /// <summary>
            /// A resource pool of Pairs.
            /// </summary>
            public static ResourcePool<Pair> Pool = new ResourcePool<Pair>();
        }
        #endregion

        /// <summary>
        /// Remove a body from the collision system. Removing a body from the world
        /// does automatically remove it from the collision system.
        /// </summary>
        /// <param name="body">The body to remove.</param>
        /// <returns>Returns true if the body was successfully removed, otherwise false.</returns>
        public abstract bool RemoveBody(RigidBody body);

        /// <summary>
        /// Add a body to the collision system. Adding a body to the world
        /// does automatically add it to the collision system.
        /// </summary>
        /// <param name="body">The body to remove.</param>
        public abstract void AddBody(RigidBody body);

        /// <summary>
        /// Gets called when the broadphase system has detected possible collisions.
        /// </summary>
        public event PassedBroadphaseHandler PassedBroadphase;

        /// <summary>
        /// Gets called when the narrowphase system has detected possible collisions.
        /// </summary>
        public event PassedNarrowphaseHandler PassedNarrowphase;

        /// <summary>
        /// Gets called when broad- and narrow phase collision were positive.
        /// </summary>
        public event CollisionDetectedHandler CollisionDetected;

        /// <summary>
        /// Initializes a new instance of the CollisionSystem.
        /// </summary>
        public CollisionSystem()
        {
            ThreadManager.InitializeInstance();
        }

        internal bool useTerrainNormal = true;
        internal bool useTriangleMeshNormal = true;

        /// <summary>
        /// If set to true the collision system uses the normal of
        /// the current colliding triangle as collision normal. This
        /// fixes unwanted behavior on triangle transitions.
        /// </summary>
        public bool UseTriangleMeshNormal { get { return useTriangleMeshNormal; } set { useTriangleMeshNormal = value; } }
        
                /// <summary>
        /// If set to true the collision system uses the normal of
        /// the current colliding triangle as collision normal. This
        /// fixes unwanted behavior on triangle transitions.
        /// </summary>
        public bool UseTerrainNormal { get { return useTerrainNormal; } set { useTerrainNormal = value; } }

        /// <summary>
        /// Checks two bodies for collisions using narrowphase.
        /// </summary>
        /// <param name="body1">The first body.</param>
        /// <param name="body2">The second body.</param>
        #region  public void Detect(RigidBody body1, RigidBody body2)
        public void Detect(RigidBody body1, RigidBody body2)
        {
            bool b1IsMulti = (body1.Shape is Multishape);
            bool b2IsMulti = (body2.Shape is Multishape);

            JVector point, normal;
            float penetration;

            if (!b1IsMulti && !b2IsMulti)
            {
                if (XenoCollide.Detect(body1.Shape, body2.Shape, ref body1.orientation,
                    ref body2.orientation, ref body1.position, ref body2.position,
                    out point, out normal, out penetration))
                {
                    if (this.RaisePassedNarrowphase(body1, body2, ref point, ref normal, penetration))
                    {
                        JVector point1, point2;
                        FindSupportPoints(body1, body2, body1.Shape, body2.Shape, ref point, ref normal, out point1, out point2);
                        RaiseCollisionDetected(body1, body2, ref point1,ref point2, ref normal, penetration);
                    }
                }
            }
            else if (b1IsMulti && b2IsMulti)
            {
                JVector[] corners = JBBox.CornersPool.GetNew();

                Multishape ms1 = (body1.Shape as Multishape);
                Multishape ms2 = (body2.Shape as Multishape);

                ms1 = ms1.RequestWorkingClone();
                ms2 = ms2.RequestWorkingClone();

                JBBox transformedBoundingBox;

                body2.BoundingBox.GetCorners(corners);

                for (int i = 0; i < 8; i++)
                {
                    JVector.Subtract(ref corners[i], ref body1.position, out corners[i]);
                    JVector.Transform(ref corners[i], ref body1.invOrientation, out corners[i]);
                }

                transformedBoundingBox = JBBox.CreateFromPoints(corners);
                int ms1Length = ms1.Prepare(ref transformedBoundingBox);


                body1.BoundingBox.GetCorners(corners);

                for (int i = 0; i < 8; i++)
                {
                    JVector.Subtract(ref corners[i], ref body2.position, out corners[i]);
                    JVector.Transform(ref corners[i], ref body2.invOrientation, out corners[i]);
                }

                transformedBoundingBox = JBBox.CreateFromPoints(corners);
                int ms2Length = ms2.Prepare(ref transformedBoundingBox);

                if (ms1Length == 0 || ms2Length == 0)
                {
                    JBBox.CornersPool.GiveBack(corners);
                    ms1.ReturnWorkingClone();
                    ms2.ReturnWorkingClone();
                    return;
                }

                for (int i = 0; i < ms1Length; i++)
                {
                    ms1.SetCurrentShape(i);

                    for (int e = 0; e < ms2Length; e++)
                    {
                        ms2.SetCurrentShape(e);

                        if (XenoCollide.Detect(ms1, ms2, ref body1.orientation,
                            ref body2.orientation, ref body1.position, ref body2.position,
                            out point, out normal, out penetration))
                        {
                            if (this.RaisePassedNarrowphase(body1, body2, ref point, ref normal, penetration))
                            {
                                JVector point1, point2;
                                FindSupportPoints(body1, body2, ms1, ms2, ref point, ref normal, out point1, out point2);
                                RaiseCollisionDetected(body1, body2, ref point1, ref point2, ref normal, penetration);
                            }
                        }
                    }
                }

                JBBox.CornersPool.GiveBack(corners);
                ms1.ReturnWorkingClone();
                ms2.ReturnWorkingClone();

            }
            else
            {
                RigidBody b1, b2;

                if (body2.Shape is Multishape) { b1 = body2; b2 = body1; }
                else { b2 = body2; b1 = body1; }

                Multishape ms = (b1.Shape as Multishape);

                ms = ms.RequestWorkingClone();

                JVector[] corners = JBBox.CornersPool.GetNew();

                b2.BoundingBox.GetCorners(corners);

                for (int i = 0; i < 8; i++)
                {
                    JVector.Subtract(ref corners[i], ref b1.position, out corners[i]);
                    JVector.Transform(ref corners[i], ref b1.invOrientation, out corners[i]);
                }

                JBBox transformedBoundingBox = JBBox.CreateFromPoints(corners);

                int msLength = ms.Prepare(ref transformedBoundingBox);

                if (msLength == 0)
                {
                    JBBox.CornersPool.GiveBack(corners);
                    ms.ReturnWorkingClone();
                    return;
                }

                for (int i = 0; i < msLength; i++)
                {
                    ms.SetCurrentShape(i);

                    if (XenoCollide.Detect(ms, b2.Shape, ref b1.orientation,
                        ref b2.orientation, ref b1.position, ref b2.position,
                        out point, out normal, out penetration))
                    {
                        if (this.RaisePassedNarrowphase(b1, b2, ref point, ref normal, penetration))
                        {
                            JVector point1, point2;
                            FindSupportPoints(b1, b2, ms, b2.Shape, ref point, ref normal, out point1, out point2);

                            if (useTerrainNormal && ms is TerrainShape)
                            {
                                (ms as TerrainShape).CollisionNormal(out normal);
                                JVector.Transform(ref normal, ref b1.orientation, out normal);
                            }
                            else if (useTriangleMeshNormal && ms is TriangleMeshShape)
                            {
                                (ms as TriangleMeshShape).CollisionNormal(out normal);
                                JVector.Transform(ref normal, ref b1.orientation, out normal);
                            }

                            RaiseCollisionDetected(b1, b2, ref point1, ref point2, ref normal, penetration);
                        }
                    }
                }

                JBBox.CornersPool.GiveBack(corners);

                ms.ReturnWorkingClone();
            }
            
        }

        private void FindSupportPoints(RigidBody body1, RigidBody body2,
            Shape shape1, Shape shape2, ref JVector point, ref JVector normal,
            out JVector point1, out JVector point2)
        {
            JVector mn; JVector.Negate(ref normal, out mn);

            JVector sA; SupportMapping(body1, shape1, ref mn, out sA);
            JVector sB; SupportMapping(body2, shape2, ref normal, out sB);

            JVector.Subtract(ref sA, ref point, out sA);
            JVector.Subtract(ref sB, ref point, out sB);

            float dot1 = JVector.Dot(ref sA, ref normal);
            float dot2 = JVector.Dot(ref sB, ref normal);

            JVector.Multiply(ref normal, dot1, out sA);
            JVector.Multiply(ref normal, dot2, out sB);

            JVector.Add(ref point, ref sA, out point1);
            JVector.Add(ref point, ref sB, out point2);
        }

        private void SupportMapping(RigidBody body, Shape workingShape, ref JVector direction, out JVector result)
        {
            JVector.Transform(ref direction, ref body.invOrientation, out result);
            workingShape.SupportMapping(ref result, out result);
            JVector.Transform(ref result, ref body.orientation, out result);
            JVector.Add(ref result, ref body.position, out result);
        }

        #endregion

        /// <summary>
        /// Sends a ray (definied by start and direction) through the scene (all bodies added).
        /// NOTE: For performance reasons terrain and trianglemeshshape aren't checked
        /// against rays (rays are of infinite length). They are checked against segments
        /// which start at rayOrigin and end in rayOrigin + rayDirection.
        /// </summary>
        public abstract bool Raycast(JVector rayOrigin, JVector rayDirection, RaycastCallback raycast, out RigidBody body, out JVector normal,out float fraction);

        /// <summary>
        /// Raycasts a single body. NOTE: For performance reasons terrain and trianglemeshshape aren't checked
        /// against rays (rays are of infinite length). They are checked against segments
        /// which start at rayOrigin and end in rayOrigin + rayDirection.
        /// </summary>
        public abstract bool Raycast(RigidBody body, JVector rayOrigin, JVector rayDirection, out JVector normal, out float fraction);


        /// <summary>
        /// Checks the state of two bodies.
        /// </summary>
        /// <param name="body1">The first body.</param>
        /// <param name="body2">The second body.</param>
        /// <returns>Returns true if both are static or inactive.</returns>
        public bool CheckBothStaticOrInactive(RigidBody body1,RigidBody body2)
        {
            return (!body1.isActive || body1.isStatic) &&
                (!body2.isActive || body2.isStatic);
       }

        /// <summary>
        /// Checks the AABB of the two rigid bodies.
        /// </summary>
        /// <param name="body1">The first body.</param>
        /// <param name="body2">The second body.</param>
        /// <returns>Returns true if an intersection occours.</returns>
        public bool CheckBoundingBoxes(RigidBody body1, RigidBody body2)
        {
            return ((((body1.boundingBox.Max.Z >= body2.boundingBox.Min.Z) && (body1.boundingBox.Min.Z <= body2.boundingBox.Max.Z)) &&
                ((body1.boundingBox.Max.Y >= body2.boundingBox.Min.Y) && (body1.boundingBox.Min.Y <= body2.boundingBox.Max.Y))) &&
                ((body1.boundingBox.Max.X >= body2.boundingBox.Min.X) && (body1.boundingBox.Min.X <= body2.boundingBox.Max.X)));
        }

        /// <summary>
        /// Raises the PassedBroadphase event.
        /// </summary>
        /// <param name="body1">The first body.</param>
        /// <param name="body2">The second body.</param>
        /// <returns>Returns false if the collision information
        /// should be dropped</returns>
        public bool RaisePassedBroadphase(RigidBody body1, RigidBody body2)
        {
            if (this.PassedBroadphase != null)
                return this.PassedBroadphase(body1, body2);

            // allow this detection by default
            return true;
        }

        /// <summary>
        /// Raises the PassedNarrowphase event.
        /// </summary>
        /// <param name="body1">The first body.</param>
        /// <param name="body2">The second body.</param>
        /// <returns>Returns false if the collision information
        /// should be dropped</returns>
        public bool RaisePassedNarrowphase(RigidBody body1, RigidBody body2, ref  JVector point,ref JVector normal, float penetration)
        {
            if (this.PassedNarrowphase != null)
                return this.PassedNarrowphase(body1, body2,ref point,ref normal,penetration);

            // allow this detection by default
            return true;
        }


        /// <summary>
        /// Raises the CollisionDetected event.
        /// </summary>
        /// <param name="body1">The first body involved in the collision.</param>
        /// <param name="body2">The second body involved in the collision.</param>
        /// <param name="point">The collision point.</param>
        /// <param name="normal">The normal pointing to body1.</param>
        /// <param name="penetration">The penetration depth.</param>
        protected void RaiseCollisionDetected(RigidBody body1, RigidBody body2,
                                            ref JVector point1, ref JVector point2,
                                            ref JVector normal, float penetration)
        {
            if (this.CollisionDetected != null)
                this.CollisionDetected(body1, body2, point1, point2, normal, penetration);
        }

        /// <summary>
        /// Tells the collisionsystem to check all bodies for collisions. Hook into the <see cref="PassedBroadphase"/>
        /// and <see cref="CollisionDetected"/> events to get the results.
        /// </summary>
        /// <param name="multiThreaded">If true internal multithreading is used.</param>
        public abstract void Detect(bool multiThreaded);
    }
}
