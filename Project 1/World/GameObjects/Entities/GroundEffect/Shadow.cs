using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Project_1.Camera;
using Project_1.Textures;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Project_1.GameObjects.Entities.Friendlies.GuildMembers;
using Project_1.GameObjects.Entities.Friendlies.Players;

namespace Project_1.GameObjects.Entities.GroundEffect
{
    internal class Shadow : GroundEffect
    {
        public Shadow() : base(new GfxPath(GfxType.Object, "Shadow"))
        {
        }

        public override void Draw(SpriteBatch aBatch, WorldObject aOwner)
        {
            Project_1.Managers.ThreadAffinity.AssertMainThread();
            Color shadowColor = Color.Black;
            if (aOwner.GetType() == typeof(GuildMember))
            {
                if (ObjectManager.Player.Party.IsInCommand(aOwner as GuildMember))
                {
                    shadowColor = Color.DarkGreen;
                }
            }

            Draw(aBatch, aOwner.ScreenRect, shadowColor, aOwner.FeetPosition.Y - 2);

        }
    }
}
