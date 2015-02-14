using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Jitter.Collision.Shapes;
using Jitter.Dynamics;
using Jitter.LinearMath;
using FuriousGameEngime_XNA4.Screens;
namespace FuriousGameEngime_XNA4.GameEntities
{
    public class TerrainObject
    {
        GameScreen gameScreen;
        TerrainPrimitive primitive;
        RigidBody terrainBody;

        public TerrainObject(GameScreen game)
        {
            this.gameScreen = game;
        }

        public void Initialize()
        {
            primitive = new TerrainPrimitive(gameScreen.GraphicsDevice, (int a, int b) =>
                { return (float)(Math.Sin(a * 0.1f) * Math.Cos(b * 0.1f))*3; });
            
            TerrainShape terrainShape = new TerrainShape(primitive.heights, 1.0f, 1.0f);

            terrainBody = new RigidBody(terrainShape);
            terrainBody.IsStatic = true;
            terrainBody.Tag = true;

            gameScreen.World.AddBody(terrainBody);
        }

        public void Draw(GameTime gameTime)
        {
            //effect.DiffuseColor = Color.Blue.ToVector3();
            //primitive.AddWorldMatrix(worldMatrix);
            //primitive.Draw(effect);
        }
    }
}
