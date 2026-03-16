using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Project_1.GameObjects;
using Project_1.GameObjects.Spawners;
using Project_1.Input;
using Project_1.Managers;
using Project_1.Managers.Saves;
using Project_1.Particles;
using Project_1.Textures;
using Project_1.Tiles;
using Project_1.UI;
using Project_1.UI.HUD;
using Project_1.UI.OptionMenu;
using System;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.IO;
using System.Runtime.CompilerServices;
using System.Security.Permissions;
using System.Threading;

namespace Project_1.Camera
{

    internal static class Camera
    {
        sealed class CameraRenderSnapshot
        {
            public CameraRenderSnapshot(WorldSpace centreInWorldSpace, AbsoluteScreenPosition windowSize, float scale, Rectangle worldRectangle, AbsoluteScreenPosition centrePointInScreenSpace)
            {
                CentreInWorldSpace = centreInWorldSpace;
                WindowSize = windowSize;
                Scale = scale;
                WorldRectangle = worldRectangle;
                CentrePointInScreenSpace = centrePointInScreenSpace;
            }

            public WorldSpace CentreInWorldSpace { get; }
            public AbsoluteScreenPosition WindowSize { get; }
            public float Scale { get; }
            public Rectangle WorldRectangle { get; }
            public AbsoluteScreenPosition CentrePointInScreenSpace { get; }
        }

        public static Rectangle ScreenRectangle { get => new Rectangle(Point.Zero, WindowSize.ToPoint()); }

        public static CameraSettings.Follow CurrentCameraSetting
        {
            get => cameraSettings.FollowSetting;
            set
            {
                //TODO: CameraStyleSelect.instance.SetValueFromOutside((int)aCameraSettings);
                cameraSettings.FollowSetting = value;
            }
        }

        public static CameraSettings.WindowType FullScreen
        {
            get => cameraSettings.Fullscreen;
            set => cameraSettings.Fullscreen = value;
        }

        public static Rectangle WorldRectangle
        {
            get
            {
                if (OnSimulationOwnerThread) return BuildLiveWorldRectangle();
                return GetRenderSnapshotForRead().WorldRectangle;
            }
        }

        public static AbsoluteScreenPosition CentrePointInScreenSpace
        {
            get
            {
                if (OnSimulationOwnerThread) return BuildLiveCentrePointInScreenSpace();
                return GetRenderSnapshotForRead().CentrePointInScreenSpace;
            }
        }

        public readonly static Point devScreenBorder = new Point(1500, 900);
        public static AbsoluteScreenPosition WindowSize
        {
            get
            {
                if (OnSimulationOwnerThread) return BuildLiveWindowSize();
                return GetRenderSnapshotForRead().WindowSize;
            }
            set => cameraSettings.WindowSize = new AbsoluteScreenPosition(value);
        }
        public static Point WindowSizeAsPoint
        {
            get => WindowSize.ToPoint();
            set => cameraSettings.WindowSize = value;
        }

        static float scale = 1f;
        static float minScale = 0.7f;
        static float maxScale = 1.4f;


        static CameraSettings cameraSettings;

        static CameraMover cameraMover;
        static bool initialized;
        static Rectangle minimapWorldRectangle;
        static volatile bool minimapWorldRectangleValid;
        static volatile CameraRenderSnapshot renderSnapshot;
        static volatile CameraRenderSnapshot renderFrameSnapshot;
        static int renderFrameSnapshotActive;

        static bool OnSimulationOwnerThread => ThreadAffinity.IsSimThread || (!SimThread.IsRunning && ThreadAffinity.IsMainThread);

        public static void Init()
        {
            ThreadAffinity.AssertMainThread();
            if (initialized) return;
            initialized = true;

            ImportSettings();
            cameraMover = new CameraMover();
            cameraSettings.SetCamera();
            PublishRenderSnapshot();
        }

        public static void Update()
        {
            ThreadAffinity.AssertSimThread();
            cameraMover.Move();
            PublishRenderSnapshot();
        }

        internal static void BuildMinimapSnapshot()
        {
            ThreadAffinity.AssertSimThread();
            minimapWorldRectangle = BuildLiveWorldRectangle();
            minimapWorldRectangleValid = true;
        }

        internal static bool TryGetMinimapWorldRectangle(out Rectangle rect)
        {
            ThreadAffinity.AssertMainThread();
            rect = minimapWorldRectangle;
            return minimapWorldRectangleValid;
        }

        #region Save/Load
        static void ImportSettings()
        {
            if (File.Exists(SaveManager.CameraSettings))
            {
                string json = File.ReadAllText(SaveManager.CameraSettings);
                cameraSettings = SaveManager.ImportData<CameraSettings>(json);
                Debug.Assert(cameraSettings != null);
            }
            else
            {
                string json = File.ReadAllText(SaveManager.DefaultCameraSettings);
                cameraSettings = SaveManager.ImportData<CameraSettings>(json);
            }
        }

        public static void ExportSettings()
        {
            SaveManager.ExportData(SaveManager.CameraSettings, cameraSettings);
        }

        public static void SavePosition(Save aSave)
        {
            ThreadAffinity.AssertSimThread();
            SaveManager.ExportData(aSave.CameraPosition, cameraMover.CentreInWorldSpace);
        }

        public static void LoadPosition(Save aSave)
        {
            ThreadAffinity.AssertSimThread();
            string json = File.ReadAllText(aSave.CameraPosition);
            WorldSpace ws = SaveManager.ImportData<WorldSpace>(json);
            cameraMover.CentreInWorldSpace = ws;
            PublishRenderSnapshot();
            //TODO: Ponder if the bound object should also be saved
        }
        #endregion

        #region Zoom
        public static float Scale
        {
            get
            {
                if (OnSimulationOwnerThread) return scale;
                return GetRenderSnapshotForRead().Scale;
            }
        }
        public static float Zoom { get => 1f / Scale; }
        public static WorldSpace CentreInWorldSpace
        {
            get
            {
                if (OnSimulationOwnerThread) return cameraMover.CentreInWorldSpace;
                return GetRenderSnapshotForRead().CentreInWorldSpace;
            }
            internal set
            {
                ThreadAffinity.AssertSimThread();
                cameraMover.CentreInWorldSpace = value;
                PublishRenderSnapshot();
            }
        }

        internal static void Scroll(ScrollEvent aScrollEvent)
        {
            ThreadAffinity.AssertSimThread();
            float before = scale;
            ZoomIn(aScrollEvent);
            ZoomOut(aScrollEvent);
            if (Math.Abs(before - scale) > float.Epsilon)
            {
                PublishRenderSnapshot();
            }
        }

        static void ZoomIn(ScrollEvent aScrollEvent)
        {
            if (!aScrollEvent.Up) return;
            if (scale <= minScale) return;
            scale -= 0.05f * aScrollEvent.Steps;

            DebugManager.Print("Scale is now " + scale);
            cameraMover.bindingRectangle.Size = new Point((int)(WindowSize.X / 4 * 3 * Zoom), (int)(WindowSize.Y / 4 * 3 * Zoom));
        }

        static void ZoomOut(ScrollEvent aScrollEvent)
        {
            if (!aScrollEvent.Down) return;
            if (scale >= maxScale) return;
            scale += 0.05f * aScrollEvent.Steps;
            DebugManager.Print("Scale is now " + scale);

            cameraMover.bindingRectangle.Size = new Point((int)(WindowSize.X / 4 * 3 * Zoom), (int)(WindowSize.Y / 4 * 3 * Zoom));
        }
        #endregion

        public static void BindCamera(MovingObject aBinder)
        {
            ThreadAffinity.AssertSimThread();
            cameraMover.BindCamera(aBinder);
        }

        public static void SetWindowSize(AbsoluteScreenPosition aSize)
        {
            ThreadAffinity.AssertMainThread();
            float x = devScreenBorder.X / aSize.X;
            float y = devScreenBorder.Y / aSize.Y;
            scale = Math.Max(x, y);
            minScale = scale - 0.3f;
            maxScale = scale + 0.4f;
            cameraMover.bindingRectangle = new Rectangle(new Point(0), new Point(aSize.X / 4 * 3, aSize.Y / 4 * 3));
            cameraMover.maxCircleCameraMove = aSize.Y / 3;
            PublishRenderSnapshot();

        }


        public static Rectangle WorldRectToScreenRect(Rectangle aWorldPos)
        {
            CameraRenderSnapshot snapshot = GetRenderSnapshotForRead();
            Point topLeft = (snapshot.CentreInWorldSpace * snapshot.Scale - snapshot.WindowSize.ToVector2() / 2).ToPoint();
            Rectangle cameraPos = new Rectangle((aWorldPos.Location.ToVector2() * snapshot.Scale).ToPoint() - topLeft, (aWorldPos.Size.ToVector2() * snapshot.Scale).ToPoint());
            return cameraPos;
        }


        #region FrameBoundry
        public static bool ScreenspaceBoundsCheck(Rectangle aRect) => ScreenRectangle.Intersects(aRect);

        public static bool WorldspaceBoundsCheck(Rectangle aRectangle) => WorldRectangle.Intersects(aRectangle);

        public static bool WorldspaceBoundsCheck(Vector2 aWorldPos) => WorldRectangle.Contains(aWorldPos);
        #endregion

        public static void MinimapDraw(SpriteBatch aBatch, WorldSpace aOrigin, AbsoluteScreenPosition aMinimapOffset, AbsoluteScreenPosition aMinimapSize)
        {
            ThreadAffinity.AssertMainThread();
            if (!TryGetMinimapWorldRectangle(out Rectangle worldRect)) return;
            int minX = (int)MathF.Floor((worldRect.Left - aOrigin.X) / Tile.Size.X);
            int minY = (int)MathF.Floor((worldRect.Top - aOrigin.Y) / Tile.Size.Y);
            int width = Math.Max(1, (int)MathF.Ceiling(worldRect.Width / (float)Tile.Size.X));
            int height = Math.Max(1, (int)MathF.Ceiling(worldRect.Height / (float)Tile.Size.Y));

            Point minimapCentre = (aMinimapOffset + aMinimapSize / 2).ToPoint();
            Point topLeft = minimapCentre + new Point(minX, minY);
            Point topRight = new Point(topLeft.X + width - 1, topLeft.Y);
            Point bottomLeft = new Point(topLeft.X, topLeft.Y + height - 1);

            UI.UIElements.Minimap.minimapDot.Draw(aBatch, new Rectangle(topLeft, new Point(1, height)), Color.White);
            UI.UIElements.Minimap.minimapDot.Draw(aBatch, new Rectangle(topLeft, new Point(width, 1)), Color.White);
            UI.UIElements.Minimap.minimapDot.Draw(aBatch, new Rectangle(topRight, new Point(1, height)), Color.White);
            UI.UIElements.Minimap.minimapDot.Draw(aBatch, new Rectangle(bottomLeft, new Point(width, 1)), Color.White);

        }

        static AbsoluteScreenPosition BuildLiveWindowSize()
        {
            return cameraSettings == null ? AbsoluteScreenPosition.Zero : new AbsoluteScreenPosition(cameraSettings.WindowSize);
        }

        static AbsoluteScreenPosition BuildLiveCentrePointInScreenSpace()
        {
            return BuildLiveWindowSize() / 2f;
        }

        static Rectangle BuildLiveWorldRectangle()
        {
            AbsoluteScreenPosition windowSize = BuildLiveWindowSize();
            AbsoluteScreenPosition centrePoint = windowSize / 2f;
            float safeScale = Math.Abs(scale) < float.Epsilon ? 1f : scale;
            WorldSpace centre = cameraMover == null ? WorldSpace.Zero : cameraMover.CentreInWorldSpace;
            return new Rectangle(centre.ToPoint() - (centrePoint / safeScale).ToPoint(), (windowSize / safeScale).ToPoint());
        }

        static CameraRenderSnapshot BuildRenderSnapshotFromLive()
        {
            AbsoluteScreenPosition windowSize = BuildLiveWindowSize();
            AbsoluteScreenPosition centrePoint = windowSize / 2f;
            float safeScale = Math.Abs(scale) < float.Epsilon ? 1f : scale;
            WorldSpace centre = cameraMover == null ? WorldSpace.Zero : cameraMover.CentreInWorldSpace;
            Rectangle worldRect = new Rectangle(centre.ToPoint() - (centrePoint / safeScale).ToPoint(), (windowSize / safeScale).ToPoint());
            return new CameraRenderSnapshot(centre, windowSize, safeScale, worldRect, centrePoint);
        }

        static void PublishRenderSnapshot()
        {
            renderSnapshot = BuildRenderSnapshotFromLive();
        }

        static CameraRenderSnapshot GetRenderSnapshotForRead()
        {
            if (OnSimulationOwnerThread)
            {
                return BuildRenderSnapshotFromLive();
            }

            if (ThreadAffinity.IsMainThread && Volatile.Read(ref renderFrameSnapshotActive) == 1)
            {
                CameraRenderSnapshot frameSnapshot = renderFrameSnapshot;
                if (frameSnapshot != null) return frameSnapshot;
            }

            CameraRenderSnapshot snapshot = renderSnapshot;
            if (snapshot != null) return snapshot;

            // Safety fallback during startup / before first publish.
            return BuildRenderSnapshotFromLive();
        }

        internal static void BeginMainThreadRenderFrame()
        {
            ThreadAffinity.AssertMainThread();
            if (OnSimulationOwnerThread) return;

            CameraRenderSnapshot snapshot = renderSnapshot;
            if (snapshot == null)
            {
                snapshot = BuildRenderSnapshotFromLive();
            }
            renderFrameSnapshot = snapshot;
            Volatile.Write(ref renderFrameSnapshotActive, 1);
        }

        internal static void EndMainThreadRenderFrame()
        {
            ThreadAffinity.AssertMainThread();
            if (OnSimulationOwnerThread) return;

            Volatile.Write(ref renderFrameSnapshotActive, 0);
            renderFrameSnapshot = null;
        }

        internal static AbsoluteScreenPosition WorldToAbsoluteScreenPosition(WorldSpace world)
        {
            CameraRenderSnapshot snapshot = GetRenderSnapshotForRead();
            WorldSpace topLeft = snapshot.CentreInWorldSpace * snapshot.Scale - new WorldSpace(snapshot.WindowSize.ToVector2() / 2);
            return new AbsoluteScreenPosition(
                (int)Math.Floor(world.X * snapshot.Scale - topLeft.X),
                (int)Math.Floor(world.Y * snapshot.Scale - topLeft.Y));
        }

        internal static WorldSpace AbsoluteScreenToWorld(AbsoluteScreenPosition screenPos)
        {
            CameraRenderSnapshot snapshot = GetRenderSnapshotForRead();
            WorldSpace vectorInScreen = (WorldSpace)(snapshot.CentrePointInScreenSpace - screenPos).ToVector2();
            float zoom = 1f / snapshot.Scale;
            return (WorldSpace)(snapshot.CentreInWorldSpace - vectorInScreen * zoom);
        }
    }
}
