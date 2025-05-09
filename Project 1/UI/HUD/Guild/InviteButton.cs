using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Project_1.Camera;
using Project_1.GameObjects;
using Project_1.GameObjects.Entities;
using Project_1.Managers;
using Project_1.Textures;
using Project_1.Tiles;
using Project_1.UI.UIElements.Buttons;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Project_1.UI.HUD.Guild
{
    internal class InviteButton : TwoStateGFXButton
    {
        GuildMember guildMember;
        public InviteButton(Friendly aFriendly, RelativeScreenPosition aPos, RelativeScreenPosition aSize) : base(new List<Action>() { }, new GfxPath(GfxType.UI, "Invite"), new List<Action>() { }, new GfxPath(GfxType.UI, "Uninvite"), aPos, aSize, Color.White)
        {
            if (aFriendly.GetType() != typeof(GuildMember)) return;

            guildMember = aFriendly as GuildMember;
            AddAction(new Action(() => Invite()), State.First);
            AddAction(new Action(() => Kick()), State.Second);
        }

        void Invite()
        {
            Debug.Assert(!ObjectManager.Player.Party.IsInParty(guildMember));
            ObjectManager.SpawnGuildMemberToParty(guildMember, null);

        }

        void Kick()
        {
            Debug.Assert(ObjectManager.Player.Party.IsInParty(guildMember));
            ObjectManager.RemoveGuildMemberFromParty(guildMember);
        }

    }
}
