using Project_1.Textures;
using Project_1.UI.UIElements;
using System;
using Microsoft.Xna.Framework;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Project_1.Camera;
using Project_1.GameObjects.Entities;
using Project_1.UI.UIElements.Buttons;
using Project_1.UI.HUD.Guild;

namespace Project_1.UI.HUD.Windows
{
    internal class GuildWindow : Window
    {
        GuildRoster roster;
        public override bool Visible
        {
            get => base.Visible; 
            set
            {
                base.Visible = value;

                if (!value)
                {
                    HUDManager.CloseGuildWindow();
                }
            }
        }
        public GuildWindow() : base(new UITexture("WhiteBackground", Color.SaddleBrown))
        {
            RelativeScreenPosition spacing = RelativeScreenPosition.GetSquareFromX(0.03f, Size);
            roster = new GuildRoster(spacing, new RelativeScreenPosition(1, 0.8f) - spacing * 2);
            visibleKey = Input.KeyBindManager.KeyListner.GuildRoster;

            AddChild(roster);
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
