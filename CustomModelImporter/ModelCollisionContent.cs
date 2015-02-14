using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Jitter.LinearMath;

namespace CustomModelImporter
{
    [ContentSerializerRuntimeType("JitterDemo.ConvexHullObject, JitterDemo")]
    public class ModelCollisionContent
    {
        [ContentSerializer]
        ModelCollisionInformation _collisionInfo;

        [ContentSerializerRuntimeType("JitterDemo.ConvexHullObject+CollisionInformation, JitterDemo")]
        class ModelCollisionInformation
        {
            public List<Vector3> vertices = new List<Vector3>();
            public List<JOctree.TriangleVertexIndices> indices = new List<JOctree.TriangleVertexIndices>();
        }

        public void SetData(List<Vector3> verticies, List<JOctree.TriangleVertexIndices> indicies)
        {
            ModelCollisionInformation collisionInfo = new ModelCollisionInformation();

            collisionInfo.vertices = verticies;
            collisionInfo.indices = indicies;

            _collisionInfo = collisionInfo;
        }
    }
}
