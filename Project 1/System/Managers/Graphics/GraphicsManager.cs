using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System;
using System.Reflection.Metadata;

namespace Project_1.Managers
{
    internal static partial class GraphicsManager
    {
        static GraphicsDeviceManager graphicsDeviceManager;
        static GraphicsAdapter graphicsAdapter;
        public static GameWindow GameWindow => gameWindow;
        public static bool HasWindowLayout => hasWindowLayout;
        public static Point CurrentWindowSize => currentWindowSize;
        public static Rectangle CurrentRenderTargetDestination => currentRenderTargetDestination;
        public static event Action<Point, Rectangle> WindowLayoutChanged;
        static GameWindow gameWindow;
        static Microsoft.Xna.Framework.Game game;
        static bool initialized;
        static bool hasWindowLayout;
        static Point currentWindowSize;
        static Rectangle currentRenderTargetDestination;

        public static void Init()
        {
            ThreadAffinity.AssertMainThread();
            if (initialized) return;
            initialized = true;
            InitializeScissorState();
            //Mouse.SetCursor(MouseCursor.FromTexture2D(Content.Load<Texture2D>(PATH_TO_IMAGE), 0, 0));
        }

        public static void SetManager(Microsoft.Xna.Framework.Game aGame)
        {
            ThreadAffinity.AssertMainThread();
            graphicsDeviceManager = new GraphicsDeviceManager(aGame);
            graphicsDeviceManager.GraphicsProfile = GraphicsProfile.HiDef;
            graphicsDeviceManager.ApplyChanges();
            graphicsAdapter = GraphicsAdapter.DefaultAdapter;
            gameWindow = aGame.Window;
            game = aGame;

        }

        static void PublishWindowLayoutChanged(Point windowSize, Rectangle renderTargetDestination)
        {
            hasWindowLayout = true;
            currentWindowSize = windowSize;
            currentRenderTargetDestination = renderTargetDestination;
            WindowLayoutChanged?.Invoke(windowSize, renderTargetDestination);
        }
    }
}
