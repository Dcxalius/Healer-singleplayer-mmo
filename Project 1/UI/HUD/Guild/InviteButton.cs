using Microsoft.Xna.Framework;
using Project_1.Messaging;
using Project_1.Messaging.Events;
using Project_1.Textures;
using Project_1.Camera;
using Project_1.UI.UIElements.Buttons;
using System;
using System.Collections.Generic;

namespace Project_1.UI.HUD.Guild
{
    internal class InviteButton : TwoStateGFXButton
    {
        int memberRenderId;
        public InviteButton(UI.UIElements.UIElement aParent, int aMemberRenderId, RelativeScreenPosition aPos, RelativeScreenPosition aSize) : base(aParent, new List<Action>() { }, new GfxPath(GfxType.UI, "Invite"), new List<Action>() { }, new GfxPath(GfxType.UI, "Uninvite"), aPos, aSize, Color.White)
        {
            memberRenderId = aMemberRenderId;
            AddAction(new Action(() => Invite()), State.First);
            AddAction(new Action(() => Kick()), State.Second);
        }

        void Invite()
        {
            MailboxManager.PublishSimCommand(new PartyMemberInviteRequested(memberRenderId));
        }

        void Kick()
        {
            MailboxManager.PublishSimCommand(new PartyMemberKickRequested(memberRenderId));
        }

    }
}
