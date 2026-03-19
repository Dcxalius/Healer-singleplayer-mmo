using Microsoft.Xna.Framework;
using Project_1.Camera;
using Project_1.Textures;
using Project_1.Messaging;
using Project_1.Messaging.Events;
using Project_1.UI.UIElements.Buttons;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
namespace Project_1.UI.HUD.Guild
{
    internal class OpenInspectWindow : GFXButton
    {
        static void OpenWindow(EntityUiSnapshot member)
        {
            if (member.RelationToPlayer == RelationToPlayerKind.Self)
            {
                MailboxManager.PublishUiEvent(new CharacterWindowToggled());
                return;
            }
            MailboxManager.PublishUiEvent(new InspectWindowToggled(member));
        }

        public OpenInspectWindow(EntityUiSnapshot aSnapshot, RelativeScreenPosition aPos, RelativeScreenPosition aSize) : base(new List<Action>() { new Action(() => OpenWindow(aSnapshot)) }, new GfxPath(GfxType.Item, "TestDagger"), aPos, aSize, Color.White)
        {

        }


    }
}
