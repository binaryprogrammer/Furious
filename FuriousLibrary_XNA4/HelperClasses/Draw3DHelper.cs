using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;
using FuriousLibrary_XNA4.Cameras;

namespace FuriousLibrary_XNA4.HelperClasses
{
    static public class Draw3DHelper
    {
        static DynamicVertexBuffer _instanceVertexBuffer;

        // To store instance transform matrices in a vertex buffer, we use this custom
        // vertex type which encodes 4x4 matrices as a set of four Vector4 values.
        static VertexDeclaration instanceVertexDeclaration = new VertexDeclaration
        (
            new VertexElement(0, VertexElementFormat.Vector4, VertexElementUsage.BlendWeight, 0),
            new VertexElement(16, VertexElementFormat.Vector4, VertexElementUsage.BlendWeight, 1),
            new VertexElement(32, VertexElementFormat.Vector4, VertexElementUsage.BlendWeight, 2),
            new VertexElement(48, VertexElementFormat.Vector4, VertexElementUsage.BlendWeight, 3)
        );

        #region Draw Functions

        /// <summary>
        /// Experimental Draw Function
        /// </summary>
        /// <param name="model"></param>
        /// <param name="camera"></param>
        /// <param name="worldMatrix"></param>
        /// <param name="polygonCount">Returns Poly Count</param>
        public static void DrawModel(Model model, Camera camera, Matrix worldMatrix, string technique, out int polygonCount)
        {
            int numberOfPolygons = 0;

            //Alan's Reccomendation to replace: Matrix localWorld = transforms[mesh.ParentBone.Index] * worldMatrix;
            model.Root.Transform = worldMatrix;

            // Look up the bone transform matrices.
            Matrix[] transforms = new Matrix[model.Bones.Count];
            model.CopyAbsoluteBoneTransformsTo(transforms);
            //model.CopyBoneTransformsTo(transforms);

            foreach (ModelMesh mesh in model.Meshes)
            {
                foreach (ModelMeshPart part in mesh.MeshParts)
                {
                    numberOfPolygons += part.PrimitiveCount;

                    part.Effect.CurrentTechnique = part.Effect.Techniques[technique];

                    //Matrix localWorld = transforms[mesh.ParentBone.Index] * worldMatrix;

                    part.Effect.Parameters["World"].SetValue(transforms[mesh.ParentBone.Index]);
                    part.Effect.Parameters["View"].SetValue(camera.ViewMatrix);
                    part.Effect.Parameters["Projection"].SetValue(camera.ProjectionMatrix);
                }

                mesh.Draw();
            }

            polygonCount = numberOfPolygons;
        }

        /// <summary>
        /// Helper for drawing the model using the specified effect technique.
        /// </summary>
        public static void DrawModel(Model model, Camera camera, Matrix world, string technique)
        {
            // Look up the bone transform matrices.
            Matrix[] transforms = new Matrix[model.Bones.Count];

            model.CopyAbsoluteBoneTransformsTo(transforms);

            // Draw the model.
            foreach (ModelMesh mesh in model.Meshes)
            {
                foreach (Effect effect in mesh.Effects)
                {
                    // Specify which effect technique to use.
                    effect.CurrentTechnique = effect.Techniques[technique];

                    Matrix localWorld = transforms[mesh.ParentBone.Index] * world;

                    effect.Parameters["World"].SetValue(localWorld);
                    effect.Parameters["View"].SetValue(camera.ViewMatrix);
                    effect.Parameters["Projection"].SetValue(camera.ProjectionMatrix);
                }

                mesh.Draw();
            }
        }

        /// <summary>
        /// Experimental Draw Function
        /// </summary>
        /// <param name="model"></param>
        /// <param name="camera"></param>
        /// <param name="worldMatrix"></param>
        /// <param name="polygonCount">Returns Poly Count</param>
        public static void DrawModel(Model model, Camera camera, Matrix worldMatrix, out int polygonCount)
        {
            int numberOfPolygons = 0;
            //Alan's Reccomendation to replace: Matrix localWorld = transforms[mesh.ParentBone.Index] * worldMatrix;
            model.Root.Transform = worldMatrix;

            // Look up the bone transform matrices.
            Matrix[] transforms = new Matrix[model.Bones.Count];
            model.CopyAbsoluteBoneTransformsTo(transforms);

            foreach (ModelMesh mesh in model.Meshes)
            {
                foreach (ModelMeshPart part in mesh.MeshParts)
                {
                    numberOfPolygons += part.PrimitiveCount;
                    part.Effect.Parameters["World"].SetValue(transforms[mesh.ParentBone.Index]);
                    part.Effect.Parameters["View"].SetValue(camera.ViewMatrix);
                    part.Effect.Parameters["Projection"].SetValue(camera.ProjectionMatrix);
                }

                mesh.Draw();
            }
            polygonCount = numberOfPolygons;

            ///test to see if it's drawing the factory
            //if (polygonCount >= 10000 && polygonCount <= 100000)
            //{
            //}
            model.Root.Transform = Matrix.Identity;
        }

        public static void DrawModel(Model model, Camera camera, Matrix worldMatrix)
        {
            //Alan's Reccomendation to replace: Matrix localWorld = transforms[mesh.ParentBone.Index] * worldMatrix;
            model.Root.Transform = worldMatrix;

            // Look up the bone transform matrices.
            Matrix[] transforms = new Matrix[model.Bones.Count];
            model.CopyAbsoluteBoneTransformsTo(transforms);

            foreach (ModelMesh mesh in model.Meshes)
            {
                foreach (ModelMeshPart part in mesh.MeshParts)
                {
                    part.Effect.Parameters["World"].SetValue(transforms[mesh.ParentBone.Index]);
                    part.Effect.Parameters["View"].SetValue(camera.ViewMatrix);
                    part.Effect.Parameters["Projection"].SetValue(camera.ProjectionMatrix);
                }

                mesh.Draw();
            }
            model.Root.Transform = Matrix.Identity;
        }

        /// <summary>
        /// A BasicEffect Draw Function
        /// </summary>
        /// <param name="model">the model to draw</param>
        /// <param name="camera"></param>
        /// <param name="worldMatrix"></param>
        /// <param name="fogColor">the color of the fog</param>
        /// <param name="fogStart">sets the minimum z value which ranges from 0 to 1</param>
        /// <param name="fogEnd">sets the maximum z value which ranges from 0 to 1</param>
        public static void DrawModel(Model model, Camera camera, Matrix worldMatrix, bool enableDefaultLighting, bool preferPerPixelLighting, bool fogEnabled, Vector3 fogColor, float fogStart, float fogEnd)
        {
            Matrix[] boneTransforms = new Matrix[model.Bones.Count];
            model.CopyAbsoluteBoneTransformsTo(boneTransforms);

            foreach (ModelMesh mesh in model.Meshes)
            {
                foreach (BasicEffect effect in mesh.Effects)
                {
                    effect.World = boneTransforms[mesh.ParentBone.Index] * worldMatrix;
                    effect.View = camera.ViewMatrix;
                    effect.Projection = camera.ProjectionMatrix;

                    if (enableDefaultLighting)
                        effect.EnableDefaultLighting();
                    effect.PreferPerPixelLighting = preferPerPixelLighting;

                    effect.FogEnabled = fogEnabled;
                    if (fogEnabled)
                    {
                        effect.FogColor = fogColor;
                        effect.FogStart = fogStart;
                        effect.FogEnd = fogEnd;
                    }
                }

                mesh.Draw();
            }
        }

        #region Experimental Draw Functions

        /// <summary>
        /// Efficiently draws several copies of a piece of geometry using hardware instancing.
        /// </summary>
        public static void DrawModelHardwareInstancing(Model model, Matrix[] modelBones, Camera camera, GraphicsDevice graphicsDevice, Matrix[] instances)
        {
            //if (instances.Length == 0)
            //    return;            
            
            //// If we have more instances than room in our vertex buffer, grow it to the neccessary size.
            //if ((_instanceVertexBuffer == null) ||
            //    (instances.Length > _instanceVertexBuffer.VertexCount))
            //{
            //    if (_instanceVertexBuffer != null)
            //        _instanceVertexBuffer.Dispose();

            //    _instanceVertexBuffer = new DynamicVertexBuffer(graphicsDevice, instanceVertexDeclaration,
            //                                                   instances.Length, BufferUsage.WriteOnly);
            //}

            //// Transfer the latest instance transform matrices into the instanceVertexBuffer.
            //_instanceVertexBuffer.SetData(instances, 0, instances.Length, SetDataOptions.Discard);

            foreach (ModelMesh mesh in model.Meshes)
            {
                foreach (ModelMeshPart meshPart in mesh.MeshParts)
                {
                    // Tell the GPU to read from both the model vertex buffer plus our instanceVertexBuffer.
                    graphicsDevice.SetVertexBuffers(
                        new VertexBufferBinding(meshPart.VertexBuffer, meshPart.VertexOffset, 0),
                        new VertexBufferBinding(_instanceVertexBuffer, 0, 1)
                    );

                    graphicsDevice.Indices = meshPart.IndexBuffer;

                    // Set up the instance rendering effect.
                    Effect effect = meshPart.Effect;

                    effect.CurrentTechnique = effect.Techniques["Toon"];

                    effect.Parameters["World"].SetValue(modelBones[mesh.ParentBone.Index]);
                    effect.Parameters["View"].SetValue(camera.ViewMatrix);
                    effect.Parameters["Projection"].SetValue(camera.ProjectionMatrix);

                    // Draw all the instance copies in a single call.
                    //foreach (EffectPass pass in effect.CurrentTechnique.Passes)
                    //{
                    //    pass.Apply();

                    //    graphicsDevice.DrawInstancedPrimitives(PrimitiveType.TriangleList, 0, 0, meshPart.NumVertices, meshPart.StartIndex, meshPart.PrimitiveCount, instances.Length);
                    //}
                }
            }
        }


        /// <summary>
        /// Draws several copies of a piece of geometry without using any special GPU instancing techniques at all. This just does a regular loop and issues several draw calls one after another.
        /// </summary>
        public static void DrawModelNoInstancing(Model model, Matrix[] modelBones, Camera camera, GraphicsDevice graphicsDevice, Matrix[] instances)
        {
            foreach (ModelMesh mesh in model.Meshes)
            {
                foreach (ModelMeshPart meshPart in mesh.MeshParts)
                {
                    graphicsDevice.SetVertexBuffer(meshPart.VertexBuffer, meshPart.VertexOffset);
                    graphicsDevice.Indices = meshPart.IndexBuffer;

                    // Set up the rendering effect.
                    Effect effect = meshPart.Effect;

                    effect.CurrentTechnique = effect.Techniques["Toon"];

                    effect.Parameters["View"].SetValue(camera.ViewMatrix);
                    effect.Parameters["Projection"].SetValue(camera.ProjectionMatrix);

                    EffectParameter transformParameter = effect.Parameters["World"];

                    // Draw a single instance copy each time around this loop.
                    for (int i = 0; i < instances.Length; i++)
                    {
                        transformParameter.SetValue(modelBones[mesh.ParentBone.Index] * instances[i]);

                        foreach (EffectPass pass in effect.CurrentTechnique.Passes)
                        {
                            pass.Apply();

                            graphicsDevice.DrawIndexedPrimitives(PrimitiveType.TriangleList, 0, 0, meshPart.NumVertices, meshPart.StartIndex, meshPart.PrimitiveCount);
                        }
                    }
                }
            }
        }
        #endregion
        #endregion

        #region Helper Functions

        /// <summary>
        /// Alters a model so it will draw using a custom effect, clearing everything assosiated with that model
        /// </summary>
        public static void ChangeEffectUsedByModel(Model model, Effect replacementEffect)
        {
            // Table mapping the original effects to our replacement versions.
            Dictionary<Effect, Effect> effectMapping = new Dictionary<Effect, Effect>();

            foreach (ModelMesh mesh in model.Meshes)
            {
                // Scan over all the effects currently on the mesh.
                foreach (BasicEffect oldEffect in mesh.Effects)
                {
                    // If we haven't already seen this effect...
                    if (!effectMapping.ContainsKey(oldEffect))
                    {
                        // Make a clone of our replacement effect. We can't just use
                        // it directly, because the same effect might need to be
                        // applied several times to different parts of the model using
                        // a different texture each time, so we need a fresh copy each
                        // time we want to set a different texture into it.
                        Effect newEffect = replacementEffect.Clone();

                        effectMapping.Add(oldEffect, newEffect);
                    }
                }

                // Now that we've found all the effects in use on this mesh,
                // update it to use our new replacement versions.
                foreach (ModelMeshPart meshPart in mesh.MeshParts)
                {
                    meshPart.Effect = effectMapping[meshPart.Effect];
                }
            }
        }

        //Use to create a dynamic enviornment map which is used to draw the world into a cube for reflection purposes
        //http://forums.create.msdn.com/forums/t/1530.aspx
        //public void DrawEnvironmentMap()
        //{
        //    // Check for event listeners for the environment map
        //    if (this.OnRenderCubemapFace == null)
        //        return;

        //    // Save the current target, and create one for the cube 
        //    RenderTarget MainRenderTarget = this.GraphicsDevice.GetRenderTarget(0);
        //    RenderTargetCube CubeRenderTarget = new RenderTargetCube
        //    (
        //       this.GraphicsDevice,
        //       256,
        //       1,
        //       SurfaceFormat.Color
        //    );

        //    // Loop though all the cubemap faces
        //    for (int i = 0; i < 6; i++)
        //    {
        //        // Find the correct cubemap face
        //        CubeMapFace cubeMapFace = (CubeMapFace)i;

        //        // Save the data from texture file
        //        this.GraphicsDevice.SetRenderTarget(0, CubeRenderTarget, cubeMapFace);
        //        this.GraphicsDevice.Clear(Color.CornflowerBlue);

        //        // Invoke the render cubemapface event, allowing our scene to control the reflected scene
        //        this.OnRenderCubemapFace(cubeMapFace);
        //    }
        //    // Resolve the render target so the texture can be read
        //    this.GraphicsDevice.ResolveRenderTarget(0);

        //    // Save the EnvironmentMap
        //    this.EnvironmentMap = CubeRenderTarget.GetTexture();

        //    // Reset the rendertarget on the graphicsdevice
        //    this.GraphicsDevice.SetRenderTarget(0, MainRenderTarget as RenderTarget2D);
        //}

        //Same Idea?
        //void DrawSceneInCubeMap2()
        //{
        //    DepthStencilBuffer flatDepthBuffer = ScreenManager.GraphicsDevice.DepthStencilBuffer;
        //    ScreenManager.GraphicsDevice.DepthStencilBuffer = cubeMapDepthBuffer;
        //    ScreenManager.GraphicsDevice.SetRenderTarget(0, this.cubeMapTarget, CubeMapFace.PositiveZ);
        //    ScreenManager.GraphicsDevice.Clear(Color.White);
        //    MakeMyCameraLookPositiveZ();
        //    RenderMyStuff();

        //    ScreenManager.GraphicsDevice.SetRenderTarget(0, this.cubeMapTarget, CubeMapFace.PositiveX);
        //    ScreenManager.GraphicsDevice.Clear(Color.White);
        //    MakeMyCameraLookPositiveX();
        //    RenderMyStuff();
        //    /* So on for the other faces */

        //    /*Restore old rendering behaviour*/
        //    ScreenManager.GraphicsDevice.SetRenderTarget(0, null);
        //    ScreenManager.GraphicsDevice.DepthStencilBuffer = flatDepthBuffer;

        //    cubeMap = cubeMapTarget.GetTexture();
        //} 

        ///// <summary>
        ///// Helper applies the edge detection and pencil sketch postprocess effect.
        ///// </summary>
        //internal void ApplyPostprocess()
        //{
        //    EffectParameterCollection parameters = postprocessEffect.Parameters;
        //    string effectTechniqueName;

        //    // Set effect parameters controlling the pencil sketch effect.
        //    if (Settings.EnableSketch)
        //    {
        //        parameters["SketchThreshold"].SetValue(Settings.SketchThreshold);
        //        parameters["SketchBrightness"].SetValue(Settings.SketchBrightness);
        //        parameters["SketchJitter"].SetValue(sketchJitter);
        //        parameters["SketchTexture"].SetValue(sketchTexture);
        //    }

        //    // Set effect parameters controlling the edge detection effect.
        //    if (Settings.EnableEdgeDetect)
        //    {
        //    Vector2 resolution = new Vector2(sceneRenderTarget.Width,
        //                                     sceneRenderTarget.Height);

        //    Texture2D normalDepthTexture = normalDepthRenderTarget;

        //    parameters["EdgeWidth"].SetValue(Settings.EdgeWidth);
        //    parameters["EdgeIntensity"].SetValue(Settings.EdgeIntensity);
        //    parameters["ScreenResolution"].SetValue(resolution);
        //    parameters["NormalDepthTexture"].SetValue(normalDepthTexture);

        //    // Choose which effect technique to use.
        //    if (Settings.EnableSketch)
        //    {
        //        if (Settings.SketchInColor)
        //           effectTechniqueName = "EdgeDetectColorSketch";
        //        else
        //            effectTechniqueName = "EdgeDetectMonoSketch";
        //    }
        //    else
        //    effectTechniqueName = "EdgeDetect";
        //    }
        //    else
        //    {
        //        // If edge detection is off, just pick one of the sketch techniques.
        //        if (Settings.SketchInColor)
        //            effectTechniqueName = "ColorSketch";
        //        else
        //            effectTechniqueName = "MonoSketch";
        //    }

        //    // Activate the appropriate effect technique.
        //    postprocessEffect.CurrentTechnique = postprocessEffect.Techniques[effectTechniqueName];

        //    // Draw a fullscreen sprite to apply the postprocessing effect.
        //    spriteBatch.Begin(0, BlendState.Opaque, null, null, null, postprocessEffect);
        //    spriteBatch.Draw(sceneRenderTarget, Vector2.Zero, Color.White);
        //    spriteBatch.End();
        //}
        #endregion

        #region Get Bounding Volumes
        /// <summary>
        /// Returns a Bounding Sphere that encomapses a model
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        public static BoundingSphere GetBoundingSphereFromModel(Model model, Matrix worldMatrix)
        {
            BoundingSphere ret = new BoundingSphere();
            BoundingSphere temp;

            float num;

            // Look up the bone transform matrices.
            Matrix[] transforms = new Matrix[model.Bones.Count];
            model.CopyAbsoluteBoneTransformsTo(transforms);

            for (int i = 0; i < model.Meshes.Count; ++i)
            {
                //model.Meshes[i].ParentBone.Transform.Decompose(out scale, out orientation, out center);

                //temp = model.Meshes[i].BoundingSphere;
                //temp.Center = center + worldMatrix.Translation;
                //num = (float)Math.Sqrt(Vector3.Dot(scale, scale));
                //temp.Radius *= num;

                Matrix localMatrix = transforms[model.Meshes[i].ParentBone.Index] * worldMatrix;
                temp.Center = Vector3.Transform(model.Meshes[i].BoundingSphere.Center, localMatrix);
                num = model.Meshes[i].BoundingSphere.Radius * localMatrix.Forward.Length();
                temp.Radius = num;

                if (i == 0)
                {
                    ret = temp;
                }
                else
                {
                    ret = BoundingSphere.CreateMerged(ret, temp);
                }
            }

            return ret;
        }

        //public static BoundingSphere GetBoundingSphereFromVerticies(Model model, Matrix worldMatrix)
        //{
        //    BoundingSphere ret = new BoundingSphere();
        //    BoundingSphere temp;

        //    //Vector3 scale;
        //    //Vector3 center;
        //    //Quaternion orientation;

        //    //float num;

        //    // Look up the bone transform matrices.
        //    //Matrix[] transforms = new Matrix[model.Bones.Count];
        //    //model.CopyAbsoluteBoneTransformsTo(transforms);

        //    for (int i = 0; i < model.Meshes.Count; ++i)
        //    {
        //        //model.Meshes[i].ParentBone.Transform.Decompose(out scale, out orientation, out center);

        //        temp = model.Meshes[i].BoundingSphere;
        //        //temp.Center = center + worldMatrix.Translation;
        //        //num = (float)Math.Sqrt(Vector3.Dot(scale, scale));
        //        //temp.Radius *= num;

        //        //Matrix localMatrix = transforms[model.Meshes[i].ParentBone.Index] * worldMatrix;
        //        //temp.Center = Vector3.Transform(model.Meshes[i].BoundingSphere.Center, localMatrix);
        //        //num = model.Meshes[i].BoundingSphere.Radius * localMatrix.Forward.Length();
        //        //temp.Radius = num;

        //        if (i == 0)
        //        {
        //            ret = temp;
        //        }
        //        else
        //        {
        //            ret = BoundingSphere.CreateMerged(ret, temp);
        //        }
        //    }

        //    return ret;
        //}

        public static BoundingSphere GetBoundingSphereFromVertices(Model model, Matrix worldTransform)
        {
            // Initialize minimum and maximum corners of the bounding box to max and min values
            Vector3 min = new Vector3(float.MaxValue, float.MaxValue, float.MaxValue);
            Vector3 max = new Vector3(float.MinValue, float.MinValue, float.MinValue);
            Vector3 center = Vector3.Zero;
            float radius = 0;

            // For each mesh of the model
            foreach (ModelMesh mesh in model.Meshes)
            {
                Matrix boneMatrix = GetBoneTransform(mesh.ParentBone);

                foreach (ModelMeshPart meshPart in mesh.MeshParts)
                {
                    // Vertex buffer parameters
                    int vertexStride = meshPart.VertexBuffer.VertexDeclaration.VertexStride;
                    int vertexBufferSize = meshPart.NumVertices * vertexStride;

                    // Get vertex data as float
                    float[] vertexData = new float[vertexBufferSize / sizeof(float)];
                    meshPart.VertexBuffer.GetData<float>(vertexData);



                    // Iterate through vertices (possibly) growing bounding box, all calculations are done in world space
                    for (int i = 0; i < vertexBufferSize / sizeof(float); i += vertexStride / sizeof(float))
                    {
                        Vector3 transformedPosition = Vector3.Transform(new Vector3(vertexData[i], vertexData[i + 1], vertexData[i + 2]), boneMatrix * worldTransform);

                        min = Vector3.Min(min, transformedPosition);
                        max = Vector3.Max(max, transformedPosition);

                        radius = Vector3.Distance(min, max)/2;
                        center = min + (max - min) / 2;
                    }
                }
            }

            // Create and return bounding box
            return new BoundingSphere(center, radius);
        }

        public static BoundingBox GetBoundingBoxFromVertices(Model model, Matrix worldTransform)
        {
            // Initialize minimum and maximum corners of the bounding box to max and min values
            Vector3 min = new Vector3(float.MaxValue, float.MaxValue, float.MaxValue);
            Vector3 max = new Vector3(float.MinValue, float.MinValue, float.MinValue);

            // For each mesh of the model
            foreach (ModelMesh mesh in model.Meshes)
            {
                //Console.WriteLine(mesh.Name.ToString());

                Matrix boneMatrix = GetBoneTransform(mesh.ParentBone);

                foreach (ModelMeshPart meshPart in mesh.MeshParts)
                {
                    // Vertex buffer parameters
                    int vertexStride = meshPart.VertexBuffer.VertexDeclaration.VertexStride;
                    int vertexBufferSize = meshPart.NumVertices * vertexStride;

                    // Get vertex data as float
                    float[] vertexData = new float[vertexBufferSize / sizeof(float)];
                    meshPart.VertexBuffer.GetData<float>(vertexData);

                    

                    // Iterate through vertices (possibly) growing bounding box, all calculations are done in world space
                    for (int i = 0; i < vertexBufferSize / sizeof(float); i += vertexStride / sizeof(float))
                    {
                        Vector3 transformedPosition = Vector3.Transform(new Vector3(vertexData[i], vertexData[i + 1], vertexData[i + 2]), boneMatrix * worldTransform);

                        min = Vector3.Min(min, transformedPosition);
                        max = Vector3.Max(max, transformedPosition);
                        //Console.WriteLine("min: " + min.ToString());
                        //Console.WriteLine("max: " + max.ToString());
                    }
                }
            }

            // Create and return bounding box
            return new BoundingBox(min, max);
        }

        private static Matrix GetBoneTransform(ModelBone bone)
        {
            Matrix ret = bone.Transform;
            ModelBone iterator = bone.Parent;
            while (iterator != null)
            {
                ret = iterator.Transform * ret;
                iterator = iterator.Parent;
            }

            return ret;
        }

        //public static BoundingSphere GetBoundingBoxFromVertices(Model model, Matrix worldMatrix)
        //{
        //    BoundingSphere ret = new BoundingSphere();
        //    BoundingSphere temp;

        //    //Well if everything in your vertex buffer is made up of float's, then you can get your VB as an array of float's and just go through that.  
        //    //Usually all you need is position, which is typically the first 3 float's in the VB.  So your loop would be something like this...

        //    //for (int i = 0; i < vb.SizeInBytes / sizeof(float); i += vertexStride / sizeof(float))
        //    //{
        //    //    Vector3 position = new Vector3();
        //    //    position.X = vbData[i];
        //    //    position.Y = vbData[i + 1];
        //    //    position.Z = vbData[i + 2];
        //    //}

        //    return ret;
        //}
        #endregion
    }
}
