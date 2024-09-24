using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Project_1.Managers;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace Project_1.Textures
{
    internal static class TextureManager
    {

        static Dictionary<string, Texture2D>[] texturesDict;
        static Dictionary<string, SpriteFont> fontDict;//TODO: Font is not open source so need to be change at some point

        static ContentManager contentManager;


        public static void Init(ContentManager aContentManager)
        {
            contentManager = aContentManager;
            InitArrays();
            InitFonts();
        }

        static void InitFonts()
        {
            fontDict = new Dictionary<string, SpriteFont>();
            string debug = "Fonts loaded: ";

            string[] dir = Directory.GetFiles(contentManager.RootDirectory + "\\Font");


            for (int i = 0; i < dir.Length; i++)
            {
                string filePath = TrimFilePath(dir[i]);
                string fontName = filePath.Split('\\')[1];

                fontDict.Add(fontName, contentManager.Load<SpriteFont>(filePath));
                debug += fontName + ", ";

            }

        }

        static void InitArrays()
        {
            texturesDict = new Dictionary<string, Texture2D>[(int)GfxType.Count];


            string debug = "Textures loaded: ";

            for (int i = 0; i < texturesDict.Length; i++)
            {
                string[] dir = Directory.GetFiles(contentManager.RootDirectory + "\\" + (GfxType)i);

                texturesDict[i] = new Dictionary<string, Texture2D>();

                for (int j = 0; j < dir.Length; j++)
                {
                    string filePath = TrimFilePath(dir[j]);
                    string textureName = filePath.Split('\\')[1];

                    texturesDict[i].Add(textureName, contentManager.Load<Texture2D>(filePath));
                    debug += textureName + ", ";

                }
            }

            DebugManager.Print(typeof(GraphicsManager), debug);
        }

        static string TrimFilePath(string aPath)
        {
            string filePath = aPath.Substring(contentManager.RootDirectory.Length + 1);
            return filePath.Substring(0, filePath.Length - 4);
        }


        public static ref SpriteFont GetFont(string fontName)
        {
            return ref CollectionsMarshal.GetValueRefOrNullRef(fontDict, fontName);
        }

        public static ref Texture2D GetTexture(GfxPath aGfxPath)
        {
            return ref CollectionsMarshal.GetValueRefOrNullRef(texturesDict[(int)aGfxPath.Type], aGfxPath.Name);
        }

    }
}
