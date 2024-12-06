using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Project_1.GameObjects;
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

        public static Rectangle RenderTargetPosition { set => renderTargetPosition = value; }


        static WorldSpace centreInWorldSpace = new WorldSpace(100, 100);

        //---

        //--- debugText is fine but should pauseGfx be somewhere else?
        static Texture2D debugTexture = TextureManager.GetTexture(new GfxPath(GfxType.Debug, "Debug"));
        static Textures.Texture pauseGfx = new Textures.Texture(new GfxPath(GfxType.UI, "PauseBackground"));

        //--

        public readonly static Point devScreenBorder = new Point(1500, 900);
        static AbsoluteScreenPosition screenRectangleSize;



        static float scale = 1f;
        static float minScale = 0.7f;
        static float maxScale = 1.4f;

        //--
        //static Rectangle maxRectangleCameraMove;
        static CameraSettings cameraSettings = CameraSettings.RectangleSoftBound;

        //--

        static RenderTarget2D cameraTarget;
        static SpriteBatch spriteBatch;
        static Rectangle renderTargetPosition;

        static RenderTarget2D gameTarget;
        static SpriteBatch gameSpriteBatch;

        static RenderTarget2D uiTarget;
        static SpriteBatch uiSpriteBatch;

        static CameraMover cameraMover = new CameraMover();


        static RasterizerState rasterizerState = new RasterizerState() { ScissorTestEnable = true };


        public static void Init()
        {
            spriteBatch = GraphicsManager.CreateSpriteBatch();
            gameSpriteBatch = GraphicsManager.CreateSpriteBatch();
            uiSpriteBatch = GraphicsManager.CreateSpriteBatch();

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
                if (scrolled > 0 && scale <= minScale || scrolled < 0 && scale >= maxScale)
                {
                    return;
                }
                scale -= scrolled / 2400f; //A single mousewheel step is 120 so 2400 gives a movement of 5% points per mousewheel step
                //DebugManager.Print(typeof(Camera), "Centre point: " + centreInWorldSpace + ", " + CentrePointInScreenSpace);
                //DebugManager.Print(typeof(Camera), "WorldRect size: " + WorldRectangle.Size);
                cameraMover.bindingRectangle.Size = new Point((int)(screenRectangleSize.X / 4 * 3 * Zoom), (int)(screenRectangleSize.Y / 4 * 3 * Zoom));

            }
        }

        public static RelativeScreenPosition GetRelativeXSquare(float aSizeInX)
        {
            float a = screenRectangleSize.X * aSizeInX;
            float b = a / screenRectangleSize.Y;
            return new(aSizeInX, b);
        }

        public static AbsoluteScreenPosition TransformRelativeToAbsoluteScreenSpace(Vector2 aPos) //TODO: Change this to Relative and absolute and move it out of here
        {
            AbsoluteScreenPosition pos = new AbsoluteScreenPosition((int)(screenRectangleSize.X * aPos.X), (int)(screenRectangleSize.Y * aPos.Y));
            //DebugManager.Print(typeof(Camera), "Abs pos = " + pos + ", and relative pos = " + aPos);
            return pos;
        }
        public static Vector2 TransformAbsoluteToRelativeScreenSpace(Point aPos) //TODO: Change this to Relative and absolute and move it out of here
        {
            Vector2 pos = new Vector2(aPos.X / (float)screenRectangleSize.X, aPos.Y / (float)screenRectangleSize.Y);
            return pos;
        }


        public static void SetCamera(CameraSettings aCameraSettings)
        {
            CameraStyleSelect.instance.SetValueFromOutside((int)aCameraSettings);
            cameraSettings = aCameraSettings;
        }

        public static void SetWindowSize(AbsoluteScreenPosition aSize)
        {
            cameraTarget = GraphicsManager.CreateRenderTarget(aSize.ToPoint());
            uiTarget = GraphicsManager.CreateRenderTarget(aSize.ToPoint());
            gameTarget = GraphicsManager.CreateRenderTarget(aSize.ToPoint());
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


        public static Rectangle WorldPosToCameraSpace(Rectangle aWorldPos)
        {
            Point topLeft = (centreInWorldSpace * scale - screenRectangleSize.ToVector2() / 2).ToPoint();
            Rectangle cameraPos = new Rectangle((aWorldPos.Location.ToVector2() * scale).ToPoint() - topLeft, (aWorldPos.Size.ToVector2() * scale).ToPoint());
            return cameraPos;
        }

        public static Vector2 WorldPosToCameraSpace(Vector2 aWorldPos)
        {
            Vector2 topLeft = centreInWorldSpace * scale - new Vector2(screenRectangleSize.X / 2, screenRectangleSize.Y / 2);

            return aWorldPos * scale - topLeft;
        }

        public static Vector2 ScreenSpaceToWorldSpace(AbsoluteScreenPosition aScreenPos)
        {
            Vector2 vectorInScreen = (CentrePointInScreenSpace - aScreenPos).ToVector2();

            Vector2 vectorInWorld = centreInWorldSpace - vectorInScreen * Zoom;

            return vectorInWorld;
        }

        public static Vector2 ScreenSpaceToWorldSpace(Vector2 aRelativeVector)
        {
            Vector2 vectorInScreen = CentrePointInScreenSpace.ToVector2() - TransformRelativeToAbsoluteScreenSpace(aRelativeVector).ToVector2();
            Vector2 vectorInWorld = WorldRectangle.Center.ToVector2() - vectorInScreen * Zoom;
            return vectorInWorld;
        }

        public static bool MomAmIInFrame(Rectangle aRect)
        {
            return ScreenRectangle.Intersects(aRect);
        }

        public static bool MomAmIInFrame(Vector2 aWorldPos)
        {
            return WorldRectangle.Contains(aWorldPos);
        }

        public static void DrawRenderTarget()
        {

            spriteBatch.Begin();
            spriteBatch.Draw(cameraTarget, renderTargetPosition, Color.White);
            spriteBatch.End();
        }

        static void DrawGameObjects(SpriteBatch aBatch)
        {
            TileManager.Draw(aBatch);
            ObjectManager.Draw(aBatch);
            ParticleManager.Draw(aBatch);
        }

        public static void DrawGameToCamera()
        {
            GraphicsManager.SetRenderTarget(cameraTarget);
            spriteBatch.Begin();
            spriteBatch.Draw(gameTarget, Vector2.Zero, Color.White);
            spriteBatch.End();
            GraphicsManager.SetRenderTarget(null);
        }

        public static void GameDraw()
        {
            UIDraw();
            GraphicsManager.SetRenderTarget(gameTarget);
            gameSpriteBatch.Begin(SpriteSortMode.FrontToBack);
            //spriteBatch.Begin();
            GraphicsManager.ClearScreen(Color.White);


            DrawGameObjects(gameSpriteBatch);
            gameSpriteBatch.Draw(uiTarget, Vector2.Zero, null, Color.White, 0f, Vector2.Zero, 1f, SpriteEffects.None, 1f);



            gameSpriteBatch.End();
            GraphicsManager.SetRenderTarget(null);

        }

        public static void PauseDraw()
        {

            GraphicsManager.SetRenderTarget(cameraTarget);
            spriteBatch.Begin();
            GraphicsManager.ClearScreen(Color.Purple);


            DrawGameObjects(spriteBatch); //draw game
            spriteBatch.Draw(uiTarget, Vector2.Zero, null, Color.White, 0f, Vector2.Zero, 1f, SpriteEffects.None, 1f); //draw game ui
            pauseGfx.Draw(spriteBatch, Vector2.Zero); //draw gray screen overlay
            UIManager.Draw(spriteBatch); //draw pause menu



            spriteBatch.End();

            GraphicsManager.SetRenderTarget(null);

        }

        static void UIDraw()
        {
            GraphicsManager.SetRenderTarget(uiTarget);
            uiSpriteBatch.Begin(SpriteSortMode.Immediate, null, null, null, rasterizerState);
            GraphicsManager.ClearScreen(Color.Transparent);
            HUDManager.Draw(uiSpriteBatch);

            uiSpriteBatch.End();

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
