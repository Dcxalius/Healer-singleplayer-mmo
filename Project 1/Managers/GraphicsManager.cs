using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Project_1.Camera;
using Project_1.GameObjects.Spawners;
using Project_1.Input;
using Project_1.Managers.States;
using Project_1.Particles;
using Project_1.Tiles;
using Project_1.UI;
using Project_1.UI.HUD;
using Project_1.UI.PauseMenu;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using static Project_1.GameObjects.Spells.AoE.AreaOfEffectData;

namespace Project_1.Managers
{

    internal static class GraphicsManager
    {
        public enum FullscreenMode
        {
            Windowed,
            Fullscreen,
            BorderlessFullscreen
        }

        static GraphicsDeviceManager graphicsDeviceManager;
        static GraphicsAdapter graphicsAdapter;
        public static GameWindow GameWindow => gameWindow;
        static GameWindow gameWindow;

        //--- Window stuff
        readonly static Point windowsTitleBarStuff = new Point(128, 32); //128 is a guesstimation of the minimum width, 32 is the required height based on https://learn.microsoft.com/en-us/windows/apps/design/basics/titlebar-design
        static Rectangle windowBounds;
        static readonly Point offset = new Point(8, 31); //I dont know why this is neccessary, well y is just the titlebar height but why does x have to 8????
        static readonly Point fullscreenOffset = new Point(0, 0);

        static bool fullsceen = false;
        static bool borderlessFullscreen = false;

        static Rectangle unCaptueredScissorRect;
        static object scissorRectCaptor = null;
        static List<(object, Rectangle)> scissors;
        static GraphicsManager()
        {
            scissors = new List<(object, Rectangle)>();
            //SetWindowSize(Camera.Camera.devScreenBorder, );
        }


        public static bool CaptureScissor(object aCaptor, Rectangle aRect)
        {

            Rectangle r = aRect;

            for (int i = 0; i < scissors.Count; i++)
            {
                r = Rectangle.Intersect(r, scissors[i].Item2);
            }
            scissors.Add((aCaptor, r));

            graphicsDeviceManager.GraphicsDevice.ScissorRectangle = r;



            return true;
        }

        public static bool ReleaseScissor(object aReleaser)
        {

            Debug.Assert(aReleaser == scissors[scissors.Count - 1].Item1);
            scissors.RemoveAt((scissors.Count - 1));


            if (scissors.Count == 0)
            {
                graphicsDeviceManager.GraphicsDevice.ScissorRectangle = unCaptueredScissorRect;
                return true;
            }

            graphicsDeviceManager.GraphicsDevice.ScissorRectangle = scissors[scissors.Count - 1].Item2;
            return true;
        }


        public static Texture2D CreateNewTexture(Point aSize) => new Texture2D(graphicsDeviceManager.GraphicsDevice, aSize.X, aSize.Y);
        public static SpriteBatch CreateSpriteBatch() => new SpriteBatch(graphicsDeviceManager.GraphicsDevice);
        public static RenderTarget2D CreateRenderTarget(Point aSize) => new RenderTarget2D(graphicsDeviceManager.GraphicsDevice, aSize.X, aSize.Y);
        public static Texture2D CreateTextureFromFile(string aPath) => Texture2D.FromFile(graphicsDeviceManager.GraphicsDevice, aPath);
        public static void SetRenderTarget(RenderTarget2D aRenderTarget) => graphicsDeviceManager.GraphicsDevice.SetRenderTarget(aRenderTarget);

        public static void SetManager(Microsoft.Xna.Framework.Game aGame)
        {
            graphicsDeviceManager = new GraphicsDeviceManager(aGame);

            graphicsDeviceManager.GraphicsProfile = GraphicsProfile.HiDef;
            graphicsDeviceManager.ApplyChanges();
            graphicsAdapter = GraphicsAdapter.DefaultAdapter;
            gameWindow = aGame.Window;
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
            if (ApplicationIsActivated()) //TODO: Add an option for this
            {
                ClipCursor(ref windowBounds);

            }
        }


        public static void ClearScreen(Color aColor)
        {
            graphicsDeviceManager.GraphicsDevice.Clear(aColor);
        }

        public static void SetWindowSize(Point aSize, CameraSettings.WindowType aFullscreen) //TODO: Figure out whats wrong with fullscreen.
        {
            if (!AllowedSize(aSize))
            {
                return;
            }

            if (aFullscreen <= CameraSettings.WindowType.Borderless)
            {
                graphicsDeviceManager.PreferredBackBufferWidth = GraphicsAdapter.DefaultAdapter.CurrentDisplayMode.Width;
                graphicsDeviceManager.PreferredBackBufferHeight = GraphicsAdapter.DefaultAdapter.CurrentDisplayMode.Height;
                StateManager.RenderTargetPosition = GetRenderTargetDestination(Camera.Camera.ScreenRectangle.Size, graphicsDeviceManager.PreferredBackBufferWidth, graphicsDeviceManager.PreferredBackBufferHeight);
            }
            else
            {
                graphicsDeviceManager.PreferredBackBufferWidth = aSize.X;
                graphicsDeviceManager.PreferredBackBufferHeight = aSize.Y;
                StateManager.RenderTargetPosition = new Rectangle(0, 0, aSize.X, aSize.Y);
            }

            switch (aFullscreen)
            {
                case CameraSettings.WindowType.Fullscreen:
                    graphicsDeviceManager.IsFullScreen = true;
                    graphicsDeviceManager.HardwareModeSwitch = false;
                    break;
                case CameraSettings.WindowType.Borderless:
                    graphicsDeviceManager.IsFullScreen = true;
                    graphicsDeviceManager.HardwareModeSwitch = true;
                    break;
                case CameraSettings.WindowType.Windowed:
                    graphicsDeviceManager.HardwareModeSwitch = false;
                    graphicsDeviceManager.IsFullScreen = false;
                    break;
                default:
                    break;
            }
            graphicsDeviceManager.ApplyChanges();

            //Add check here to see if display area is correct and if it isn't change aSize


            Camera.Camera.SetWindowSize(new Camera.AbsoluteScreenPosition(aSize));
            StateManager.Rescale();
            unCaptueredScissorRect = graphicsDeviceManager.GraphicsDevice.ScissorRectangle;

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
            IntPtr activatedHandle = GetForegroundWindow();
            if (activatedHandle == IntPtr.Zero)
            {
                return false;       // No window is currently activated
            }

            int procId = Process.GetCurrentProcess().Id;
            _ = GetWindowThreadProcessId(activatedHandle, out int activeProcId);

            return activeProcId == procId;
        }


        [DllImport("user32.dll", CharSet = CharSet.Auto, ExactSpelling = true)]
        private static extern IntPtr GetForegroundWindow();

        [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        private static extern int GetWindowThreadProcessId(IntPtr handle, out int processId);
    }
}
