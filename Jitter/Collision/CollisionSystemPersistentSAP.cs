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
using System.Linq;
using System.Collections.Generic;

using Jitter.Dynamics;
using Jitter.LinearMath;
using Jitter.Collision.Shapes;
using System.Collections;
#endregion

namespace Jitter.Collision
{

    // TODO: The collision detection itself is great,
    //       but removing an object is O(n) and Memory usage is O(n^2)
    //       If someone has time: use hashalgorithm for pairmanagement
    //       and batch remove/add methods.

    /// <summary>
    /// EXPERIMENTAL: FULL SAP WITH COHERENT UPDATES
    /// </summary>
    public class CollisionSystemPersistentSAP : CollisionSystem
    {

        #region private class Triangular2BitMatrix
        private class Triangular2BitMatrix
        {
            private BitArray bitMatrix0 = new BitArray(0);
            private BitArray bitMatrix1 = new BitArray(0);

            public Triangular2BitMatrix()
            {
                Size = 0;
            }

            public void ZeroAll()
            {
                bitMatrix0.SetAll(false);
                bitMatrix1.SetAll(false);
            }

            public int Size
            {
                set
                {
                    int length = (int)((double)(value - 1.0d) * ((double)value / 2.0d)) + value;
                    bitMatrix0.Length = length;
                    bitMatrix1.Length = length;
                }
                get
                {
                    int maxIndex = bitMatrix0.Length - 1;
                    return (int)(Math.Sqrt(2.25d + 2.0d * (double)maxIndex) - 0.5d);
                }
            }

            public int GetValue(int row, int column)
            {
                if (column > row) { int temp = column; column = row; row = temp; }
                int index = (int)((float)(row - 1) * ((float)row / 2.0f)) + column;

                return (bitMatrix1[index] ? 1 : 0) * 2 + (bitMatrix0[index] ? 1 : 0);
            }

            public void SetValue(int row, int column, int value)
            {
                if (column > row) { int temp = column; column = row; row = temp; }
                int index = (int)((float)(row - 1) * ((float)row / 2.0f)) + column;

                bitMatrix0[index] = !(value % 2 == 0);
                bitMatrix1[index] = !(value / 2 == 0);
            }

            public int IncrementCounter(int row, int column)
            {
                if (column > row) { int temp = column; column = row; row = temp; }
                int index = (int)((float)(row - 1) * ((float)row / 2.0f)) + column;

                bool b0 = bitMatrix0[index];
                bool b1 = bitMatrix1[index];

                if (b0 && b1) return 3;

                b1 = b0 ^ b1;
                b0 = !b0;

                bitMatrix0[index] = b0;
                bitMatrix1[index] = b1;

                return (b1 ? 1 : 0) * 2 + (b0 ? 1 : 0);
            }

            public int DecrementCounter(int row, int column)
            {
                if (column > row) { int temp = column; column = row; row = temp; }
                int index = (int)((float)(row - 1) * ((float)row / 2.0f)) + column;

                bool b0 = bitMatrix0[index];
                bool b1 = bitMatrix1[index];

                if (!b0 && !b1) return 0;

                b1 = !(b0 ^ b1);
                b0 = !b0;

                bitMatrix0[index] = b0;
                bitMatrix1[index] = b1;

                return (b1 ? 1 : 0) * 2 + (b0 ? 1 : 0);
            }
        }
        #endregion

        #region private class SweepPoint
        private class SweepPoint
        {
            public RigidBody Body;
            public bool Begin;
            public int Axis;

            public SweepPoint(RigidBody body, bool begin, int axis)
            {
                this.Body = body;
                this.Begin = begin;
                this.Axis = axis;
            }

            public float GetValue()
            {
                 if (Begin)
                {
                    if (Axis == 0) return Body.boundingBox.Min.X;
                    else if (Axis == 1) return Body.boundingBox.Min.Y;
                    else return Body.boundingBox.Min.Z;
                }
                else
                {
                    if (Axis == 0) return Body.boundingBox.Max.X;
                    else if (Axis == 1) return Body.boundingBox.Max.Y;
                    else return Body.boundingBox.Max.Z;
                }
            }
        }
        #endregion

        private const int MatrixGrowFactor = 500;
        private const int NumberAddedObjectsBruteForceIsUsed = 250;

        private List<RigidBody> bodyList = new List<RigidBody>();
        private List<SweepPoint> axis1 = new List<SweepPoint>();
        private List<SweepPoint> axis2 = new List<SweepPoint>();
        private List<SweepPoint> axis3 = new List<SweepPoint>();

        private Triangular2BitMatrix t2bM;

        private HashSet<ArbiterKey> fullOverlaps = new HashSet<ArbiterKey>();

        Action<object> detectCallback, sortCallback;

        public CollisionSystemPersistentSAP()
        {
            detectCallback = new Action<object>(DetectCallback);
            sortCallback = new Action<object>(SortCallback);

            t2bM = new Triangular2BitMatrix();
        }

        #region Incoherent Update - Quicksort

        private int QuickSort(SweepPoint sweepPoint1, SweepPoint sweepPoint2)
        {
            float val1 = sweepPoint1.GetValue();
            float val2 = sweepPoint2.GetValue();

            if (val1 > val2) return 1;
            else if (val2 > val1) return -1;
            else return 0;
        }

        List<RigidBody> activeList = new List<RigidBody>();

        private void DirtySortAxis(List<SweepPoint> axis)
        {
            axis.Sort(QuickSort);
            activeList.Clear();

            for (int i = 0; i < axis.Count; i++)
            {
                SweepPoint keyelement = axis[i];

                if (keyelement.Begin)
                {
                    foreach (RigidBody body in activeList)
                    {
                        int count = t2bM.IncrementCounter(body.internalIndex, keyelement.Body.internalIndex);
                        if (count == 3) fullOverlaps.Add(new ArbiterKey(body, keyelement.Body));
                    }

                    activeList.Add(keyelement.Body);
                }
                else
                {
                    activeList.Remove(keyelement.Body);
                }
            }
        }
        #endregion

        #region Coherent Update - Insertionsort

        private void SortAxis(List<SweepPoint> axis)
        {
            for (int j = 1; j < axis.Count; j++)
            {
                SweepPoint keyelement = axis[j];
                float key = keyelement.GetValue();

                int i = j - 1;

                while (i >= 0 && axis[i].GetValue() > key)
                {
                    SweepPoint swapper = axis[i];

                    if (keyelement.Begin && !swapper.Begin)
                    {
                        lock (t2bM)
                        {
                            int count = t2bM.IncrementCounter(keyelement.Body.internalIndex,
                                swapper.Body.internalIndex);

                            if (count == 3)
                            {
                                ArbiterKey pair = new ArbiterKey(keyelement.Body, swapper.Body);
                                fullOverlaps.Add(pair);
                            }
                        }
                    }

                    if (!keyelement.Begin && swapper.Begin)
                    {
                        lock (t2bM)
                        {
                            int count = t2bM.DecrementCounter(keyelement.Body.internalIndex,
                                swapper.Body.internalIndex);

                            if (count == 2)
                            {
                                ArbiterKey pair = new ArbiterKey(keyelement.Body, swapper.Body);
                                fullOverlaps.Remove(pair);
                            }
                        }
                    }

                    axis[i + 1] = swapper;
                    i = i - 1;
                }
                axis[i + 1] = keyelement;
            }
        }
        #endregion

        private void ResizeMatrix(int growValue)
        {
            if (bodyList.Count > t2bM.Size)
            {
                t2bM.Size += growValue;
            }
            else if (t2bM.Size - bodyList.Count > growValue)
            {
                t2bM.Size -= growValue;
            }
        }

        int addCounter = 0;

        public override void AddBody(RigidBody body)
        {
            if (body.internalIndex < bodyList.Count && bodyList[body.internalIndex] == body) return;

            body.internalIndex = bodyList.Count;

            bodyList.Add(body);

            axis1.Add(new SweepPoint(body, true, 0)); axis1.Add(new SweepPoint(body, false, 0));
            axis2.Add(new SweepPoint(body, true, 1)); axis2.Add(new SweepPoint(body, false, 1));
            axis3.Add(new SweepPoint(body, true, 2)); axis3.Add(new SweepPoint(body, false, 2));

            ResizeMatrix(MatrixGrowFactor);

            addCounter++;
        }

        public override bool RemoveBody(RigidBody body)
        {
            if (body.internalIndex > bodyList.Count || bodyList[body.internalIndex] != body) return false;

            for (int i = 0; i < axis1.Count; i++) { if (axis1[i].Body == body) { axis1.RemoveAt(i); i--; } }
            for (int i = 0; i < axis2.Count; i++) { if (axis2[i].Body == body) { axis2.RemoveAt(i); i--; } }
            for (int i = 0; i < axis3.Count; i++) { if (axis3[i].Body == body) { axis3.RemoveAt(i); i--; } }

            Stack<ArbiterKey> depricated = new Stack<ArbiterKey>();
            foreach (var pair in fullOverlaps) if (pair.body1 == body || pair.body2 == body) depricated.Push(pair);
            while (depricated.Count > 0) fullOverlaps.Remove(depricated.Pop());

            RigidBody lastBody = bodyList[bodyList.Count - 1];

            if (body == lastBody)
            {
                for (int i = 0; i < bodyList.Count; i++)
                {
                    t2bM.SetValue(body.internalIndex, i, 0);
                }

                bodyList.RemoveAt(body.internalIndex);
            }
            else
            {
                for (int i = 0; i < bodyList.Count; i++)
                {
                    int value = t2bM.GetValue(lastBody.internalIndex, i);
                    t2bM.SetValue(body.internalIndex, i, value);
                }

                bodyList.RemoveAt(lastBody.internalIndex);
                bodyList[body.internalIndex] = lastBody;

                lastBody.internalIndex = body.internalIndex;
            }

            ResizeMatrix(MatrixGrowFactor);

            return true;
        }

        bool swapOrder = false;

        /// <summary>
        /// Tells the collisionsystem to check all bodies for collisions. Hook into the
        /// <see cref="CollisionSystem.PassedBroadphase"/>
        /// and <see cref="CollisionSystem.CollisionDetected"/> events to get the results.
        /// </summary>
        /// <param name="multiThreaded">If true internal multithreading is used.</param>
        public override void Detect(bool multiThreaded)
        {
            if (addCounter > NumberAddedObjectsBruteForceIsUsed)
            {
                t2bM.ZeroAll();
                fullOverlaps.Clear();

                DirtySortAxis(axis1);
                DirtySortAxis(axis2);
                DirtySortAxis(axis3);
            }
            else
            {
                if (multiThreaded)
                {
                    ThreadManager.internalInstance.AddTask(sortCallback, axis1);
                    ThreadManager.internalInstance.AddTask(sortCallback, axis2);
                    ThreadManager.internalInstance.AddTask(sortCallback, axis3);

                    ThreadManager.internalInstance.Execute();
                }
                else
                {
                    SortAxis(axis1);
                    SortAxis(axis2);
                    SortAxis(axis3);
                }
            }

            addCounter = 0;

            if (multiThreaded)
            {
                foreach (ArbiterKey key in fullOverlaps)
                {
                    if (this.CheckBothStaticOrInactive(key.body1, key.body2)) continue;

                    if (base.RaisePassedBroadphase(key.body1, key.body2))
                    {

                        Pair pair = Pair.Pool.GetNew();

                        if (swapOrder) { pair.body1 = key.body1; pair.body2 = key.body2; }
                        else { pair.body2 = key.body2; pair.body1 = key.body1; }
                        swapOrder = !swapOrder;

                        ThreadManager.internalInstance.AddTask(detectCallback, pair);
                    }
                }

                ThreadManager.internalInstance.Execute();
            }
            else
            {
                foreach (ArbiterKey key in fullOverlaps)
                {
                    if (this.CheckBothStaticOrInactive(key.body1, key.body2)) continue;

                    if (base.RaisePassedBroadphase(key.body1, key.body2))
                    {

                        if (swapOrder) { Detect(key.body1, key.body2); }
                        else Detect(key.body2, key.body1);
                        swapOrder = !swapOrder;

                    }
                }
            }
        }

        private void SortCallback(object obj)
        {
            SortAxis(obj as List<SweepPoint>);
        }

        private void DetectCallback(object obj)
        {
            Pair pair = obj as Pair;
            base.Detect(pair.body1, pair.body2);
            Pair.Pool.GiveBack(pair);
        }

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
    }
}
