using Microsoft.Xna.Framework;
using Project_1.Camera;
using Project_1.Textures;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Project_1.UI.UIElements.Guild
{
    internal class GuildMember : Box
    {
        Label name;
        GFXButton openInventory;
        GFXButton openCharacterWindow;
        public GuildMember(string aName, RelativeScreenPosition aPos, RelativeScreenPosition aSize) : base(new UITexture("WhiteBackground", Color.Aquamarine), aPos, aSize)
        {

            name = new Label(aName, RelativeScreenPosition.Zero, new RelativeScreenPosition(300, aSize.Y), Label.TextAllignment.CentreLeft);
            RelativeScreenPosition buttonSize = RelativeScreenPosition.GetSquareFromY(aSize.Y);
            RelativeScreenPosition spacing = RelativeScreenPosition.GetSquareFromX(0.05f);

            openCharacterWindow = new OpenCharacterWindow(new RelativeScreenPosition(aSize.X - buttonSize.X - spacing.X, 0), buttonSize);
            openInventory = new OpenInventory(new RelativeScreenPosition( aSize.X - (buttonSize.X - spacing.X) * 2, 0), buttonSize);
            
            children.Add(name);
            children.Add(openCharacterWindow);
            children.Add(openInventory);
        }
    }
}
