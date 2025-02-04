using Microsoft.Xna.Framework;
using Project_1.Camera;
using Project_1.GameObjects.Entities;
using Project_1.GameObjects.Entities.Players;
using Project_1.Textures;
using Project_1.UI.HUD;
using Project_1.UI.UIElements.Buttons;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Project_1.UI.UIElements.Guild
{
    internal class OpenInspectWindow : GFXButton
    {
        static void OpenWindow(Friendly aFriendly)
        {
            if (aFriendly.RelationToPlayer == GameObjects.Unit.Relation.RelationToPlayer.Self)
            {
                HUDManager.ToggleCharacterWindow();
                return;
            }
            HUDManager.ToggleInspectWindow(aFriendly as GuildMember);
        }

        public OpenInspectWindow(Friendly aFriendly, RelativeScreenPosition aPos, RelativeScreenPosition aSize) : base(new List<Action>() { new Action(() => OpenWindow(aFriendly)) }, new GfxPath(GfxType.Item, "TestDagger"), aPos, aSize, Color.White)
        {

        }

        
    }
}
