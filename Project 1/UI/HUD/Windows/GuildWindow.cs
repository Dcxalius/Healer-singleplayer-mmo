using Project_1.Textures;
using Project_1.UI.UIElements;
using System;
using Microsoft.Xna.Framework;  
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Project_1.UI.UIElements.Guild;
using Project_1.Camera;
using Project_1.GameObjects.Entities;
using Project_1.UI.UIElements.Buttons;

namespace Project_1.UI.HUD.Windows
{
    internal class GuildWindow : Window
    {
        GuildRoster roster;
        public GuildWindow() : base(new UITexture("WhiteBackground", Color.SaddleBrown))
        {
            roster = new GuildRoster(RelativeScreenPosition.Zero, new (WindowSize.X, 0.17f));
            visibleKey = Input.KeyBindManager.KeyListner.GuildRoster;

            AddChild(roster);
            //ToggleVisibilty();
        }

        public void SetGuildMemberInviteStatus(List<string> aName, List<TwoStateGFXButton.State> aState)
        {
            roster.SetGuildMemberInviteStatus(aName, aState);
        }

        public void SetRoster(Friendly[] aData)
        {
            roster.SetData(aData);
        }
    }
}
