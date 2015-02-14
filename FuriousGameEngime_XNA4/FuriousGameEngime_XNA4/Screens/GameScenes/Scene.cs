using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using FuriousGameEngime_XNA4.Screens;
using Jitter.Dynamics;
using Jitter.Collision.Shapes;
using Jitter.LinearMath;
using FuriousGameEngime_XNA4.GameEntities;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Content;

namespace FuriousGameEngime_XNA4.Screens.GameScenes
{
    public abstract class Scene
    {
        public GameScreen gameScreen { get; private set; }

        public Scene(GameScreen game)
        {
            this.gameScreen = game;
        }

        public abstract void Build();
        public abstract void Destroy();
        public abstract void LoadContent(ContentManager Content);

        public void Restore()
        {
            Destroy();
            Build();
        }

        protected RigidBody ground = null;

        public void AddGround()
        {
            //ground = new RigidBody(new BoxShape(new JVector(300, 20, 300)));
            //ground.Position = new JVector(0, -10, 0);
            //ground.IsStatic = true; gameScreen.World.AddBody(ground);
            //ground.DynamicFriction = 0.0f;
        }

        public void RemoveGround()
        {
            gameScreen.World.RemoveBody(ground);
        }

        public virtual void Draw(GameTime gameTime, SpriteBatch spriteBatch) { }
    }
}
