using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using FuriousGameEngime_XNA4.Screens;
using Microsoft.Xna.Framework;
using FuriousGameEngime_XNA4.ModelManager;
using Jitter.LinearMath;
using System.IO;

namespace FuriousGameEngime_XNA4.GameEntities
{
    class EnviornmentEntity : Entity
    {
        internal EnviornmentEntity(GameScreen gameScreen, ModelBase modelBase, string name, JVector position)
            :base (gameScreen, modelBase, name, position)
        {

        }

        internal EnviornmentEntity(GameScreen gameScreen, BinaryReader reader)
            :base(gameScreen, reader)
        {

        }

        internal override void Save(BinaryWriter writer)
        {
            writer.Write(name);

            writer.Write(Position.X);
            writer.Write(Position.Y);
            writer.Write(Position.Z);

            JQuaternion rotation = JQuaternion.CreateFromMatrix(Rotation);
            writer.Write(rotation.X);
            writer.Write(rotation.Y);
            writer.Write(rotation.Z);
            writer.Write(rotation.W);
        }
    }
}
