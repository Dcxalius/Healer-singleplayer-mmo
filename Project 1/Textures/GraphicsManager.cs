using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Project_1.Managers;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace Project_1.Textures
{
    //TODO: Slit this into GraphicsMang and TextureMang

    internal static class GraphicsManager
    {
        static Dictionary<string, Texture2D>[] texturesDict;
        static GraphicsDeviceManager gdm;
        static ContentManager cm;
        public static SpriteFont buttonFont;

        public static SpriteBatch CreateSpriteBatch()
        {
            return new SpriteBatch(gdm.GraphicsDevice);
        }

        public static void SetManager(Game aGame)
        {
            gdm = new GraphicsDeviceManager(aGame);
        }

        public static void Init(ContentManager aCm)
        {
            cm = aCm;

            InitArrays();
            SetWindowSize(Camera.devScreenBorder);
            buttonFont = cm.Load<SpriteFont>("Font/Gloryse"); //TODO: Font is not open source so need to be change at some point
        }

        static void InitArrays()
        {
            //textures = new Texture2D[(int)GfxType.Count][];
            texturesDict = new Dictionary<string, Texture2D>[(int)GfxType.Count];



            //for (int i = 0; i < textures.Length; i++)
            //{
            //    string[] dir = Directory.GetFiles(cm.RootDirectory + "\\" + (GfxType)i);
            //    textures[i] = new Texture2D[dir.Length];
            //    for (int j = 0; j < dir.Length; j++)
            //    {
            //        string trimmedName = dir[j].Substring(cm.RootDirectory.Length + 1);
            //        trimmedName = trimmedName.Substring(0, trimmedName.Length - 4);

            //        textures[i][j] = cm.Load<Texture2D>(trimmedName);
            //        //Console.WriteLine(trimmedName);

            //    }
            //}

            string debug = "Textures loaded: ";

            for (int i = 0; i < texturesDict.Length; i++)
            {
                string[] dir = Directory.GetFiles(cm.RootDirectory + "\\" + (GfxType)i);

                texturesDict[i] = new Dictionary<string, Texture2D>();

                for (int j = 0; j < dir.Length; j++)
                {
                    string filePath = dir[j].Substring(cm.RootDirectory.Length + 1);
                    filePath = filePath.Substring(0, filePath.Length - 4);
                    string textureName = filePath.Split('\\')[1];

                    texturesDict[i].Add(textureName, cm.Load<Texture2D>(filePath));
                    debug += textureName + ", ";

                }
            }

            DebugManager.Print(typeof(GraphicsManager), debug);
        }

        public static void ClearScreen(Color aColor)
        {
            gdm.GraphicsDevice.Clear(aColor);
        }

        public static ref Texture2D GetTexture(GfxPath aGfxPath)
        {
            return ref CollectionsMarshal.GetValueRefOrNullRef(texturesDict[(int)aGfxPath.Type], aGfxPath.Name);
        }

        public static void SetWindowSize(Point aSize)
        {

            gdm.PreferredBackBufferWidth = aSize.X;
            gdm.PreferredBackBufferHeight = aSize.Y;
            gdm.ApplyChanges();


            Camera.SetWindowSize(aSize);

        }
    }
}
