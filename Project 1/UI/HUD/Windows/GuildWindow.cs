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

namespace Project_1.UI.HUD.Windows
{
    internal class GuildWindow : Window
    {
        GuildRoster roster;
        public GuildWindow() : base(new UITexture("WhiteBackground", Color.SaddleBrown))
        {
            roster = new GuildRoster(RelativeScreenPosition.Zero, new (WindowSize.X, 0.17f));
            visibleKey = Input.KeyBindManager.KeyListner.GuildRoster;

            children.Add(roster);
            ToggleVisibilty();
        }

        public void SetRoster(Friendly[] aData)
        {
            roster.SetData(aData);
        }
    }
}
