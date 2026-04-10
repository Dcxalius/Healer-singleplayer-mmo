using Microsoft.Xna.Framework;
using Project_1.Camera;
using Project_1.Managers;
using Project_1.Textures;
using Project_1.UI.UIElements.Boxes;
using System;

namespace Project_1.UI.CharacterCreator
{
    internal class ClassSelector : Box
    {

        static string[] classNames;
        static bool initialized;

        static void Init()
        {
            if (initialized) return;
            initialized = true;
            string root = Game1.ContentManager.RootDirectory;
            classNames = System.IO.Directory.GetFiles(root + "\\Data\\Class\\Player");
            for (int i = 0; i < classNames.Length; i++)
            {
                classNames[i] = SaveManager.TrimToNameOnly(classNames[i]);
            }

        }

        const float lines = 2;
        const float rows = 1;

        public ClassSelector(UI.UIElements.UIElement aParent, RelativeScreenPosition aPos, RelativeScreenPosition aSize) : base(aParent, new UITexture("WhiteBackground", Color.DarkKhaki), aPos, aSize)
        {
            Init();
            RelativeScreenPosition size = new RelativeScreenPosition(1 / lines, 1 / rows);
            for (int i = 0; i < classNames.Length; i++)
            {
                DebugManager.Print("i == " + i + "  formula == " + i % (classNames.Length / lines));
                new ClassSelectButton(this, classNames[i], new RelativeScreenPosition(size.X * (i % (classNames.Length)), size.Y * MathF.Floor(i / (classNames.Length))), size);
            }
        }
    }
}
