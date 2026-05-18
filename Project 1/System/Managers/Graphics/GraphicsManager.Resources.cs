using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System.Diagnostics;

namespace Project_1.Managers
{
    internal static partial class GraphicsManager
    {
        public static GraphicsDevice GraphicsDevice => graphicsDeviceManager.GraphicsDevice;

        [DebuggerStepThrough]
        public static Texture2D CreateNewTexture(Point aSize)
        {
            ThreadAffinity.AssertMainThread();
            return new Texture2D(graphicsDeviceManager.GraphicsDevice, aSize.X, aSize.Y);
        }

        [DebuggerStepThrough]
        public static SpriteBatch CreateSpriteBatch()
        {
            ThreadAffinity.AssertMainThread();
            return new SpriteBatch(graphicsDeviceManager.GraphicsDevice);
        }

        [DebuggerStepThrough]
        public static RenderTarget2D CreateRenderTarget(Point aSize, bool withDepth = false)
        {
            ThreadAffinity.AssertMainThread();
            return new RenderTarget2D(
                graphicsDeviceManager.GraphicsDevice,
                aSize.X,
                aSize.Y,
                false,
                SurfaceFormat.Color,
                withDepth ? DepthFormat.Depth24 : DepthFormat.None);
        }

        [DebuggerStepThrough]
        public static Texture2D CreateTextureFromFile(string aPath)
        {
            ThreadAffinity.AssertMainThread();
            return Texture2D.FromFile(graphicsDeviceManager.GraphicsDevice, aPath);
        }

        [DebuggerStepThrough]
        public static void SetRenderTarget(RenderTarget2D aRenderTarget)
        {
            ThreadAffinity.AssertMainThread();
            graphicsDeviceManager.GraphicsDevice.SetRenderTarget(aRenderTarget);
        }

        [DebuggerStepThrough]
        public static void ClearScreen(Color aColor)
        {
            ThreadAffinity.AssertMainThread();
            graphicsDeviceManager.GraphicsDevice.Clear(aColor);
        }
    }
}
