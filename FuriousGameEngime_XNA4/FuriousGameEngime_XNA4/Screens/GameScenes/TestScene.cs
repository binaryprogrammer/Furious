using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Jitter.Dynamics;
using Jitter.Collision.Shapes;
using Jitter.LinearMath;
using FuriousGameEngime_XNA4.GameEntities;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using FuriousGameEngime_XNA4.Enviornment;

namespace FuriousGameEngime_XNA4.Screens.GameScenes
{
    public class TestScene : Scene
    {
        public TestScene(GameScreen gameScreen)
            : base(gameScreen)
        {

        }

        public override void LoadContent(ContentManager Content)
        {

        }

        public override void Build()
        {
            //TODO: load the assosiated level here
        }

        public override void Destroy()
        {
            RemoveGround();
        }

        public override void Draw(GameTime gameTime, SpriteBatch spriteBatch)
        {
            for (int i = 0; i < gameScreen.entities.Count; ++i)
            {
                gameScreen.entities[i].Draw(gameTime, spriteBatch);
            }
            base.Draw(gameTime, spriteBatch);
        }
    }
}
