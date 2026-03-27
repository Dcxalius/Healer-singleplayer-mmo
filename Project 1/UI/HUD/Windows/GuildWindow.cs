using Project_1.Textures;
using System;
using Microsoft.Xna.Framework;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Project_1.Camera;
using Project_1.UI.UIElements.Buttons;
using Project_1.UI.HUD.Guild;
using Project_1.Messaging.Events;

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
                    if (InspectWindow.Current?.Visible == true)
                    {
                        InspectWindow.Current.ToggleVisibilty();
                    }
                }
            }
        }
        public GuildWindow() : base(new UITexture("WhiteBackground", Color.SaddleBrown))
        {
            RelativeScreenPosition spacing = RelativeScreenPosition.GetSquareFromX(0.03f, Size);
            roster = new GuildRoster(spacing, new RelativeScreenPosition(1, 0.8f) - spacing * 2, this);
            visibleKey = Input.KeyBindManager.KeyListner.GuildRoster;

            AddChild(roster);
        }

        public void SetGuildMemberInviteStatus(List<string> aName, List<TwoStateGFXButton.State> aState)
        {
            roster.SetGuildMemberInviteStatus(aName, aState);
        }

        public void SetRoster(EntityUiSnapshot[] aData)
        {
            roster.SetData(aData);
        }

        public void AddMember(EntityUiSnapshot aData)
        {
            roster.AddMember(aData);
        }
    }
}
