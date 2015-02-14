using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using FuriousGameEngime_XNA4.HelperClasses;
using FuriousGameEngime_XNA4.Screens;
using Jitter.Dynamics;
using FuriousGameEngime_XNA4.GameEntities;
using Jitter.Collision.Shapes;

namespace FuriousGameEngime_XNA4.ModelManager
{
    public class ModelBase
    {
        #region Variables
        GameScreen _gameScreen;

        /// <summary>
        /// a reference of the loaded model
        /// </summary>
        internal readonly Model model;

        /// <summary>
        /// is the base physics body that is added to each instance
        /// </summary>
        internal readonly RigidBody bodyBase;

        /// <summary>
        /// the name of the model is the file name
        /// </summary>
        internal readonly string name;

        /// <summary>
        /// an instance of the model with a transform
        /// </summary>
        List<ModelInstance> _instances = new List<ModelInstance>();

        /// <summary>
        /// our model's bones
        /// </summary>
        Matrix[] _instancedModelBones;

        /// <summary>
        /// our vertex buffer
        /// </summary>
        DynamicVertexBuffer _instanceVertexBuffer;
        #endregion

        // To store instance transform matrices in a vertex buffer, we use this custom
        // vertex type which encodes 4x4 matrices as a set of four Vector4 values.
        static VertexDeclaration instanceVertexDeclaration = new VertexDeclaration
        (
            new VertexElement(0, VertexElementFormat.Vector4, VertexElementUsage.BlendWeight, 0),
            new VertexElement(16, VertexElementFormat.Vector4, VertexElementUsage.BlendWeight, 1),
            new VertexElement(32, VertexElementFormat.Vector4, VertexElementUsage.BlendWeight, 2),
            new VertexElement(48, VertexElementFormat.Vector4, VertexElementUsage.BlendWeight, 3)
        );

        /// <summary>
        /// the base model that represents all instances in the game
        /// </summary>
        /// <param name="game"></param>
        /// <param name="model"></param>
        internal ModelBase(GameScreen gameScreen, Model model, string name)
        {
            _gameScreen = gameScreen;
            this.model = model;
            _instancedModelBones = new Matrix[model.Bones.Count];
            model.CopyAbsoluteBoneTransformsTo(_instancedModelBones);

            bodyBase = PhysicsRigging.AddPhysicsMesh(model);

            this.name = name;
        }

        /// <summary>
        /// Creates a new instance of the modelBase
        /// </summary>
        /// <returns> a new Model Instance</returns>
        internal ModelInstance CreateInstance()
        {
            ModelInstance ret = new ModelInstance(_gameScreen, this);
            _instances.Add(ret);
            return ret;
        }

        internal void AddInstance(ModelInstance instance)
        {
            _instances.Add(instance);
        }

        internal void RemoveInstance(ModelInstance instance)
        {
            _instances.Remove(instance);
        }

        internal Shape CollisionShape
        {
            get
            {
                return bodyBase.Shape;
            }
        }

        #region Draw
        /// <summary>
        /// Efficiently draws several copies of a piece of geometry using hardware instancing.
        /// </summary>
        internal void DrawAllInstances(GraphicsDevice graphicsDevice, Matrix view, Matrix projection)
        {
            Matrix[] transforms = new Matrix[_instances.Count];

            for (int i = 0; i < _instances.Count; i++)
            {
                transforms[i] = _instances[i].Transform;
            }

            if (transforms == null || transforms.Length == 0)
                return;

            // If we have more instances than room in our vertex buffer, grow it to the neccessary size.
            if ((_instanceVertexBuffer == null) || (transforms.Length > _instanceVertexBuffer.VertexCount))
            {
                if (_instanceVertexBuffer != null)
                    _instanceVertexBuffer.Dispose();

                _instanceVertexBuffer = new DynamicVertexBuffer(graphicsDevice, instanceVertexDeclaration, transforms.Length, BufferUsage.WriteOnly);
            }

            // Transfer the latest instance transform matrices into the instanceVertexBuffer.
            _instanceVertexBuffer.SetData(transforms, 0, transforms.Length, SetDataOptions.Discard);

            foreach (ModelMesh mesh in model.Meshes)
            {
                if (mesh.Name.Contains("collision"))
                    continue;

                foreach (ModelMeshPart meshPart in mesh.MeshParts)
                {
                    // Tell the GPU to read from both the model vertex buffer plus our instanceVertexBuffer.
                    graphicsDevice.SetVertexBuffers(new VertexBufferBinding(meshPart.VertexBuffer, meshPart.VertexOffset, 0),
                        new VertexBufferBinding(_instanceVertexBuffer, 0, 1));

                    graphicsDevice.Indices = meshPart.IndexBuffer;

                    // Set up the instance rendering effect.
                    Effect effect = meshPart.Effect;

                    if (effect.CurrentTechnique.Name == "SkinnedEffect")
                    {
                        return;
                    }
                    else
                    {
                        effect.Parameters["World"].SetValue(_instancedModelBones[mesh.ParentBone.Index]);
                        effect.Parameters["View"].SetValue(view);
                        effect.Parameters["Projection"].SetValue(projection);
                    }

                    // Draw all the instance copies in a single call.
                    foreach (EffectPass pass in effect.CurrentTechnique.Passes)
                    {
                        pass.Apply();

                        graphicsDevice.DrawInstancedPrimitives(PrimitiveType.TriangleList, 0, 0, meshPart.NumVertices,
                            meshPart.StartIndex, meshPart.PrimitiveCount, transforms.Length);
                        
                        DebuggingInformation.polygonCount += meshPart.NumVertices;
                    }
                }
            }
        }
        #endregion
    }
}
