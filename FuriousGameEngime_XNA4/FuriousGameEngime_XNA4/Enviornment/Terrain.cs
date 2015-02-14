using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using FuriousGameEngime_XNA4.Cameras;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Jitter.Collision.Shapes;
using Microsoft.Xna.Framework.Content;
using FuriousGameEngime_XNA4.Screens;
using Jitter.Dynamics;
using Jitter.LinearMath;

namespace FuriousGameEngime_XNA4.Enviornment
{
    public struct VertexMultitextured
    {
        public Vector3 Position;
        public Vector3 Normal;
        public Vector4 TextureCoordinate;
        public Vector4 TexWeights;

        public static int SizeInBytes = (3 + 3 + 4 + 4) * sizeof(float);

        public static VertexDeclaration VertexDeclaration = new VertexDeclaration
        (
            new VertexElement(0, VertexElementFormat.Vector3, VertexElementUsage.Position, 0),
            new VertexElement(sizeof(float) * 3, VertexElementFormat.Vector3, VertexElementUsage.Normal, 0),
            new VertexElement(sizeof(float) * 6, VertexElementFormat.Vector4, VertexElementUsage.TextureCoordinate, 0),
            new VertexElement(sizeof(float) * 10, VertexElementFormat.Vector4, VertexElementUsage.TextureCoordinate, 1)
        );
    }

    class Terrain //:Entity // not sure if I should make the terrain an entity or not.
    {
        GameScreen _gameScreen;
        internal readonly HeightMapInfo mapInfo;
        internal readonly Model model;
        internal readonly RigidBody body;
        internal readonly float[,] shapeHeights;

        /// <summary>
        /// 1 = full detail. 0 = no detal
        /// </summary>
        float collisionDetail = .6f;
        
        /// <summary>
        /// the layout map of the textures. It takes the RGBA value for each pixel and applies the appropriate texture with blending
        /// </summary>
        Texture2D _textureMap;

        /// <summary>
        /// the texture that is applied in the area of the _textureMap.R value
        /// </summary>
        Texture2D _textureR;

        /// <summary>
        /// the texture that is applied in the area of the _textureMap.G value
        /// </summary>
        Texture2D _textureG;

        /// <summary>
        /// the texture that is applied in the area of the _textureMap.B value
        /// </summary>
        Texture2D _textureB;
        
        /// <summary>
        /// grabs heightmap info from model and adds a body to the physics world for collision
        /// </summary>
        internal Terrain(GameScreen gameScreen, Model model, Texture2D textureMap, Texture2D textureR, Texture2D textureG, Texture2D textureB, string name, Vector3 position)
        {
            _gameScreen = gameScreen;
            this.model = model;

            _textureMap = textureMap;
            _textureR = textureR;
            _textureG = textureG;
            _textureB = textureB;

            //gets the heightmap info out of the model
            mapInfo = model.Tag as HeightMapInfo;
            if (mapInfo == null)
            {
                throw new Exception("Heightmap info is null. Please make sure you are using the terrain importer on your heightmap.");
            }

            int width = (int)(mapInfo.width / mapInfo.scale * collisionDetail);
            shapeHeights = new float[width, width];
            for (int x = 0; x < width; ++x)
            {
                for (int z = 0; z < width; ++z)
                {
                    shapeHeights[x, z] = mapInfo.heights[(int)(x / collisionDetail), (int)(z / collisionDetail)];
                }
            }

            TerrainShape shape = new TerrainShape(shapeHeights, mapInfo.scale / collisionDetail);
            body = new RigidBody(shape);
            gameScreen.World.AddBody(body);
            body.IsStatic = true;
            body.Position = Conversion.ToJitterVector(new Vector3(position.X - mapInfo.width/2, 0, position.Z - mapInfo.height/2));
        }

        //TODO: Improve Heightmap
        //- break apary large heightmaps into smaller segments for culling
        //- terrain stiching for multiple heightmaps. Must be able to stitch maps regardless of constructor
        //- create overides that allows you to set only as may textures or colors as you want (add alpha channel?). Could add a string for technique and set that based on which constructor was used
        //- create an overide that will pass in a noise map (such as Perlin Noise) for procedural generation
        //- billboarding vegitation
        //- manage the inclusion of water on the heightmap, specifically rivers
        //- camera should have an effect applied when underwater
        //- water should have ripples and waves
        
        /// <summary>
        /// This is called when the game should draw itself.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        internal void Draw(Camera camera, Vector3 lightDirection)
        {
            foreach (ModelMesh mesh in model.Meshes)
            {
                foreach (Effect effect in mesh.Effects)
                {
                    //TODO: maybe I don't have to pass in the constats every time. Might speed up the draw.

                    // Specify which effect technique to use.
                    effect.CurrentTechnique = effect.Techniques["MultiTextured"];
                    effect.Parameters["TextureMap"].SetValue(_textureMap);
                    effect.Parameters["TextureR"].SetValue(_textureR);
                    effect.Parameters["TextureB"].SetValue(_textureB);
                    effect.Parameters["TextureG"].SetValue(_textureG);

                    effect.Parameters["View"].SetValue(camera.View);
                    effect.Parameters["Projection"].SetValue(camera.Projection);
                    effect.Parameters["World"].SetValue(Matrix.Identity);
                    effect.Parameters["WorldViewProjection"].SetValue(camera.View * camera.Projection);

                    lightDirection.Normalize();
                    effect.Parameters["xEnableLighting"].SetValue(true);
                    effect.Parameters["LightDir"].SetValue(lightDirection);
                    effect.Parameters["xAmbient"].SetValue(0.1f);

                    effect.Parameters["TerrainScale"].SetValue(mapInfo.scale);
                    effect.Parameters["TerrainWidth"].SetValue(mapInfo.width);
                }

                mesh.Draw();

                DebuggingInformation.polygonCount += mesh.MeshParts[0].NumVertices;
            }
        }
    }
}
