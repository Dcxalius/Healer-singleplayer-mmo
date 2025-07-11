using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Project_1.GameObjects.Spells;
using Project_1.Managers;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Serialization;

namespace Project_1.Textures
{
    internal static class TextureManager
    {

        static Dictionary<string, Texture2D>[] texturesDict;
        static Dictionary<string, SpriteFont> fontDict;//TODO: Font is not open source so need to be change at some point

        static ContentManager contentManager;

        public static Effect textOutline;

        static TextureManager()
        {
            contentManager = Game1.ContentManager;
            InitArrays();
            InitFonts();
            textOutline = contentManager.Load<Effect>("TextOutline");
            //textOutline.Parameters["texelSize"].SetValue()
        }

        static void InitFonts()
        {
            fontDict = new Dictionary<string, SpriteFont>();
            string debug = "Fonts loaded: ";

            string[] dir = Directory.GetFiles(contentManager.RootDirectory + "\\Font");


            for (int i = 0; i < dir.Length; i++)
            {
                string filePath = TrimContentFolderAndImageFileExtention(dir[i]);
                string fontName = filePath.Split('\\')[1];

                fontDict.Add(fontName, contentManager.Load<SpriteFont>(filePath));
                debug += fontName + ", ";

            }

        }

        static void InitArrays()
        {
            texturesDict = new Dictionary<string, Texture2D>[(int)GfxType.Count];

            string root = contentManager.RootDirectory + "\\Graphics\\";
            string debug = "Textures loaded: ";

            for (int i = 0; i < texturesDict.Length; i++)
            {
                string path =  root + (GfxType)i;
                string[] dir = Directory.GetFiles(path);

                texturesDict[i] = new Dictionary<string, Texture2D>();


                for (int j = 0; j < dir.Length; j++)
                {
                    string filePath = TrimContentFolderAndImageFileExtention(dir[j]); 
                    string textureName = filePath.Split('\\').Last();

                    texturesDict[i].Add(textureName, contentManager.Load<Texture2D>(filePath));
                    debug += textureName + ", ";

                }

                string[] dirsInDir = Directory.GetDirectories(path);
                
                for (int j = 0; j < dirsInDir.Length; j++)
                {
                    string[] filesInFolders = Directory.GetFiles(dirsInDir[j]);
                    for (int k = 0; k < filesInFolders.Length; k++)
                    {
                        string filePath = TrimContentFolderAndImageFileExtention(filesInFolders[k]);
                        string textureName = filePath.Split('\\').Last();

                        texturesDict[i].Add(textureName, contentManager.Load<Texture2D>(filePath));
                        debug += textureName + ", ";
                    }
                    

                }
            }

            DebugManager.Print(typeof(GraphicsManager), debug);
        }


        static string TrimContentFolderAndImageFileExtention(string aPath)
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
            ref Texture2D? a = ref CollectionsMarshal.GetValueRefOrNullRef(texturesDict[(int)aGfxPath.Type], aGfxPath.Name);

            if (System.Runtime.CompilerServices.Unsafe.IsNullRef(ref a)) 
            {
                DebugManager.Print(typeof(TextureManager), "Texture " + aGfxPath.Name + " from type " + aGfxPath.Type + " was not found.");
                a = ref CollectionsMarshal.GetValueRefOrNullRef(texturesDict[(int)GfxType.Debug], "MissingTexture");
            }

            return ref a ;
        }

    }
}
