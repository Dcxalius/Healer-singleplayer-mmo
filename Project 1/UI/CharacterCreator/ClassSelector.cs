using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Project_1.Camera;
using Project_1.Managers;
using Project_1.Textures;
using Project_1.UI.UIElements.Boxes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Project_1.GameObjects.Entities.Friendlies.Players;

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

        public ClassSelector(RelativeScreenPosition aPos, RelativeScreenPosition aSize) : base(new UITexture("WhiteBackground", Color.DarkKhaki), aPos, aSize)
        {
            Init();
            RelativeScreenPosition size = new RelativeScreenPosition(1 / lines, 1 / rows);
            for (int i = 0; i < classNames.Length; i++)
            {
                DebugManager.Print("i == " + i + "  formula == " + i % (classNames.Length / lines));
                AddChild(new ClassSelectButton(classNames[i], new RelativeScreenPosition(size.X * (i % (classNames.Length)), size.Y * MathF.Floor(i / (classNames.Length))), size));
            }
        }
    }
}
