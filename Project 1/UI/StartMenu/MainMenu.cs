using Microsoft.Xna.Framework;
using Project_1.Camera;
using Project_1.Textures;
using Project_1.UI.PauseMenu;
using Project_1.UI.UIElements.Boxes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Project_1.UI.StartMenu
{
    internal class MainMenu : MenuBox
    {

        static RelativeScreenPosition mainSize = new RelativeScreenPosition(0.2f, 0.5f);
        //static UITexture staticGfx = new UITexture("WhiteBackground", Color.DarkGray);

        static RelativeScreenPosition mainPos = new RelativeScreenPosition(0.5f - (mainSize.X / 2), 0.5f - mainSize.Y / 2);

        public MainMenu() : base(new UITexture("GrayBackground", Color.Red), new RelativeScreenPosition(0.6f, 0.4f), new RelativeScreenPosition(0.3f, 0.5f))
        {
            AddChild(new ContinueLastSaveButton(GetStartPositionFromTop, ButtonSize));
            AddChild(new NewGameButton(GetStartPositionFromTop, ButtonSize));
            AddChild(new LoadGameButton(GetStartPositionFromTop, ButtonSize));
            AddChild(new OptionMenuButton(GetStartPositionFromTop, ButtonSize));

            AddChild(new ExitGameButton(GetStartPositionFromBottom, ButtonSize));
        }
    }
}
