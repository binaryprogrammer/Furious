//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Text;
//using Microsoft.Xna.Framework.Content.Pipeline.Graphics;
//using Microsoft.Xna.Framework.Content.Pipeline;
//using System.ComponentModel;
//using Microsoft.Xna.Framework.Content.Pipeline.Processors;
//using System.IO;

//namespace CustomModelImporter
//{
//    /// <summary>
//    /// Custom content pipeline processor derives from the built-in
//    /// MaterialProcessor. This changes the material to use our custom
//    /// environment mapping effect, and also builds the environment map
//    /// texture in a special way.
//    /// </summary>
//    [ContentProcessor]
//    [DesignTimeVisible(false)]
//    public class CustomMaterialProcessor : MaterialProcessor
//    {
//        [DisplayName("Mapped Material")]
//        [Description("The material map applied to the model.")]

//        /// <summary>
//        /// Converts a material.
//        /// </summary>
//        public override MaterialContent Process(MaterialContent input, ContentProcessorContext context)
//        {
//            // Create a new effect material.
//            EffectMaterialContent customMaterial = new EffectMaterialContent();

//            //// Point the new material at our custom effect file.
//            //string effectFile = Path.GetFullPath("Effects//CartoonEffect.fx");

//            //customMaterial.Effect = new ExternalReference<EffectContent>(effectFile);

//            //// Copy texture data across from the original material.
//            //BasicMaterialContent basicMaterial = (BasicMaterialContent)input;

//            //if (basicMaterial.Texture != null)
//            //{
//            //    customMaterial.Textures.Add("Texture", basicMaterial.Texture);
//            //    customMaterial.OpaqueData.Add("TextureEnabled", true);
//            //}

//            // Chain to the base material processor.
//            return base.Process(customMaterial, context);
//        }


//        /// <summary>
//        /// Builds a texture for use by this material.
//        /// </summary>
//        //protected override ExternalReference<TextureContent> BuildTexture(string textureName, ExternalReference<TextureContent> texture, 
//        //    ContentProcessorContext context)
//        //{
//        //    // Use our custom CubemapProcessor for the environment map texture.
//        //    //if (textureName == "CartoonEffect")
//        //    //{
//        //    //    return context.BuildAsset<TextureContent, TextureContent>(texture, "CubemapProcessor");
//        //    //}

//        //    // Apply default processing to all other textures.
//        //    return base.BuildTexture(textureName, texture, context);
//        //}
//    }
//}
