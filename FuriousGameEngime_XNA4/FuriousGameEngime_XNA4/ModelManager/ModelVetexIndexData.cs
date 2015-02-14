using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;

namespace FuriousGameEngime_XNA4.ModelManager
{
    public class ModelVetexIndexData
    {
        public VertexBuffer vertexBuffer;

        public ModelVetexIndexData(VertexBuffer buffer)
        {
            vertexBuffer = buffer;
        }

    }

    /// <summary>
    /// This class will load the HeightMapInfo when the game starts. This class needs 
    /// to match the HeightMapInfoWriter.
    /// </summary>
    //public class HeightMapInfoReader : ContentTypeReader<ModelVetexIndexData>
    //{
    //    protected override ModelVetexIndexData Read(ContentReader input, ModelVetexIndexData existingInstance)
    //    {
    //        //float terrainScale = input.ReadSingle();
    //        //int width = input.ReadInt32();
    //        //int height = input.ReadInt32();
    //        //float[,] heights = new float[width, height];
    //        //Vector3[,] normals = new Vector3[width, height];

    //        //VertexBuffer buffer =  new VertexBuffer(graphicsDevice, ;

    //        //VertexDeclaration dec = new VertexDeclaration(

    //        //return new ModelVetexIndexData(buffer);
    //    }
    //}
}
