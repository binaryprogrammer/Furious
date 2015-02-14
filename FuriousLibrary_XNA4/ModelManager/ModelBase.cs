using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using FuriousLibrary_XNA4.Collision;
using Microsoft.Xna.Framework.Graphics;
using FuriousLibrary_XNA4.HelperClasses;

namespace FuriousLibrary_XNA4.ModelManager
{
    public class ModelBase
    {
        #region Variables
        Game _game;

        /// <summary>
        /// a reference of the loaded model
        /// </summary>
        internal readonly Model model;

        /// <summary>
        /// an instance of the model with a transform
        /// </summary>
        internal List<ModelInstance> instances = new List<ModelInstance>();

        /// <summary>
        /// the model, and inherintly all the instances bounding boxes.
        /// </summary>
        BoundingOrientedBox _boundingBox = new BoundingOrientedBox();

        /// <summary>
        /// the x, y, and z scale of the bounding box
        /// </summary>
        internal Vector3 boundingBoxScale = Vector3.One;

        /// <summary>
        /// the rotation of the bounding box
        /// </summary>
        internal Quaternion boundingBoxRotation = Quaternion.Identity;

        /// <summary>
        /// the position of the bounding box. set to the center of the object as default
        /// </summary>
        internal Vector3 boundingBoxPosition = Vector3.Zero;

        /// <summary>
        /// the draw offset for all model instances
        /// </summary>
        internal Vector3 offset = Vector3.Zero;

        /// <summary>
        /// all the transforms of each instance of this baseModel
        /// </summary>
        Matrix[] _instanceTransforms;

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

        internal ModelBase(Game game, Model model)
        {
            _game = game;
            this.model = model;
            _instancedModelBones = new Matrix[model.Bones.Count];
            model.CopyAbsoluteBoneTransformsTo(_instancedModelBones);
            ResetBoundingBox();
        }

        /// <summary>
        /// the model, and inherintly all the instances bounding boxes.
        /// </summary>
        internal BoundingOrientedBox OrientedBoundingBox
        {
            get
            {
                BoundingOrientedBox transformed = _boundingBox;
                transformed.Center = Vector3.Transform(boundingBoxPosition, Matrix.Identity);
                transformed.HalfExtent *= boundingBoxScale;
                transformed.Orientation = boundingBoxRotation;

                return transformed;
            }
        }

        internal void ResetBoundingBox()
        {
            _boundingBox = BoundingOrientedBox.CreateFromBoundingBox(Draw3DHelper.GetBoundingBoxFromVertices(model, Matrix.Identity));

            boundingBoxPosition = _boundingBox.Center;
            boundingBoxScale = Vector3.One;
            boundingBoxRotation = Quaternion.Identity;
        }

        /// <summary>
        /// Creates a new instance of the modelBase
        /// </summary>
        /// <returns> a new Model Instance</returns>
        internal ModelInstance GetInstance()
        {
            ModelInstance ret = new ModelInstance(_game, this);
            instances.Add(ret);

            // Gather instance transform matrices into a single array.
            UpdateDrawTransforms();

            return ret;
        }

        /// <summary>
        /// because we keep an array of transforms that we draw from we need to update this list if any of the transforms change
        /// </summary>
        internal void UpdateDrawTransforms(ModelInstance instance)
        {
            for (int i = 0; i < instances.Count; i++)
            {
                if (instances[i] == instance)
                {
                    _instanceTransforms[i] = instance.Transform * Matrix.CreateTranslation(offset);
                }
            }
        }

        internal void UpdateDrawTransforms()
        {
            Array.Resize(ref _instanceTransforms, instances.Count);

            for (int i = 0; i < instances.Count; i++)
            {
                _instanceTransforms[i] = instances[i].Transform * Matrix.CreateTranslation(offset);
            }
        }

        /// <summary>
        /// Efficiently draws several copies of a piece of geometry using hardware instancing.
        /// </summary>
        internal void DrawAllInstances(GraphicsDevice graphicsDevice, string technique, Matrix view, Matrix projection)
        {
            if (_instanceTransforms == null || _instanceTransforms.Length == 0)
                return;

            // If we have more instances than room in our vertex buffer, grow it to the neccessary size.
            if ((_instanceVertexBuffer == null) || (_instanceTransforms.Length > _instanceVertexBuffer.VertexCount))
            {
                if (_instanceVertexBuffer != null)
                    _instanceVertexBuffer.Dispose();

                _instanceVertexBuffer = new DynamicVertexBuffer(graphicsDevice, instanceVertexDeclaration, _instanceTransforms.Length, BufferUsage.WriteOnly);
            }

            // Transfer the latest instance transform matrices into the instanceVertexBuffer.
            _instanceVertexBuffer.SetData(_instanceTransforms, 0, _instanceTransforms.Length, SetDataOptions.Discard);

            foreach (ModelMesh mesh in model.Meshes)
            {
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
                        effect.CurrentTechnique = effect.Techniques[technique];

                        effect.Parameters["World"].SetValue(_instancedModelBones[mesh.ParentBone.Index]);
                        effect.Parameters["View"].SetValue(view);
                        effect.Parameters["Projection"].SetValue(projection);
                    }

                    // Draw all the instance copies in a single call.
                    foreach (EffectPass pass in effect.CurrentTechnique.Passes)
                    {
                        pass.Apply();

                        graphicsDevice.DrawInstancedPrimitives(PrimitiveType.TriangleList, 0, 0, meshPart.NumVertices,
                            meshPart.StartIndex, meshPart.PrimitiveCount, _instanceTransforms.Length);
                    }
                }
            }
        }
    }
}
