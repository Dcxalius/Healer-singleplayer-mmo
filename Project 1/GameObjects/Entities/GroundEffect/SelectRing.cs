using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Project_1.Camera;
using Project_1.GameObjects.Entities.Temp;
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

        public void Draw(SpriteBatch aBatch, Entity aOwner)
        {
            if (aOwner.Selected == true)
            {
                Draw(aBatch, aOwner, aOwner.RelationColor, 1);
            }
        }
    }
}
