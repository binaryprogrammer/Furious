using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using FuriousLibrary_XNA4.Cameras;

namespace FuriousLibrary_XNA4.ModelManager
{
    class Models
    {
        /// <summary>
        /// a reference to our game
        /// </summary>
        Game _game;

        /// <summary>
        /// a reference to our content manager
        /// </summary>
        readonly ContentManager _content;

        /// <summary>
        /// allows us to search for the modelBase by name
        /// </summary>
        internal readonly Dictionary<string, ModelBase> nameToModelBase = new Dictionary<string, ModelBase>();

        /// <summary>
        /// allows us to seach model base by model
        /// </summary>
        internal readonly Dictionary<Model, ModelBase> modelToModelBase = new Dictionary<Model, ModelBase>();

        /// <summary>
        /// a list of all the baseModels we have loaded in the game
        /// </summary>
        internal readonly List<ModelBase> baseModels;

        internal Models(Game game, ContentManager manager)
        {
            _game = game;
            _content = manager;
            baseModels = new List<ModelBase>();
        }

        /// <summary>
        /// load a new baseModel to the world
        /// </summary>
        /// <param name="path"></param>
        /// <returns></returns>
        internal ModelBase Load(string path)
        {
            ModelBase ret;
            if (!nameToModelBase.TryGetValue(path, out ret))
            {
                ret = new ModelBase(_game, _content.Load<Model>(path));
                nameToModelBase.Add(path, ret);
                modelToModelBase.Add(ret.model, ret);
                baseModels.Add(ret);
            }
            return ret;
        }

        /// <summary>
        /// draw all our baseModels
        /// </summary>
        /// <param name="camera"></param>
        internal void Draw(Camera camera, string technique)
        {
            for (int i = 0; i < baseModels.Count; ++i)
            {
                baseModels[i].DrawAllInstances(_game.GraphicsDevice, technique, camera.ViewMatrix, camera.ProjectionMatrix);
            }
        }
    }
}
