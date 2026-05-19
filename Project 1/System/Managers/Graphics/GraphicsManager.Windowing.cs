using System;
using System.Diagnostics;
using System.Runtime.InteropServices;
using Microsoft.Xna.Framework;
using Project_1.Camera;
using Project_1.Messaging;
using Project_1.Messaging.Events;

namespace Project_1.Managers
{
    internal static partial class GraphicsManager
    {
        static readonly Point windowTitleBarMinSize = new Point(128, 32);
        static readonly object windowSizeLock = new object();
        static readonly bool isWindows = RuntimeInformation.IsOSPlatform(OSPlatform.Windows);

        static Rectangle cursorClipRect;
        static bool fullscreen;
        static bool pendingWindowSizeChange;
        static Point pendingWindowSize;
        static CameraSettings.WindowType pendingWindowMode;

        [DllImport("user32.dll")]
        static extern void ClipCursor(ref Rectangle rect);

        [DllImport("user32.dll", CharSet = CharSet.Auto, ExactSpelling = true)]
        static extern IntPtr GetForegroundWindow();

        [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        static extern int GetWindowThreadProcessId(IntPtr handle, out int processId);

        public static void Update()
        {
            ThreadAffinity.AssertMainThread();

            ClipCursor();

            CheckWindowSize();
        }

        static void CheckWindowSize()
        {
            if (!pendingWindowSizeChange) return;

            Point size;
            CameraSettings.WindowType mode;
            lock (windowSizeLock)
            {
                size = pendingWindowSize;
                mode = pendingWindowMode;
                pendingWindowSizeChange = false;
            }

            ApplyWindowSize(size, mode);
        }

        static void ClipCursor()
        {
            cursorClipRect.Location = gameWindow.Position;
            //Monogame rect is left, top, width, height but Windows API is left, top, right, bottom hence we add the size to the location to get the right and bottom values.
            cursorClipRect.Size = gameWindow.ClientBounds.Size + cursorClipRect.Location;
            if (isWindows && ApplicationIsActivated())
            {
                ClipCursor(ref cursorClipRect);
            }
        }

        public static void SetWindowSize(Point aSize, CameraSettings.WindowType aFullscreen)
        {
            if (!ThreadAffinity.IsMainThread)
            {
                lock (windowSizeLock)
                {
                    pendingWindowSize = aSize;
                    pendingWindowMode = aFullscreen;
                    pendingWindowSizeChange = true;
                }
                return;
            }

            ApplyWindowSize(aSize, aFullscreen);
        }

        static void ApplyWindowSize(Point aSize, CameraSettings.WindowType aFullscreen)
        {
            SetExternalWindowSize(aSize, aFullscreen, out Point appliedSize, out Rectangle renderTargetDestination);
            SetFullScreen(aFullscreen);

            graphicsDeviceManager.ApplyChanges();
            SetInternals(appliedSize, renderTargetDestination);
        }

        static void SetInternals(Point aSize, Rectangle aRenderTargetDestination)
        {

            Camera.Camera.SetWindowSize(new AbsoluteScreenPosition(aSize));
            MailboxManager.PublishUiEvent(new HudRescaleRequested(aSize));
            PublishWindowLayoutChanged(aSize, aRenderTargetDestination);
            uncapturedScissorRect = graphicsDeviceManager.GraphicsDevice.ScissorRectangle;
            graphicsDeviceManager.GraphicsDevice.ScissorRectangle = uncapturedScissorRect;
        }

        static void SetExternalWindowSize(Point aSize, CameraSettings.WindowType aFullscreen, out Point oAppliedSize, out Rectangle oRenderTargetDestination)
        {
            if (aFullscreen == CameraSettings.WindowType.Fullscreen)
            {
                oAppliedSize = NormalizeFullscreenSize(aSize);
                graphicsDeviceManager.PreferredBackBufferWidth = oAppliedSize.X;
                graphicsDeviceManager.PreferredBackBufferHeight = oAppliedSize.Y;
                oRenderTargetDestination = new Rectangle(Point.Zero, oAppliedSize);
            }
            else if (aFullscreen == CameraSettings.WindowType.Borderless)
            {
                oAppliedSize = CurrentDisplayModeSize;
                graphicsDeviceManager.PreferredBackBufferWidth = oAppliedSize.X;
                graphicsDeviceManager.PreferredBackBufferHeight = oAppliedSize.Y;
                oRenderTargetDestination = new Rectangle(Point.Zero, oAppliedSize);
            }
            else
            {
                oAppliedSize = NormalizeAllowedSize(aSize);
                graphicsDeviceManager.PreferredBackBufferWidth = oAppliedSize.X;
                graphicsDeviceManager.PreferredBackBufferHeight = oAppliedSize.Y;
                oRenderTargetDestination = new Rectangle(Point.Zero, oAppliedSize);
            }
        }

        static void SetFullScreen(CameraSettings.WindowType aFullscreen)
        {
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
            }

            fullscreen = aFullscreen != CameraSettings.WindowType.Windowed;
        }

        static Point NormalizeAllowedSize(Point aSize)
        {
            int maxX = Math.Max(windowTitleBarMinSize.X, graphicsAdapter.CurrentDisplayMode.Width);
            int maxY = Math.Max(windowTitleBarMinSize.Y, graphicsAdapter.CurrentDisplayMode.Height - windowTitleBarMinSize.Y);

            Point normalized = new Point(
                Math.Clamp(aSize.X, windowTitleBarMinSize.X, maxX),
                Math.Clamp(aSize.Y, windowTitleBarMinSize.Y, maxY));

            if (normalized != aSize)
            {
                DebugManager.Print($"Adjusted window size from {aSize} to {normalized}");
            }

            return normalized;
        }

        static Point NormalizeFullscreenSize(Point aSize)
        {
            Point[] supportedSizes = GetSupportedFullscreenSizes();
            for (int i = 0; i < supportedSizes.Length; i++)
            {
                if (supportedSizes[i] == aSize)
                {
                    return aSize;
                }
            }

            Point fallback = CurrentDisplayModeSize;
            DebugManager.Print($"Adjusted fullscreen size from {aSize} to {fallback}");
            return fallback;
        }

        static Rectangle GetRenderTargetDestination(Point resolution, int preferredBackBufferWidth, int preferredBackBufferHeight)
        {
            //Q: What is/was this for? Why isnt is used?
            float resolutionRatio = (float)resolution.X / resolution.Y;
            Point bounds = new Point(preferredBackBufferWidth, preferredBackBufferHeight);
            float screenRatio = (float)bounds.X / bounds.Y;
            float scale;
            Rectangle rectangle = new Rectangle();

            if (resolutionRatio < screenRatio)
            {
                scale = (float)bounds.Y / resolution.Y;
            }
            else if (resolutionRatio > screenRatio)
            {
                scale = (float)bounds.X / resolution.X;
            }
            else
            {
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

        static bool AllowedSize(Point aSize) //TODO: Move this closer to the UI code that is trying to set the window size and make it return an error message that can be displayed to the user instead of just printing to debug.
        {
            if (aSize.Y > graphicsAdapter.CurrentDisplayMode.Height - windowTitleBarMinSize.Y)
            {
                DebugManager.Print("My Y is too big");
                return false;
            }

            if (aSize.X > graphicsAdapter.CurrentDisplayMode.Width)
            {
                DebugManager.Print("My X is too big");
                return false;
            }

            if (aSize.X < windowTitleBarMinSize.X)
            {
                DebugManager.Print("Tried to set it smaller than the required lenght");
                return false;
            }

            return true;
        }

        public static bool ApplicationIsActivated()
        {
            if (isWindows)
            {
                IntPtr activatedHandle = GetForegroundWindow();
                if (activatedHandle == IntPtr.Zero)
                {
                    return false;
                }

                int procId = Process.GetCurrentProcess().Id;
                _ = GetWindowThreadProcessId(activatedHandle, out int activeProcId);
                return activeProcId == procId;
            }

            //TODO: Implement other platforms

            if (game != null) //Q: Is there a scenario where game could be null at this point? If not, should we throw an exception instead of just returning true?
            {
                return game.IsActive;
            }

            return true;
        }
    }
}
