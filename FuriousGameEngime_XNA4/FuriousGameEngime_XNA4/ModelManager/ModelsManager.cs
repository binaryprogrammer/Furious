using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using FuriousGameEngime_XNA4.Cameras;
using FuriousGameEngime_XNA4.Screens;
using System.IO;

namespace FuriousGameEngime_XNA4.ModelManager
{
    class ModelsManager
    {
        /// <summary>
        /// a reference to our game
        /// </summary>
        GameScreen _gameScreen;

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
        //internal readonly Dictionary<Model, ModelBase> modelToModelBase = new Dictionary<Model, ModelBase>();

        /// <summary>
        /// a list of all the baseModels we have loaded in the game
        /// </summary>
        internal readonly List<ModelBase> baseModels;

        internal ModelsManager(GameScreen gameScreen, ContentManager manager)
        {
            _gameScreen = gameScreen;
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
                string fileName = Path.GetFileNameWithoutExtension(path);
                ret = new ModelBase(_gameScreen, _content.Load<Model>(path), fileName);
                XMLReaderWriter.AddPhysicsObjectProperties(_content, fileName);

                PhysicsProperties property = XMLReaderWriter.PhysicsProperties(fileName);

                ret.bodyBase.AffectedByGravity = property.AffectedByGravity;
                ret.bodyBase.AllowDeactivation = property.AllowDeactivation;
                ret.bodyBase.DynamicFriction = property.DynamicFriction;
                ret.bodyBase.IsGhost = property.IsGhost;
                ret.bodyBase.IsStatic = property.IsStatic;
                ret.bodyBase.Restitution = property.Restitiution;
                ret.bodyBase.StaticFriction = property.StaticFriction;

                nameToModelBase.Add(path, ret);
                //modelToModelBase.Add(ret.model, ret);
                baseModels.Add(ret);
            }
            return ret;
        }

        /// <summary>
        /// draw all our baseModels
        /// </summary>
        /// <param name="camera"></param>
        internal void Draw(Camera camera)
        {
            for (int i = 0; i < baseModels.Count; ++i)
            {
                baseModels[i].DrawAllInstances(_gameScreen.GraphicsDevice, camera.View, camera.Projection);
            }
        }
    }
}
