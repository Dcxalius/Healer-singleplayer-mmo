using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Project_1.Input;
using Project_1.UI;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace Project_1.Managers
{
    //TODO: Split this into GraphicsMang and TextureMang

    internal static class GraphicsManager //TODO: Split this into TextureManager and GraphicsManager
    {
        public enum FullscreenMode
        {
            Windowed,
            Fullscreen,
            BorderlessFullscreen
        }

        static GraphicsDeviceManager graphicsDeviceManager;
        static GraphicsAdapter graphicsAdapter;
        static GameWindow gameWindow;

        //--- Window stuff
        readonly static Point windowsTitleBarStuff = new Point(128, 32); //128 is a guesstimation of the minimum width, 32 is the required height based on https://learn.microsoft.com/en-us/windows/apps/design/basics/titlebar-design
        static Rectangle windowBounds;
        static readonly Point offset = new Point(8, 31); //I dont know why this is neccessary, well y is just the titlebar height but why does x have to 8????
        static readonly Point fullscreenOffset = new Point(8, 0); //I dont know why this is neccessary, well y is just the titlebar height but why does x have to 8????

        static bool fullsceen = false;
        static bool borderlessFullscreen = false;



        public static SpriteBatch CreateSpriteBatch()
        {
            return new SpriteBatch(graphicsDeviceManager.GraphicsDevice);
        }

        public static RenderTarget2D CreateRenderTarget(Point aSize)
        {
            return new RenderTarget2D(graphicsDeviceManager.GraphicsDevice, aSize.X, aSize.Y);
        }

        public static void SetRenderTarget(RenderTarget2D aRenderTarget)
        {
            graphicsDeviceManager.GraphicsDevice.SetRenderTarget(aRenderTarget);
        }

        public static void SetManager(Game aGame)
        {
            graphicsDeviceManager = new GraphicsDeviceManager(aGame);
            graphicsAdapter = GraphicsAdapter.DefaultAdapter;
            gameWindow = aGame.Window;
        }

        public static void Init()
        {


            SetWindowSize(Camera.devScreenBorder, fullsceen, borderlessFullscreen);

        }

        [DllImport("user32.dll")]
        static extern void ClipCursor(ref Rectangle rect);

        public static void Update()
        {
            //Rectangle rect = Camera.ScreenRectangle;

            if (fullsceen)
            {
                windowBounds.Location = gameWindow.Position + fullscreenOffset;

            }
            else
            {
                windowBounds.Location = gameWindow.Position + offset;

            }

            windowBounds.Size = gameWindow.ClientBounds.Size + windowBounds.Location;
            //DebugManager.Print(typeof(GraphicsManager), InputManager.GetMousePosAbsolute().ToString());
            //rect.Location += 
            if (ApplicationIsActivated())
            {
                ClipCursor(ref windowBounds);

            }
        }


        public static void ClearScreen(Color aColor)
        {
            graphicsDeviceManager.GraphicsDevice.Clear(aColor);
        }

        public static void SetWindowSize(Point aSize, bool aFullscreen, bool aBorderless)
        {
            if (!AllowedSize(aSize))
            {
                return;
            }

            if (aFullscreen)
            {
                graphicsDeviceManager.PreferredBackBufferWidth = GraphicsAdapter.DefaultAdapter.CurrentDisplayMode.Width;
                graphicsDeviceManager.PreferredBackBufferHeight = GraphicsAdapter.DefaultAdapter.CurrentDisplayMode.Height;
                Camera.RenderTargetPosition = GetRenderTargetDestination(Camera.devScreenBorder, graphicsDeviceManager.PreferredBackBufferWidth, graphicsDeviceManager.PreferredBackBufferHeight);
            }
            else
            {
                graphicsDeviceManager.PreferredBackBufferWidth = aSize.X;
                graphicsDeviceManager.PreferredBackBufferHeight = aSize.Y;
                Camera.RenderTargetPosition = new Rectangle(0, 0, aSize.X, aSize.Y);
            }

            graphicsDeviceManager.IsFullScreen = aFullscreen;
            graphicsDeviceManager.HardwareModeSwitch = aBorderless && aFullscreen;
            graphicsDeviceManager.ApplyChanges();

            //Add check here to see if display area is correct and if it isn't change aSize


            Camera.SetWindowSize(aSize);

            UIManager.Rescale();


        }

        //stolen from https://community.monogame.net/t/how-do-i-make-full-screen-stretch-to-the-entire-screen-and-have-black-bars-on-the-sides-if-the-screen-aspect-ratio-isnt-16-9/17364
        static Rectangle GetRenderTargetDestination(Point resolution, int preferredBackBufferWidth, int preferredBackBufferHeight)
        {
            float resolutionRatio = (float)resolution.X / resolution.Y;
            float screenRatio;
            Point bounds = new Point(preferredBackBufferWidth, preferredBackBufferHeight);
            screenRatio = (float)bounds.X / bounds.Y;
            float scale;
            Rectangle rectangle = new Rectangle();

            if (resolutionRatio < screenRatio)
                scale = (float)bounds.Y / resolution.Y;
            else if (resolutionRatio > screenRatio)
                scale = (float)bounds.X / resolution.X;
            else
            {
                // Resolution and window/screen share aspect ratio
                rectangle.Size = bounds;
                return rectangle;
            }
            rectangle.Width = (int)(resolution.X * scale);
            rectangle.Height = (int)(resolution.Y * scale);
            return CenterRectangle(new Rectangle(Point.Zero, bounds), rectangle);
        }

        static Rectangle CenterRectangle(Rectangle outerRectangle, Rectangle innerRectangle)
        {
            Point delta = outerRectangle.Center - innerRectangle.Center;
            innerRectangle.Offset(delta);
            return innerRectangle;
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

        /// <summary>Returns true if the current application has focus, false otherwise</summary> 
        /// from https://stackoverflow.com/questions/7162834/determine-if-current-application-is-activated-has-focus
        public static bool ApplicationIsActivated()
        {
            var activatedHandle = GetForegroundWindow();
            if (activatedHandle == IntPtr.Zero)
            {
                return false;       // No window is currently activated
            }

            var procId = Process.GetCurrentProcess().Id;
            int activeProcId;
            GetWindowThreadProcessId(activatedHandle, out activeProcId);

            return activeProcId == procId;
        }


        [DllImport("user32.dll", CharSet = CharSet.Auto, ExactSpelling = true)]
        private static extern IntPtr GetForegroundWindow();

        [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        private static extern int GetWindowThreadProcessId(IntPtr handle, out int processId);


        static void ToggleFullScreen()
        {

            fullsceen = !fullsceen;
        }
    }
}
