using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Project_1.Managers
{
    internal static partial class GraphicsManager
    {
        public static Texture2D CreateNewTexture(Point aSize)
        {
            ThreadAffinity.AssertMainThread();
            return new Texture2D(graphicsDeviceManager.GraphicsDevice, aSize.X, aSize.Y);
        }

        public static SpriteBatch CreateSpriteBatch()
        {
            ThreadAffinity.AssertMainThread();
            return new SpriteBatch(graphicsDeviceManager.GraphicsDevice);
        }

        public static RenderTarget2D CreateRenderTarget(Point aSize)
        {
            ThreadAffinity.AssertMainThread();
            return new RenderTarget2D(graphicsDeviceManager.GraphicsDevice, aSize.X, aSize.Y);
        }

        public static Texture2D CreateTextureFromFile(string aPath)
        {
            ThreadAffinity.AssertMainThread();
            return Texture2D.FromFile(graphicsDeviceManager.GraphicsDevice, aPath);
        }

        public static void SetRenderTarget(RenderTarget2D aRenderTarget)
        {
            ThreadAffinity.AssertMainThread();
            graphicsDeviceManager.GraphicsDevice.SetRenderTarget(aRenderTarget);
        }

        public static void ClearScreen(Color aColor)
        {
            ThreadAffinity.AssertMainThread();
            graphicsDeviceManager.GraphicsDevice.Clear(aColor);
        }
    }
}
