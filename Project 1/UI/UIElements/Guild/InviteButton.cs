using Microsoft.Xna.Framework;
using Project_1.Camera;
using Project_1.GameObjects;
using Project_1.GameObjects.Entities;
using Project_1.Textures;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Project_1.UI.UIElements.Guild
{
    internal class InviteButton : GFXButton
    {
        GuildMember friendly;
        public InviteButton(Friendly aFriendly, RelativeScreenPosition aPos, RelativeScreenPosition aSize) : base(new GfxPath(GfxType.Object, "Walker"), aPos, aSize, Color.White)
        {
            if (aFriendly.GetType() != typeof(GuildMember)) return;
            
            friendly = aFriendly as GuildMember;
        }

        public override void HoldReleaseOnMe()
        {
            base.HoldReleaseOnMe();
            Debug.Assert(!ObjectManager.Player.Party.IsInParty(friendly));

            ObjectManager.SpawnGuildMemberToParty(friendly);
        }
    }
}
