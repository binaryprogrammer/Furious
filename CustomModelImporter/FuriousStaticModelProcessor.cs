using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework.Content.Pipeline.Graphics;
using Microsoft.Xna.Framework.Content.Pipeline;
using Microsoft.Xna.Framework.Content.Pipeline.Processors;
using System.ComponentModel;
using Microsoft.Xna.Framework.Content.Pipeline.Serialization.Compiler;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Jitter.LinearMath;
using Microsoft.Xna.Framework.Content;

namespace CustomModelImporter
{
    /// <summary>
    /// This class will be instantiated by the XNA Framework Content Pipeline
    /// to apply custom processing to content data, converting an object of
    /// type TInput to TOutput. The input and output types may be the same if
    /// the processor wishes to alter data without changing its type.
    ///
    /// This should be part of a Content Pipeline Extension Library project.
    ///
    /// display name for this processor.
    /// </summary>
    [ContentProcessor(DisplayName = "Furious Static Model Importer")]
    public class FuriousStaticModelProcessor : ModelProcessor
    {
        ContentIdentity _rootIdentity;

        /// <summary>
        /// Override the Process method to store the ContentIdentity of the model root node.
        /// </summary>
        public override ModelContent  Process(NodeContent input, ContentProcessorContext context)
        {
            _rootIdentity = input.Identity;

            return base.Process(input, context);
        }

        #region old not used
        public void ExtractData(NodeContent node)
        {
            List<Vector3> vertices = new List<Vector3>();
            List<JOctree.TriangleVertexIndices> indices = new List<JOctree.TriangleVertexIndices>();

            FindVerticies(node, ref vertices, ref indices);

            // Recursively scan over the children of this node and extract all verticies into the ModelInformation Class
            foreach (NodeContent child in node.Children)
            {
                FindVerticies(child, ref vertices, ref indices);
            }

            //outputModel.SetData(vertices, indices); // a class that contains all the model data
        }

        void FindVerticies(NodeContent node, ref List<Vector3> vertices, ref List<JOctree.TriangleVertexIndices> indices)
        {
            MeshContent mesh = node as MeshContent;

            if (mesh != null)
            {
                // Look up the absolute transform of the mesh.
                Matrix absoluteTransform = mesh.AbsoluteTransform;

                // Reorder vertex and index data so triangles will render in
                // an order that makes efficient use of the GPU vertex cache.
                MeshHelper.OptimizeForCache(mesh);

                foreach (GeometryContent geometry in mesh.Geometry)
                {
                    // Before we add any more where are we starting from 
                    int offset = vertices.Count;

                    // Loop over all the indices in this piece of geometry.
                    // Every group of three indices represents one triangle.
                    foreach (int index in geometry.Indices)
                    {
                        // Look up the position of this vertex.
                        Vector3 vertex = geometry.Vertices.Positions[index];

                        // Transform from local into world space.
                        vertex = Vector3.Transform(vertex, absoluteTransform);

                        // Store this vertex.
                        vertices.Add(vertex);
                    }

                    int triangleCount = geometry.Indices.Count / 3;

                    JOctree.TriangleVertexIndices[] tvi = new JOctree.TriangleVertexIndices[triangleCount];
                    for (int i = 0; i != tvi.Length; ++i)
                    {
                        // The offset is because we are storing them all in the one array and the vertices were added to the end of the array. 
                        tvi[i].I0 = geometry.Indices[i * 3 + 0] + offset;
                        tvi[i].I1 = geometry.Indices[i * 3 + 1] + offset;
                        tvi[i].I2 = geometry.Indices[i * 3 + 2] + offset;
                    }

                    // Store our triangles 
                    indices.AddRange(tvi);
                }
            }
        }
        #endregion
    }
}
