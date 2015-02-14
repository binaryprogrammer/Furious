using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework.Content.Pipeline.Graphics;
using Microsoft.Xna.Framework.Content.Pipeline.Processors;
using Microsoft.Xna.Framework.Content.Pipeline;
using System.IO;
using Microsoft.Xna.Framework;
using System.ComponentModel;

namespace HeightmapImporter
{
    /// <summary>
    /// Custom content processor for creating terrain meshes. Given an
    /// input heightfield texture, this processor uses the MeshBuilder
    /// class to programatically generate terrain geometry.
    /// </summary>
    [ContentProcessor]
    public class FuriousTerrainProcessor : ContentProcessor<Texture2DContent, ModelContent>
    {
        #region Processor Parameters
        /// <summary>
        /// Controls the scale of the terrain. This will be the distance between vertices in the finished terrain mesh.
        /// </summary>
        [DefaultValue(30.0f)]
        [Description("The distance between vertices in the finished terrain mesh.")]
        public float Scale
        {
            get { return _scale; }
            set { _scale = value; }
        }
        float _scale = 30.0f;
        
        /// <summary>
        /// Controls the height of the terrain. The heights of the vertices in the finished mesh will vary between 0 and -Bumpiness.
        /// </summary>
        [DefaultValue(640.0f)]
        [Description("Controls the height of the terrain.")]
        public float Bumpiness
        {
            get { return _bumpiness; }
            set { _bumpiness = value; }
        }
        float _bumpiness = 640.0f;
        
        /// <summary>
        /// Controls the how often the texture applied to the terrain will be repeated.
        /// </summary>
        [DefaultValue(.1f)]
        [Description("Controls how often the texture will be repeated across the terrain.")]
        public float TexCoordScale
        {
            get { return _texCoordScale; }
            set { _texCoordScale = value; }
        }
        float _texCoordScale = 0.1f;
        
        /// <summary>
        /// Controls the texture that will be applied to the terrain. If no value is supplied, a texture will not be applied.
        /// </summary>
        [DefaultValue("texture.bmp")]
        [Description("Controls the texture that will be applied to the terrain. If no value is supplied, a texture will not be applied.")]
        [DisplayName("Terrain Texture")]
        public string TerrainTexture
        {
            get { return _texture; }
            set { _texture = value; }
        }
        string _texture = "texture.bmp";



        #endregion

        /// <summary>
        /// Generates a terrain mesh from an input heightfield texture.
        /// </summary>
        public override ModelContent Process(Texture2DContent input, ContentProcessorContext context)
        {
            PixelBitmapContent<float> heightfield;
            
            // Convert the input texture to float format, for ease of processing.
            input.ConvertBitmapType(typeof(PixelBitmapContent<float>));

            heightfield = (PixelBitmapContent<float>)input.Mipmaps[0];
            MeshBuilder builder = MeshBuilder.StartMesh("terrain");

            Vector3[,] positions = new Vector3[heightfield.Width, heightfield.Height];

            // Create the terrain vertices.
            for (int z = 0; z < heightfield.Height; z++)
            {
                for (int x = 0; x < heightfield.Width; x++)
                {
                    // position the vertices so that the heightfield is centered around x=0, z=0
                    positions[x, z].X = _scale * (x - ((heightfield.Width - 1) / 2.0f)) + heightfield.Width;
                    positions[x, z].Z = _scale * (z - ((heightfield.Height - 1) / 2.0f)) + heightfield.Height;

                    positions[x, z].Y = (heightfield.GetPixel(x, z) - 1) * _bumpiness;
                }
            }

            #region Terrain Smooting
            Vector3[,] newPositions = positions;

            // Smoothing Passes
            for (int i = 0; i < 3; ++i)
            {
                //Flatten Terrain
                for (int z = 0; z < heightfield.Height; ++z)
                {
                    for (int x = 0; x < heightfield.Width; ++x)
                    {
                        float verticieCumulitiveHeight = 0;
                        int adjacentVerticies = 0;

                        bool validNegativeY = z - 1 > 0;
                        bool validNegativeX = x - 1 > 0;
                        bool validPositiveY = z + 1 < heightfield.Height;
                        bool validPositiveX = x + 1 < heightfield.Width;

                        //Check the left
                        if (validNegativeX)
                        {
                            //Left
                            verticieCumulitiveHeight += positions[x - 1, z].Y;
                            ++adjacentVerticies;

                            if (validPositiveY)
                            {
                                //Upper Left
                                verticieCumulitiveHeight += positions[x - 1, z + 1].Y;
                                ++adjacentVerticies;
                            }
                            if (validNegativeY)
                            {
                                //Lower Left
                                verticieCumulitiveHeight += positions[x - 1, z - 1].Y;
                                ++adjacentVerticies;
                            }
                        }

                        //Check the Right
                        if (validPositiveX)
                        {
                            //Right
                            verticieCumulitiveHeight += positions[x + 1, z].Y;
                            ++adjacentVerticies;

                            if (validPositiveY)
                            {
                                //Upper Right
                                verticieCumulitiveHeight += positions[x + 1, z + 1].Y;
                                ++adjacentVerticies;
                            }

                            if (validNegativeY)
                            {
                                //Lower Right
                                verticieCumulitiveHeight += positions[x + 1, z - 1].Y;
                                ++adjacentVerticies;
                            }
                        }

                        if (validPositiveY)
                        {
                            //Up
                            verticieCumulitiveHeight += positions[x, z + 1].Y;
                            ++adjacentVerticies;
                        }

                        if (validNegativeY)
                        {
                            //Bottom
                            verticieCumulitiveHeight += positions[x, z - 1].Y;
                            ++adjacentVerticies;
                        }

                        //Center position
                        newPositions[x, z].Y = (verticieCumulitiveHeight / adjacentVerticies);
                    }
                }

                for (int z = 0; z < heightfield.Height; ++z)
                {
                    for (int x = 0; x < heightfield.Width; ++x)
                    {
                        positions[x, z].Y = newPositions[x, z].Y;
                    }
                }
            }
            #endregion

            // Create a material, and point it at our terrain texture.
            BasicMaterialContent material = new BasicMaterialContent();
            material.SpecularColor = new Vector3(.4f, .4f, .4f);

            if (!string.IsNullOrEmpty(TerrainTexture))
            {
                string directory = Path.GetDirectoryName(input.Identity.SourceFilename);
                string texture = Path.Combine(directory, TerrainTexture);

                material.Texture = new ExternalReference<TextureContent>(texture);
            }

            builder.SetMaterial(material);

            // Create a vertex channel for holding texture coordinates.
            int texCoordId = builder.CreateVertexChannel<Vector2>(VertexChannelNames.TextureCoordinate(0));

            // Create the individual triangles that make up our terrain.
            for (int z = 0; z < heightfield.Height - 1; z++)
            {
                for (int x = 0; x < heightfield.Width - 1; x++)
                {
                    AddVertex(builder, texCoordId, heightfield.Width, x, z);
                    AddVertex(builder, texCoordId, heightfield.Width, x + 1, z);
                    AddVertex(builder, texCoordId, heightfield.Width, x + 1, z + 1);

                    AddVertex(builder, texCoordId, heightfield.Width, x, z);
                    AddVertex(builder, texCoordId, heightfield.Width, x + 1, z + 1);
                    AddVertex(builder, texCoordId, heightfield.Width, x, z + 1);
                }
            }

            // Chain to the ModelProcessor so it can convert the mesh we just generated.
            MeshContent terrainMesh = builder.FinishMesh();

            ModelContent model = context.Convert<MeshContent, ModelContent>(terrainMesh, "ModelProcessor");

            // generate information about the height map, and attach it to the finished model's tag.
            model.Tag = new HeightMapInfoContent(terrainMesh, _scale, heightfield.Width, heightfield.Height);

            return model;
        }

        /// <summary>
        /// Helper for adding a new triangle vertex to a MeshBuilder, along with an associated texture coordinate value.
        /// </summary>
        void AddVertex(MeshBuilder builder, int texCoordId, int w, int x, int y)
        {
            builder.SetVertexChannelData(texCoordId, new Vector2(x, y) * TexCoordScale);

            builder.AddTriangleVertex(x + y * w);
        }
    }
}
