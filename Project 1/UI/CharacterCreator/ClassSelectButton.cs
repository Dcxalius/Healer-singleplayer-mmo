using Microsoft.Xna.Framework;
using Project_1.Camera;
using Project_1.Textures;
using Project_1.UI.UIElements.Buttons;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Project_1.UI.CharacterCreator
{
    internal class ClassSelectButton : GFXButton
    {
        bool selected;
        string className;

        public static string ClassName => classSelected;
        static List<ClassSelectButton> classSelectButtons = new List<ClassSelectButton>();
        static string classSelected = null; 
        static void DeselectAll()
        {
            for (int i = 0; i < classSelectButtons.Count; i++)
            {
                classSelectButtons[i].Color = Color.White;
                classSelectButtons[i].selected = false;
            }
            classSelected = null;
        }

        public ClassSelectButton(string aClassName, RelativeScreenPosition aPos, RelativeScreenPosition aSize) : base(new GfxPath(GfxType.UI, aClassName), aPos, aSize, Color.White)
        {
            selected = false;
            className = aClassName;
            classSelectButtons.Add(this);
        }

        public override void ClickedOnAndReleasedOnMe()
        {
            base.ClickedOnAndReleasedOnMe();

            DeselectAll();
            classSelected = className;
            Color = Color.DarkBlue;
            selected = true;
        }


    }
}
