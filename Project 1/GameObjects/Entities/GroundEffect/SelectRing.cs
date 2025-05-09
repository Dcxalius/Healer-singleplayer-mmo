using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Project_1.Camera;
using Project_1.Textures;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Project_1.GameObjects.Entities.GroundEffect
{
    internal class SelectRing : GroundEffect
    {

        public SelectRing() : base(new GfxPath(GfxType.Object, "SelectRing"))
        {
        }

        public override void Draw(SpriteBatch aBatch, WorldObject aOwner)
        {
            if (aOwner is not Entity) return;

            Entity entityOwner = aOwner as Entity;
            if (entityOwner.Selected == true)
            {
                Draw(aBatch, entityOwner.ScreenRect, entityOwner.RelationColor, entityOwner.FeetPosition.Y - 1);
            }
        }
    }
}
