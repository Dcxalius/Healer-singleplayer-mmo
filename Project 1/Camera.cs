using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Project_1.Content.Input;
using Project_1.GameObjects;
using Project_1.Managers;
using Project_1.Textures;
using Project_1.Tiles;
using Project_1.UI;
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
            Hardbound
        }

        public static float Scale
        {
             get { return scale; }
        }

        public static float Zoom
        {
            get { return 1f / scale; }
        }

        static SpriteBatch spriteBatch;

        static Vector2 centrePoint = new Vector2(100,100);
        static Texture2D debugTexture = GraphicsManager.GetTexture(new GfxPath(GfxType.Debug, "Debug"));

        static Textures.Texture pauseGfx = new Textures.Texture(new GfxPath(GfxType.UI, "PauseBackground"));

        public static Point screenBorder = new Point(1500, 900);
        public static Rectangle screenRectangle;

        static float scale = 1f;

        static float cameraMoveBorderSize = 0.1f;
        static Vector2 velocity = Vector2.Zero;
        static Vector2 momentum = Vector2.Zero;
        static int baseSpeed = 100;
        static Vector2 drag = new Vector2(0.9f, 0.9f);

        static MovingObject boundObject;
        static Rectangle bindingRectangle = new Rectangle(new Point(0), new Point(screenBorder.X/ 4 * 3, screenBorder.Y/4 * 3));
        static Point centrePointInScreen = new Point(screenBorder.X/2, screenBorder.Y/2);
        static CameraSettings cameraSettings = CameraSettings.RectangleSoftBound;
        static float maxCircleCameraMove = screenBorder.Y / 3;
        //static Rectangle maxRectangleCameraMove;

        public static void Init()
        {
            SetWindowSize(screenBorder);
            spriteBatch = GraphicsManager.CreateSpriteBatch();
        }

        public static void Update()
        {
            if(InputManager.ScrolledSinceLastFrame != 0)
            {
                //DebugManager.Print(typeof(Camera), "Scrolled: " + InputManager.ScrolledSinceLastFrame);
                scale -= (float)InputManager.ScrolledSinceLastFrame / 2400f; //A single mousewheel step is 120 so 2400 gives a movement of 5% points per mousewheel step
                //screenRectangle.Size = (screenBorder.ToVector2() * Zoom).ToPoint();
                //screenRectangle.Location = (centrePoint - screenRectangle.Size.ToVector2() / 2).ToPoint();
                //centrePoint = screenRectangle.Location.ToVector2() + screenRectangle.Size.ToVector2() / 2;
                DebugManager.Print(typeof(Camera), "Centre point: " + centrePoint);
            }

            Move();

            if (InputManager.ScrolledSinceLastFrame != 0)
            {
                //DebugManager.Print(typeof(Camera), "ScreenRectangle: " + screenRectangle);
            }
        }

        public static (Vector2 , Vector2) TransformDevSizeToRelativeVectors(Rectangle aRect)
        {
            Vector2 pos = aRect.Location.ToVector2() / screenBorder.ToVector2();
            Vector2 size = aRect.Size.ToVector2() / screenBorder.ToVector2();

            return (pos, size);
        }

        public static void BindCamera(MovingObject aBinder)
        {
            boundObject = aBinder;
        }

        public static void SetCamera(CameraSettings aCameraSettings)
        {
            cameraSettings = aCameraSettings;
        }

        static void SetWindowSize(Point aSize)
        {
            screenBorder = aSize;
            screenRectangle = new Rectangle(Point.Zero, aSize);
            //screenRectangle = new Rectangle(centrePoint.ToPoint() - new Point(screenBorder.X / 2, screenBorder.Y / 2), aSize);
            //zoom = something xd
            GraphicsManager.SetWindowSize(screenBorder); //Flip this? So GraphicsManager handles screensize and passes it to camera
        }

        static void ApplyMouseVelocity()
        {
            newApplyMouseVelocity();
            return;
            if (cameraSettings == CameraSettings.Hardbound)
            {
                return;
            }
            Vector2 mousePos = InputManager.GetMousePosRelative();
            if (mousePos.X < cameraMoveBorderSize)
            {
                float movementFactor = 1 - mousePos.X * 10;
                velocity.X -= (float)(baseSpeed * TimeManager.gt.ElapsedGameTime.TotalSeconds) * movementFactor;
            }
            if (mousePos.X > 1 - cameraMoveBorderSize)
            {
                float movementFactor = (mousePos.X - 1 + cameraMoveBorderSize) * 10;
                velocity.X += (float)(baseSpeed * TimeManager.gt.ElapsedGameTime.TotalSeconds) * movementFactor;
            }
            if (mousePos.Y < cameraMoveBorderSize)
            {
                float movementFactor = 1 - mousePos.Y * 10;
                velocity.Y -= (float)(baseSpeed * TimeManager.gt.ElapsedGameTime.TotalSeconds) * movementFactor;


            }
            if (mousePos.Y > 1 - cameraMoveBorderSize)
            {
                float movementFactor = (mousePos.Y - 1 + cameraMoveBorderSize) * 10;
                velocity.Y += (float)(baseSpeed * TimeManager.gt.ElapsedGameTime.TotalSeconds) * movementFactor;

            }
        }

        static void newApplyMouseVelocity()
        {
            if (cameraSettings == CameraSettings.Hardbound)
            {
                return;
            }

            Vector2 relativeMousePos = InputManager.GetMousePosRelative();
            float movementFactor = 0;

            if (relativeMousePos.X < cameraMoveBorderSize)
            {
                movementFactor = 1 - relativeMousePos.X * 10;
            }

            if (relativeMousePos.X > 1 - cameraMoveBorderSize)
            {
                movementFactor = (relativeMousePos.X - 1 + cameraMoveBorderSize) * 10;
            }

            if (relativeMousePos.Y < cameraMoveBorderSize)
            {

                float tempMovementFactor = 1 - relativeMousePos.Y * 10;
                if (tempMovementFactor > movementFactor)
                {
                    movementFactor = tempMovementFactor;
                }

            }

            if (relativeMousePos.Y > 1 - cameraMoveBorderSize)
            {
                float tempMovementFactor = (relativeMousePos.Y - 1 + cameraMoveBorderSize) * 10;

                if (tempMovementFactor > movementFactor)
                {
                    movementFactor = tempMovementFactor;
                }
            }

            if (movementFactor < 0)
            {
                return;
            }

            //DebugManager.Print(typeof(Camera), "Mouse pos = " + relativeMousePos);

            Vector2 absoluteMosPos = InputManager.GetMousePosAbsolute().ToVector2();

            Vector2 mouseAbsoluteToCentre = absoluteMosPos - centrePointInScreen.ToVector2();
            mouseAbsoluteToCentre.Normalize();
            velocity = mouseAbsoluteToCentre * (float)(baseSpeed * TimeManager.gt.ElapsedGameTime.TotalSeconds) * movementFactor;
            //DebugManager.Print(typeof(Camera), "Velocity = " + velocity.ToString());
        }

        static void Move()
        {
            switch (cameraSettings)
            {
                case CameraSettings.Free:
                    MoveFree();
                    break;
                case CameraSettings.CircleSoftBound:
                    MoveCircleSoftBound();
                    break;
                case CameraSettings.RectangleSoftBound:
                    MoveRectangleSoftBound();
                    break;
                case CameraSettings.Hardbound:
                    MoveHardBound();
                    break;
                default:
                    break;
            }

            CheckForSpacePress();

            //screenRectangle.Location = (centrePoint * scale).ToPoint() - new Point((int)(screenBorder.X / 2f), (int)(screenBorder.Y / 2f));

            
        }

        static void CheckForSpacePress()
        {
            if (InputManager.GetPress(Microsoft.Xna.Framework.Input.Keys.Space))
            {
                if (boundObject == null)
                {
                    cameraSettings = CameraSettings.Free;
                    return;
                }
                centrePoint = boundObject.Position;
            }
        }

        static void MoveRectangleSoftBound()
        {
            if (boundObject == null)
            {
                cameraSettings = CameraSettings.Free;
                return;
            }

            ApplyMouseVelocity();


            bindingRectangle.Location = (boundObject.Position - bindingRectangle.Size.ToVector2() / 2).ToPoint();

            if (!bindingRectangle.Contains(centrePoint))
            { 
                Vector2 cameraRectIntersection = CalculateIntersection();

                TeleportCamera(boundObject.Position - cameraRectIntersection);
            }

            ApplyMovementToCamera();
        }

        static Vector2 CalculateIntersection()
        {

            Vector2 playerStart = boundObject.Position;
            Vector2 playerToCameraRay = Vector2.Normalize(centrePoint - boundObject.Position);

            Vector2 rectangleCornerStart = GetClosestRectangleCorner(playerToCameraRay);
            (Vector2 rectangleSideRay, Vector2 rectangleFurtherSideRay) = GetRectangleRays(rectangleCornerStart);

            
            float u = (playerStart.Y * rectangleSideRay.X + rectangleSideRay.Y * rectangleCornerStart.X - rectangleCornerStart.Y * rectangleSideRay.X - rectangleSideRay.Y * playerStart.X) / (playerToCameraRay.X * rectangleSideRay.Y - playerToCameraRay.Y * rectangleSideRay.X);
            float furtherU = (playerStart.Y * rectangleFurtherSideRay.X + rectangleFurtherSideRay.Y * rectangleCornerStart.X - rectangleCornerStart.Y * rectangleFurtherSideRay.X - rectangleFurtherSideRay.Y * playerStart.X) / (playerToCameraRay.X * rectangleFurtherSideRay.Y - playerToCameraRay.Y * rectangleFurtherSideRay.X);

            Vector2 intersection = playerStart + playerToCameraRay * u;
            Vector2 furtherIntersection = playerStart + playerToCameraRay * furtherU;

            Vector2 playerToCorner = rectangleCornerStart - playerStart;
            Vector2 cameraToCorner = rectangleCornerStart - centrePoint;


            Vector2 distanceToBinder = boundObject.Position - intersection;
            float length = distanceToBinder.Length();

            Vector2 normalized = Vector2.Normalize(distanceToBinder);
            Vector2 tele = normalized * length * 0.9999f;

            playerToCorner.Normalize();
            cameraToCorner.Normalize();

            if (!bindingRectangle.Contains(boundObject.Position - tele))
            {
                distanceToBinder = boundObject.Position - furtherIntersection;
                length = distanceToBinder.Length();

                normalized = Vector2.Normalize(distanceToBinder);
                tele = normalized * length * 0.9999f;
                return tele;
            }



            if (Math.Abs(cameraToCorner.X) > Math.Abs(playerToCorner.X))
            {
                return tele;
            }
            if (Math.Abs(cameraToCorner.Y) > Math.Abs(playerToCorner.Y))
            {
                return tele;

                //return furtherIntersection;
            }

            DebugManager.Print(typeof(Camera), cameraToCorner.ToString());

            Debug.Assert(false);
            return Vector2.Zero;
        }

        static Vector2 GetClosestRectangleCorner(Vector2 aRay)
        {
            Vector2 returnable = Vector2.Zero;
            string msg = "Closest corner: ";

            if (aRay.Y > 0)
            {
                returnable.Y = bindingRectangle.Bottom;
                msg += "Bottom ";
            }
            else
            {
                returnable.Y = bindingRectangle.Top;
                msg += "Top ";

            }
            if (aRay.X > 0)
            {
                returnable.X = bindingRectangle.Right;
                msg += "Right ";
            }
            else
            {
                returnable.X = bindingRectangle.Left;
                msg += "Left ";

            }

            //DebugManager.Print(typeof(Camera), msg);

            return returnable;
        }

        static (Vector2, Vector2) GetRectangleRays(Vector2 aRectangleCorner)
        {
            Vector2 closestDirVector = centrePoint - aRectangleCorner;
            Vector2 otherDirVector = centrePoint - aRectangleCorner;
            string msg = "";
            if (Math.Abs(closestDirVector.X) > Math.Abs(closestDirVector.Y))
            {
                msg = "x > Y";
                closestDirVector.Y = 0;
                otherDirVector.X = 0;
            }
            else
            {
                msg = "y > x";
                closestDirVector.X = 0;
                otherDirVector.Y = 0;
            }

            closestDirVector.Normalize();
            otherDirVector.Normalize();


            //DebugManager.Print(typeof(Camera), msg);


            return (closestDirVector, otherDirVector);
        }

        static void MoveCircleSoftBound()
        {
            if (boundObject == null)
            {
                cameraSettings = CameraSettings.Free;
                return;
            }

            ApplyMouseVelocity();


            StayWithinCircleBind();


            ApplyMovementToCamera();
        }

        static void MoveHardBound()
        {
            if (boundObject == null)
            {
                cameraSettings = CameraSettings.Free;
                return;
            }

            centrePoint = boundObject.Position;
        }

        static void MoveFree()
        {
            ApplyMouseVelocity();
            ApplyMovementToCamera();
        }

        static void StayWithinCircleBind()
        {
            Vector2 distanceToBinder = boundObject.Position - centrePoint;
            if (distanceToBinder.Length() >= maxCircleCameraMove)
            {
                Vector2 normalized = Vector2.Normalize(distanceToBinder);
                Vector2 tele = normalized * maxCircleCameraMove * 0.9999f;
                TeleportCamera(boundObject.Position - tele);
                //velocity = Vector2.Zero;
            }
        }

        static void TeleportCamera(Vector2 aPos)
        {
            centrePoint  = aPos;
        }

        static void ApplyMovementToCamera()
        {
            momentum += velocity;

            velocity = Vector2.Zero;
            centrePoint += momentum;
            momentum = new Vector2(momentum.X * drag.X, momentum.Y * drag.Y);
        }

        public static Vector2 WorldPosToCameraSpace(Vector2 aWorldPos)
        {
            Vector2 topLeft = centrePoint * scale - new Vector2(screenBorder.X / 2, screenBorder.Y / 2);

            return aWorldPos*scale - topLeft ; 
        }

        public static bool MomAmIInFrame(Rectangle aRect)
        {
            if (screenRectangle.Intersects(aRect))
            {
                return true;
            }

            //if (screenRectangle.Contains(aRect))
            //{
            //    return true;
            //}

            return false;
        }

        static void Draw(SpriteBatch aBatch)
        {
            TileManager.Draw(aBatch);
            ObjectManager.Draw(aBatch);
            UIManager.DrawGameUI(aBatch);

        }

        public static void RunningDraw()
        {
            spriteBatch.Begin();

            Draw(spriteBatch);
            if (DebugManager.mode == DebugMode.On)
            {
                Vector2 DebugPos = new Vector2(screenBorder.X / 2, screenBorder.Y / 2);
                spriteBatch.Draw(debugTexture, DebugPos, new Rectangle(0, 0, 10, 10), Color.White, 0f, new Vector2(5), 1f, SpriteEffects.None, 1f);

                //Rectangle r = new Rectangle((centrePoint - screenRectangle.Location.ToVector2() - screenBorder.ToVector2() / 2).ToPoint() , screenRectangle.Size);
                //spriteBatch.Draw(debugTexture, r, new Rectangle(0, 0, 10, 10), Color.White);
            }

            spriteBatch.End();
        }

        public static void PauseDraw()
        {
            spriteBatch.Begin();
            Draw(spriteBatch);
            
            pauseGfx.Draw(spriteBatch, Vector2.Zero);
            UIManager.Draw(spriteBatch);
            spriteBatch.End();


        }

        public static void OptionDraw()
        {
            spriteBatch.Begin();

            UIManager.Draw(spriteBatch);

            spriteBatch.End();
        }
    }
}
