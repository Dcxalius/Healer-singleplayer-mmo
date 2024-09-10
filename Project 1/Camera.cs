using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Project_1.GameObjects;
using Project_1.Input;
using Project_1.Managers;
using Project_1.Textures;
using Project_1.Tiles;
using Project_1.UI;
using Project_1.UI.OptionMenu;
using System;
using System.Data;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Windows.Forms;

namespace Project_1
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

        public static float Scale
        {
             get { return scale; }
        }

        public static float Zoom
        {
            get { return 1f / scale; }
        }

        public static Rectangle ScreenRectangle
        {
            get => screenRectangle;
        }

        public static CameraSettings CurrentCameraSetting
        {
            get => cameraSettings;
        }
        
        public static Vector2 CentreInWorldSpace
        {
            get => centreInWorldSpace;
            set => centreInWorldSpace = value;
        }

        public static Point CentrePointInScreenSpace { get => new Point(devScreenBorder.X / 2, devScreenBorder.Y / 2); }

        static SpriteBatch spriteBatch;

        static Vector2 centreInWorldSpace = new Vector2(100,100);

        //---

        //--- debugText is fine but should pauseGfx be somewhere else?
        static Texture2D debugTexture = GraphicsManager.GetTexture(new GfxPath(GfxType.Debug, "Debug"));
        static Textures.Texture pauseGfx = new Textures.Texture(new GfxPath(GfxType.UI, "PauseBackground"));

        //--

        public readonly static Point devScreenBorder = new Point(1500, 900);
        static Rectangle screenRectangle;



        static float scale = 1f;
        static float minScale = 0.7f;
        static float maxScale = 1.4f;

        //--
        //static Rectangle maxRectangleCameraMove;
        static CameraSettings cameraSettings = CameraSettings.RectangleSoftBound;

        //--

        static RenderTarget2D cameraTarget;
        static Rectangle renderTargetPosition;

        static CameraMover cameraMover = new CameraMover();

        public static Rectangle RenderTargetPosition
        {
            set => renderTargetPosition = value;
        }

        public static void Init()
        {
            //SetWindowSize(devScreenBorder);
            spriteBatch = GraphicsManager.CreateSpriteBatch();
        }

        public static void Update()
        {
            ScrollZoom();

            cameraMover.Move();
        }
        public static void BindCamera(MovingObject aBinder)
        {
            cameraMover.BindCamera(aBinder);
        }

        static void ScrollZoom()
        {
            int scrolled = InputManager.ScrolledSinceLastFrame;
            if (scrolled != 0)
            {
                if ((scrolled > 0 && scale <= minScale) || (scrolled < 0 && scale >= maxScale))
                {
                    return;
                }
                scale -= scrolled / 2400f; //A single mousewheel step is 120 so 2400 gives a movement of 5% points per mousewheel step
                DebugManager.Print(typeof(Camera), "Centre point: " + centreInWorldSpace);
                cameraMover.bindingRectangle.Size = new Point((int)(screenRectangle.Size.X / 4 * 3 * Zoom), (int)(screenRectangle.Size.Y / 4 * 3 * Zoom));

            }
        }

        public static (Vector2 , Vector2) TransformDevSizeToRelativeVectors(Rectangle aRect)
        {
            Vector2 pos = aRect.Location.ToVector2() / devScreenBorder.ToVector2();
            Vector2 size = aRect.Size.ToVector2() / devScreenBorder.ToVector2();

            return (pos, size);
        }

        public static Point TransformRelativeToAbsoluteScreenSpace(Vector2 aPos) 
        {
            return (screenRectangle.Size.ToVector2() * aPos).ToPoint();
        }
        public static Vector2 TransformAbsoluteToRelativeScreenSpace(Point aPos)
        {
            return (aPos.ToVector2() / screenRectangle.Size.ToVector2());
        }


        public static void SetCamera(CameraSettings aCameraSettings)
        {
            CameraStyleSelect.instance.SetValueFromOutside((int)aCameraSettings);
            cameraSettings = aCameraSettings;
        }

        public static void SetWindowSize(Point aSize)
        {
            cameraTarget = GraphicsManager.CreateRenderTarget(aSize);
            screenRectangle.Size = aSize;
            //zoom = something xd
            //scale = scale 
            cameraMover.bindingRectangle = new Rectangle(new Point(0), new Point(screenRectangle.Size.X / 4 * 3, screenRectangle.Size.Y / 4 * 3));
            cameraMover.maxCircleCameraMove = screenRectangle.Size.Y / 3;
        }

       
        public static Rectangle WorldPosToCameraSpace(Rectangle aWorldPos)
        {
            Point topLeft = (centreInWorldSpace * scale - screenRectangle.Size.ToVector2() / 2).ToPoint();
            Rectangle cameraPos = new Rectangle((aWorldPos.Location.ToVector2() * scale).ToPoint() - topLeft, aWorldPos.Size);
            return cameraPos;
        }

        public static Vector2 WorldPosToCameraSpace(Vector2 aWorldPos)
        {
            Vector2 topLeft = centreInWorldSpace * scale - new Vector2(screenRectangle.Size.X / 2, screenRectangle.Size.Y / 2);

            return aWorldPos*scale - topLeft ; 
        }

        public static bool MomAmIInFrame(Rectangle aRect)
        {
            if (screenRectangle.Intersects(aRect))
            {
                return true;
            }
            return false;
        }

        public static void DrawRenderTarget()
        {
            spriteBatch.Begin();
            spriteBatch.Draw(cameraTarget, renderTargetPosition, Color.White);
            spriteBatch.End();
        }

        static void Draw(SpriteBatch aBatch)
        {
            TileManager.Draw(aBatch);
            ObjectManager.Draw(aBatch);
            UIManager.DrawGameUI(aBatch); // a bit ugly but needs to do this since we want to draw the game in pause aswell.

        }

        public static void RunningDraw()
        {
            GraphicsManager.SetRenderTarget(cameraTarget);
            spriteBatch.Begin();
            GraphicsManager.ClearScreen(Color.White);

            Draw(spriteBatch);
            if (DebugManager.mode == DebugMode.On)
            {
                Vector2 DebugPos = new Vector2(screenRectangle.Size.X / 2, screenRectangle.Size.Y / 2);
                spriteBatch.Draw(debugTexture, DebugPos, new Rectangle(0, 0, 10, 10), Color.White, 0f, new Vector2(5), 1f, SpriteEffects.None, 1f);

                //Rectangle r = new Rectangle((centrePoint - screenRectangle.Location.ToVector2() - screenBorder.ToVector2() / 2).ToPoint() , screenRectangle.Size);
                //spriteBatch.Draw(debugTexture, r, new Rectangle(0, 0, 10, 10), Color.White);
            }

            spriteBatch.End();
            GraphicsManager.SetRenderTarget(null);

        }

        public static void PauseDraw()
        {
            GraphicsManager.SetRenderTarget(cameraTarget);
            spriteBatch.Begin();
            Draw(spriteBatch);
            GraphicsManager.ClearScreen(Color.Purple);

            pauseGfx.Draw(spriteBatch, Vector2.Zero);
            UIManager.Draw(spriteBatch);
            spriteBatch.End();

            GraphicsManager.SetRenderTarget(null);

        }

        public static void OptionDraw()
        {
            GraphicsManager.SetRenderTarget(cameraTarget);
            spriteBatch.Begin();

            UIManager.Draw(spriteBatch);

            spriteBatch.End();
            GraphicsManager.SetRenderTarget(null);
        }
    }
}
