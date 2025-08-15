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
using System.Windows.Forms;

namespace Project_1.Camera
{

    internal static class Camera
    {

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

        public static Rectangle WorldRectangle { get => new Rectangle(cameraMover.CentreInWorldSpace.ToPoint() - (CentrePointInScreenSpace / Scale).ToPoint(), (WindowSize / Scale).ToPoint()); }

        public static AbsoluteScreenPosition CentrePointInScreenSpace { get => WindowSize / 2; }

        public readonly static Point devScreenBorder = new Point(1500, 900);
        public static AbsoluteScreenPosition WindowSize
        {
            get => new AbsoluteScreenPosition(cameraSettings.WindowSize);
            set => cameraSettings.WindowSize = new AbsoluteScreenPosition(value);
        }
        public static Point WindowSizeAsPoint
        {
            get => cameraSettings.WindowSize;
            set => cameraSettings.WindowSize = value;
        }

        static float scale = 1f;
        static float minScale = 0.7f;
        static float maxScale = 1.4f;


        static CameraSettings cameraSettings;

        static CameraMover cameraMover;


        static Camera()
        {
            ImportSettings();
            cameraMover = new CameraMover();
            cameraSettings.SetCamera();
        }

        public static void Update()
        {
            cameraMover.Move();
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
            SaveManager.ExportData(aSave.CameraPosition, cameraMover.CentreInWorldSpace);
        }

        public static void LoadPosition(Save aSave)
        {
            string json = File.ReadAllText(aSave.CameraPosition);
            WorldSpace ws = SaveManager.ImportData<WorldSpace>(json);
            cameraMover.CentreInWorldSpace = ws;
            //TODO: Ponder if the bound object should also be saved
        }
        #endregion

        #region Zoom
        public static float Scale { get => scale; }
        public static float Zoom { get => 1f / scale; }
        public static WorldSpace CentreInWorldSpace { get => cameraMover.CentreInWorldSpace; internal set => cameraMover.CentreInWorldSpace = value; }

        internal static void Scroll(ScrollEvent aScrollEvent)
        {
            ZoomIn(aScrollEvent);
            ZoomOut(aScrollEvent);
        }

        static void ZoomIn(ScrollEvent aScrollEvent)
        {
            if (!aScrollEvent.Up) return;
            if (scale <= minScale) return;
            scale -= 0.05f * aScrollEvent.Steps;

            DebugManager.Print(typeof(Camera), "Scale is now " + scale);
            cameraMover.bindingRectangle.Size = new Point((int)(WindowSize.X / 4 * 3 * Zoom), (int)(WindowSize.Y / 4 * 3 * Zoom));
        }

        static void ZoomOut(ScrollEvent aScrollEvent)
        {
            if (!aScrollEvent.Down) return;
            if (scale >= maxScale) return;
            scale += 0.05f * aScrollEvent.Steps;
            DebugManager.Print(typeof(Camera), "Scale is now " + scale);

            cameraMover.bindingRectangle.Size = new Point((int)(WindowSize.X / 4 * 3 * Zoom), (int)(WindowSize.Y / 4 * 3 * Zoom));
        }
        #endregion

        public static void BindCamera(MovingObject aBinder)
        {
            cameraMover.BindCamera(aBinder);
        }

        public static void SetWindowSize(AbsoluteScreenPosition aSize)
        {
            float x = devScreenBorder.X / aSize.X;
            float y = devScreenBorder.Y / aSize.Y;
            scale = Math.Max(x, y);
            minScale = scale - 0.3f;
            maxScale = scale + 0.4f;
            cameraMover.bindingRectangle = new Rectangle(new Point(0), new Point(aSize.X / 4 * 3, aSize.Y / 4 * 3));
            cameraMover.maxCircleCameraMove = aSize.Y / 3;


        }


        public static Rectangle WorldRectToScreenRect(Rectangle aWorldPos)
        {
            Point topLeft = (cameraMover.CentreInWorldSpace * scale - WindowSize.ToVector2() / 2).ToPoint();
            Rectangle cameraPos = new Rectangle((aWorldPos.Location.ToVector2() * scale).ToPoint() - topLeft, (aWorldPos.Size.ToVector2() * scale).ToPoint());
            return cameraPos;
        }


        #region FrameBoundry
        public static bool MomAmIInFrame(Rectangle aRect)
        {
            return ScreenRectangle.Intersects(aRect);
        }

        public static bool MomAmIInFrame(Vector2 aWorldPos)
        {
            return WorldRectangle.Contains(aWorldPos);
        }
        #endregion

        public static void MinimapDraw(SpriteBatch aBatch, WorldSpace aOrigin, AbsoluteScreenPosition aMinimapOffset, AbsoluteScreenPosition aMinimapSize)
        {
            UI.UIElements.Minimap.minimapDot.Draw(aBatch, new Rectangle(new AbsoluteScreenPosition(WorldRectangle.Location - aOrigin.ToPoint()) / (TileManager.TileSize) + aMinimapOffset + aMinimapSize / 2, new Point(1, WorldRectangle.Size.Y / TileManager.TileSize.Y)), Color.White);
            UI.UIElements.Minimap.minimapDot.Draw(aBatch, new Rectangle(new AbsoluteScreenPosition(WorldRectangle.Location - aOrigin.ToPoint()) / (TileManager.TileSize) + aMinimapOffset + aMinimapSize / 2, new Point(WorldRectangle.Size.X / TileManager.TileSize.X, 1)), Color.White);
            UI.UIElements.Minimap.minimapDot.Draw(aBatch, new Rectangle(new AbsoluteScreenPosition(WorldRectangle.Location + new Point(WorldRectangle.Size.X, 0) - aOrigin.ToPoint()) / (TileManager.TileSize) + aMinimapOffset + aMinimapSize / 2, new Point(1, WorldRectangle.Size.Y / TileManager.TileSize.Y)), Color.White);
            UI.UIElements.Minimap.minimapDot.Draw(aBatch, new Rectangle(new AbsoluteScreenPosition(WorldRectangle.Location + new Point(0, WorldRectangle.Size.Y)- aOrigin.ToPoint()) / (TileManager.TileSize) + aMinimapOffset + aMinimapSize / 2, new Point(WorldRectangle.Size.X / TileManager.TileSize.X, 1)), Color.White);

        }
    }
}
