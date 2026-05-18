using System.Collections.Generic;
using System.Diagnostics;
using Microsoft.Xna.Framework;

namespace Project_1.Managers
{
    internal static partial class GraphicsManager
    {
        static Rectangle uncapturedScissorRect;
        static List<(object captor, Rectangle rectangle)> scissors;

        static void InitializeScissorState()
        {
            scissors = new List<(object, Rectangle)>();
        }

        public static bool CaptureScissor(object aCaptor, Rectangle aRect)
        {
            ThreadAffinity.AssertMainThread();

            Rectangle r = aRect;
            for (int i = 0; i < scissors.Count; i++)
            {
                //Q: If this is the desired behaviour, why not just assert that the smaller rect is fully contained within the larger one.
                r = Rectangle.Intersect(r, scissors[i].rectangle);
            }

            scissors.Add((aCaptor, r));
            graphicsDeviceManager.GraphicsDevice.ScissorRectangle = r;
            return true;
        }

        public static bool ReleaseScissor(object aReleaser)
        {
            ThreadAffinity.AssertMainThread();

            if (scissors.Count == 0)
            {
                Debug.Assert(false, "ReleaseScissor called with an empty scissor stack.");
                graphicsDeviceManager.GraphicsDevice.ScissorRectangle = uncapturedScissorRect;
                return false;
            }

            if (aReleaser != scissors[scissors.Count - 1].captor)
            {
                Debug.Assert(false, "ReleaseScissor called out of order.");
                return false;
            }

            scissors.RemoveAt(scissors.Count - 1);

            if (scissors.Count == 0)
            {
                graphicsDeviceManager.GraphicsDevice.ScissorRectangle = uncapturedScissorRect;
                return true;
            }

            graphicsDeviceManager.GraphicsDevice.ScissorRectangle = scissors[scissors.Count - 1].rectangle;
            return true;
        }

        public static void AssertScissorStackEmpty()
        {
            ThreadAffinity.AssertMainThread();
            Debug.Assert(scissors.Count == 0, "Scissor stack not empty at start of UI draw.");
        }
    }
}
