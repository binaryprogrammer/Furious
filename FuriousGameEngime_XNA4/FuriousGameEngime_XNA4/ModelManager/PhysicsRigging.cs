﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Jitter.LinearMath;
using Jitter.Dynamics;
using Jitter.Collision.Shapes;
using Microsoft.Xna.Framework.Graphics;
using FuriousGameEngime_XNA4.Screens;
using Microsoft.Xna.Framework.Content;

namespace FuriousGameEngime_XNA4.ModelManager
{
    public static class PhysicsRigging
    {        
        public static RigidBody AddPhysicsMesh(Model model)
        {            
            List<Vector3> vertices = new List<Vector3>();
            List<JOctree.TriangleVertexIndices> indices = new List<JOctree.TriangleVertexIndices>();

            ExtractData(vertices, indices, model);

            List<JVector> jvecs = new List<JVector>();

            foreach (Vector3 vec in vertices)
            {
                jvecs.Add(Conversion.ToJitterVector(vec));
            }

            int[] convexHullIndices = JConvexHull.Build(jvecs, JConvexHull.Approximation.Level6);

            List<JVector> hullPoints = new List<JVector>();
            for (int i = 0; i < convexHullIndices.Length; i++)
            {
                hullPoints.Add(jvecs[convexHullIndices[i]]);
            }

            ConvexHullShape cvhs = new ConvexHullShape(hullPoints);

            //model.Tag = new PhysicsMesh(Conversion.ToXNAVector(hullPoints), convexHullIndices.ToList());
            
            return new RigidBody(cvhs);
        }

        static void ExtractData(List<Vector3> vertices, List<JOctree.TriangleVertexIndices> indices, Model model)
        {
            Matrix[] bones_ = new Matrix[model.Bones.Count];
            model.CopyAbsoluteBoneTransformsTo(bones_);
            foreach (ModelMesh modelmesh in model.Meshes)
            {
                Matrix xform = bones_[modelmesh.ParentBone.Index];
                foreach (ModelMeshPart meshPart in modelmesh.MeshParts)
                {
                    // Before we add any more where are we starting from 
                    int offset = vertices.Count;

                    // Read the format of the vertex buffer 
                    VertexDeclaration declaration = meshPart.VertexBuffer.VertexDeclaration;
                    VertexElement[] vertexElements = declaration.GetVertexElements();
                    // Find the element that holds the position 
                    VertexElement vertexPosition = new VertexElement();
                    foreach (VertexElement vert in vertexElements)
                    {
                        if (vert.VertexElementUsage == VertexElementUsage.Position &&
                        vert.VertexElementFormat == VertexElementFormat.Vector3)
                        {
                            vertexPosition = vert;
                            // There should only be one 
                            break;
                        }
                    }
                    // Check the position element found is valid 
                    if (vertexPosition == null ||
                    vertexPosition.VertexElementUsage != VertexElementUsage.Position ||
                    vertexPosition.VertexElementFormat != VertexElementFormat.Vector3)
                    {
                        throw new Exception("Model uses unsupported vertex format!");
                    }

                    // This where we store the vertices until transformed 
                    Vector3[] allVertex = new Vector3[meshPart.NumVertices];

                    // Read the vertices from the buffer in to the array 
                    meshPart.VertexBuffer.GetData<Vector3>(meshPart.VertexOffset * declaration.VertexStride + vertexPosition.Offset,
                        allVertex, 0, meshPart.NumVertices, declaration.VertexStride);

                    // Transform them based on the relative bone location and the world if provided 
                    for (int i = 0; i != allVertex.Length; ++i)
                    {
                        Vector3.Transform(ref allVertex[i], ref xform, out allVertex[i]);
                    }

                    // Store the transformed vertices with those from all the other meshes in this model 
                    vertices.AddRange(allVertex);

                    // Find out which vertices make up which triangles 
                    if (meshPart.IndexBuffer.IndexElementSize != IndexElementSize.SixteenBits)
                    {
                        // This could probably be handled by using int in place of short but is unnecessary 
                        throw new Exception("Model uses 32-bit indices, which are not supported.");
                    }

                    // Each primitive is a triangle 
                    short[] indexElements = new short[meshPart.PrimitiveCount * 3];
                    meshPart.IndexBuffer.GetData<short>(meshPart.StartIndex * 2, indexElements, 0, meshPart.PrimitiveCount * 3);

                    // Each TriangleVertexIndices holds the three indexes to each vertex that makes up a triangle 
                    JOctree.TriangleVertexIndices[] tvi = new JOctree.TriangleVertexIndices[meshPart.PrimitiveCount];
                    for (int i = 0; i != tvi.Length; ++i)
                    {
                        // The offset is because we are storing them all in the one array and the 
                        // vertices were added to the end of the array. 
                        tvi[i].I0 = indexElements[i * 3 + 0] + offset;
                        tvi[i].I1 = indexElements[i * 3 + 1] + offset;
                        tvi[i].I2 = indexElements[i * 3 + 2] + offset;
                    }
                    // Store our triangles 
                    indices.AddRange(tvi);
                }
            }
        }            
    }
    

    public class PhysicsMesh
    {
        // During the process of constructing a primitive model, vertex
        // and index data is stored on the CPU in these managed lists.
        public List<Vector3> vertices = new List<Vector3>();
        public List<short> indices = new List<short>();

        public PhysicsMesh(List<Vector3> positions, List<int> indices)
        {
            vertices = positions;
            for (int i = 0; i < indices.Count; ++i)
            {
                AddIndex(indices[i]);
            }
        }

        void AddIndex(int index)
        {
            if (index > short.MaxValue)
                throw new ArgumentOutOfRangeException("index");

            indices.Add((short)index);
        }
    }
}
