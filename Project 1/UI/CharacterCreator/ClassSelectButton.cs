using Microsoft.Xna.Framework;
using Project_1.Camera;
using Project_1.Textures;
using Project_1.UI.UIElements.Buttons;
using System.Collections.Generic;

namespace Project_1.UI.CharacterCreator
{
    internal class ClassSelectButton : GFXButton
    {
        string className;

        public static string ClassName => classSelected;
        static List<ClassSelectButton> classSelectButtons = new List<ClassSelectButton>();
        static string classSelected = null; 
        static void DeselectAll()
        {
            for (int i = 0; i < classSelectButtons.Count; i++)
            {
                classSelectButtons[i].Color = Color.White;
            }
            classSelected = null;
        }

        public ClassSelectButton(UI.UIElements.UIElement aParent, string aClassName, RelativeScreenPosition aPos, RelativeScreenPosition aSize) : base(aParent, new GfxPath(GfxType.UI, aClassName), aPos, aSize, Color.White)
        {
            className = aClassName;
            classSelectButtons.Add(this);
        }

        public override void ClickedOnAndReleasedOnMe()
        {
            base.ClickedOnAndReleasedOnMe();

            DeselectAll();
            classSelected = className;
            Color = Color.DarkBlue;
        }


    }
}
