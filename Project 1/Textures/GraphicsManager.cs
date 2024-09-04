using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Project_1.Managers;
using Project_1.UI;
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
        public enum FullscreenMode
        {
            Windowed,
            Fullscreen,
            BorderlessFullscreen
        }

        static Dictionary<string, Texture2D>[] texturesDict;
        static GraphicsDeviceManager gdm;
        static GraphicsAdapter graphicsAdapter;
        static ContentManager cm;
        static GameWindow gameWindow;
        public static SpriteFont buttonFont;
        readonly static Point windowsTitleBarStuff = new Point(128, 32); //128 is a guesstimation of the minimum width, 32 is the required height based on https://learn.microsoft.com/en-us/windows/apps/design/basics/titlebar-design

        public static SpriteBatch CreateSpriteBatch()
        {
            return new SpriteBatch(gdm.GraphicsDevice);
        }

        public static void SetManager(Game aGame)
        {
            gdm = new GraphicsDeviceManager(aGame);
            graphicsAdapter = GraphicsAdapter.DefaultAdapter;
            gameWindow = aGame.Window;
        }

        public static void Init(ContentManager aCm)
        {
            cm = aCm;

            InitArrays();
            SetWindowSize(Camera.devScreenBorder, false, false);
            buttonFont = cm.Load<SpriteFont>("Font/Gloryse"); //TODO: Font is not open source so need to be change at some point
        }

        [DllImport("user32.dll")]
        static extern void ClipCursor(ref Rectangle rect);

        public static void Update()
        {
            //Rectangle rect = Camera.ScreenRectangle;
            Rectangle rect = gameWindow.ClientBounds;
            rect.Location = gameWindow.Position;
            //rect.Location += 

            ClipCursor(ref rect);
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

        public static void SetWindowSize(Point aSize, bool aFullscreen, bool aBorderless)
        {
            if (!AllowedSize(aSize))
            {
                return;
            }

            gdm.PreferredBackBufferWidth = aSize.X;
            gdm.PreferredBackBufferHeight = aSize.Y;
            gdm.IsFullScreen = aFullscreen;
            gdm.HardwareModeSwitch = (aBorderless || aFullscreen);
            gdm.ApplyChanges();

            //Add check here to see if display area is correct and if it isn't change aSize
            Camera.SetWindowSize(aSize);

            UIManager.Rescale();


        }

        static bool AllowedSize(Point aSize)
        {
            //https://stackoverflow.com/questions/1264406/how-do-i-get-the-taskbars-position-and-size   
            if (aSize.Y > graphicsAdapter.CurrentDisplayMode.Height - windowsTitleBarStuff.Y)
            {
                DebugManager.Print(typeof(GraphicsManager), "My Y is too big");
                return false;
            }
            if (aSize.X > graphicsAdapter.CurrentDisplayMode.Width)
            {
                DebugManager.Print(typeof(GraphicsManager), "My X is too big");
                return false;

            }

            if (aSize.X < windowsTitleBarStuff.X)
            {
                DebugManager.Print(typeof(GraphicsManager), "Tried to set it smaller than the required lenght");
                return false;
            }

            return true;
        }
    }
}
