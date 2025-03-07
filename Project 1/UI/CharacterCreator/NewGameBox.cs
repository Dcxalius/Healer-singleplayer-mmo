using Microsoft.Xna.Framework;
using Project_1.Camera;
using Project_1.Managers;
using Project_1.Textures;
using Project_1.UI.UIElements.Boxes;
using Project_1.UI.UIElements.Buttons;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Project_1.UI.CharacterCreator
{
    internal class NewGameBox : MenuBox
    {
        string name;
        string className;

        public NewGameBox(RelativeScreenPosition aPos, RelativeScreenPosition aSize) : base(new UITexture("GrayBackground", Color.AliceBlue), aPos, aSize)
        {
            name = "";
            RelativeScreenPosition size = RelativeScreenPosition.GetSquareFromY(0.05f);
            AddChild(new LoadingMenu.ExitButton(aSize - size, size));
            AddChild(new InputBox("Name:", Color.Black, "Input Character Name", Color.White, Color.LightGray, Color.Black, new RelativeScreenPosition(0.05f), new RelativeScreenPosition(0.25f, 0.08f)));
            AddChild(new GFXButton(new List<Action>() { new Action(() => className = "Priest")}, new GfxPath(GfxType.UI, "Priest"), new RelativeScreenPosition(0.4f), new AbsoluteScreenPosition(64).ToRelativeScreenPosition(), Color.AliceBlue));
            AddChild(new GFXButton(new List<Action>() { new Action(() => className = "Druid")}, new GfxPath(GfxType.UI, "Druid"), new RelativeScreenPosition(0.5f, 0.4f), new AbsoluteScreenPosition(64).ToRelativeScreenPosition(), Color.AliceBlue));
        }

        public override void Update()
        {
            DebugManager.Print(GetType(), name);
            base.Update();
        }
    }
}
