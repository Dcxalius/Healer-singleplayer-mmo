using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Project_1.GameObjects;
using Project_1.GameObjects.Spawners;
using Project_1.Input;
using Project_1.Managers;
using Project_1.Particles;
using Project_1.Textures;
using Project_1.Tiles;
using Project_1.UI;
using Project_1.UI.HUD;
using Project_1.UI.OptionMenu;
using System;
using System.Data;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Security.Permissions;
using System.Windows.Forms;

namespace Project_1.Camera
{

    internal static class Camera
    {
        public enum CameraSettings
        {
            Free,
            CircleSoftBound,
            RectangleSoftBound,
            Hardbound,
            Count
        }

        public static float Scale { get => scale; }

        public static float Zoom { get => 1f / scale; }


        public static Rectangle ScreenRectangle { get => new Rectangle(Point.Zero, ScreenSize.ToPoint()); }

        public static AbsoluteScreenPosition ScreenSize { get => screenRectangleSize; }

        public static CameraSettings CurrentCameraSetting { get => cameraSettings; }

        public static Rectangle WorldRectangle { get => new Rectangle(centreInWorldSpace.ToPoint() - (CentrePointInScreenSpace / Scale).ToPoint(), (ScreenSize / Scale).ToPoint()); }
        public static WorldSpace CentreInWorldSpace { get => centreInWorldSpace; set => centreInWorldSpace = value; }

        public static AbsoluteScreenPosition CentrePointInScreenSpace { get => screenRectangleSize / 2; }



        static WorldSpace centreInWorldSpace = new WorldSpace(100, 100);



        public readonly static Point devScreenBorder = new Point(1500, 900);
        static AbsoluteScreenPosition screenRectangleSize;



        static float scale = 1f;
        static float minScale = 0.7f;
        static float maxScale = 1.4f;

        //static Rectangle maxRectangleCameraMove;
        static CameraSettings cameraSettings = CameraSettings.RectangleSoftBound;



        static CameraMover cameraMover = new CameraMover();




        public static void Init()
        {
        }

        public static void Update()
        {
            cameraMover.Move();
        }


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
            cameraMover.bindingRectangle.Size = new Point((int)(screenRectangleSize.X / 4 * 3 * Zoom), (int)(screenRectangleSize.Y / 4 * 3 * Zoom));
        }

        static void ZoomOut(ScrollEvent aScrollEvent)
        {
            if (!aScrollEvent.Down) return;
            if (scale >= maxScale) return;
            scale += 0.05f * aScrollEvent.Steps;
            DebugManager.Print(typeof(Camera), "Scale is now " + scale);

            cameraMover.bindingRectangle.Size = new Point((int)(screenRectangleSize.X / 4 * 3 * Zoom), (int)(screenRectangleSize.Y / 4 * 3 * Zoom));
        }

        public static void BindCamera(MovingObject aBinder)
        {
            cameraMover.BindCamera(aBinder);
        }

        public static void SetCamera(CameraSettings aCameraSettings)
        {
            CameraStyleSelect.instance.SetValueFromOutside((int)aCameraSettings);
            cameraSettings = aCameraSettings;
        }

        public static void SetWindowSize(AbsoluteScreenPosition aSize)
        {
            screenRectangleSize = aSize;

            float x = devScreenBorder.X / aSize.X;
            float y = devScreenBorder.Y / aSize.Y;
            scale = Math.Max(x, y);
            minScale = scale - 0.3f;
            maxScale = scale + 0.4f;
            cameraMover.bindingRectangle = new Rectangle(new Point(0), new Point(screenRectangleSize.X / 4 * 3, screenRectangleSize.Y / 4 * 3));
            cameraMover.maxCircleCameraMove = screenRectangleSize.Y / 3;

            Init();

        }


        public static Rectangle WorldRectToScreenRect(Rectangle aWorldPos)
        {
            Point topLeft = (centreInWorldSpace * scale - screenRectangleSize.ToVector2() / 2).ToPoint();
            Rectangle cameraPos = new Rectangle((aWorldPos.Location.ToVector2() * scale).ToPoint() - topLeft, (aWorldPos.Size.ToVector2() * scale).ToPoint());
            return cameraPos;
        }



        public static bool MomAmIInFrame(Rectangle aRect)
        {
            return ScreenRectangle.Intersects(aRect);
        }

        public static bool MomAmIInFrame(Vector2 aWorldPos)
        {
            return WorldRectangle.Contains(aWorldPos);
        }

        

        

    }
}
