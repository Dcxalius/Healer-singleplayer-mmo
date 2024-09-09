using Microsoft.Xna.Framework;
using Project_1.Input;
using Project_1.Managers;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static Project_1.Camera;

namespace Project_1
{
    internal class CameraMover
    {


        float cameraMoveBorderSize = 0.1f;
        Vector2 velocity = Vector2.Zero;
        Vector2 momentum = Vector2.Zero;
        int baseSpeed = 100;
        Vector2 drag = new Vector2(0.9f, 0.9f);



        MovingObject boundObject;
        public Rectangle bindingRectangle = new Rectangle(new Point(0), new Point(devScreenBorder.X / 4 * 3, devScreenBorder.Y / 4 * 3));
        public float maxCircleCameraMove = devScreenBorder.Y / 3;


        void ApplyMouseVelocity()
        {
            if (CurrentCameraSetting == CameraSettings.Hardbound)
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

            Vector2 mouseAbsoluteToCentre = absoluteMosPos - CentrePointInScreenSpace.ToVector2();
            mouseAbsoluteToCentre.Normalize();
            velocity = mouseAbsoluteToCentre * (float)(baseSpeed * TimeManager.gt.ElapsedGameTime.TotalSeconds) * movementFactor;
            //DebugManager.Print(typeof(Camera), "Velocity = " + velocity.ToString());
        }

        public void Move()
        {
            switch (CurrentCameraSetting)
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
        }
        public void BindCamera(MovingObject aBinder)
        {
            boundObject = aBinder;
        }


        void CheckForSpacePress()
        {
            if (InputManager.GetPress(Microsoft.Xna.Framework.Input.Keys.Space))
            {
                if (boundObject == null)
                {
                    SetCamera(Camera.CameraSettings.Free);
                    return;
                }
                CentreInWorldSpace = boundObject.Position;
            }
        }

        void MoveRectangleSoftBound()
        {
            if (boundObject == null)
            {
                SetCamera(Camera.CameraSettings.Free);

                return;
            }

            ApplyMouseVelocity();

            CheckIfCameraTriesToLeavePlayer();



            ApplyMovementToCamera();
        }

        void CheckIfCameraTriesToLeavePlayer()
        {
            bindingRectangle.Location = (boundObject.Position - bindingRectangle.Size.ToVector2() / 2).ToPoint();

            if (!bindingRectangle.Contains(CentreInWorldSpace))
            {
                Vector2 cameraRectIntersection = CalculateIntersection();

                CentreInWorldSpace = boundObject.Position - cameraRectIntersection;
            }
        }

        Vector2 CalculateIntersection() //TODO: split this function more
        {

            Vector2 playerStart = boundObject.Position;
            Vector2 playerToCameraRay = Vector2.Normalize(CentreInWorldSpace - boundObject.Position);

            Vector2 rectangleCornerStart = GetClosestRectangleCorner(playerToCameraRay);
            (Vector2 rectangleSideRay, Vector2 rectangleFurtherSideRay) = GetRectangleRays(rectangleCornerStart);


            float u = LengthToCollisionFromFirstVector(playerStart, playerToCameraRay, rectangleCornerStart, rectangleSideRay);

            Vector2 intersection = playerStart + playerToCameraRay * u;

            Vector2 distanceToBinder = boundObject.Position - intersection;
            float length = distanceToBinder.Length();

            Vector2 normalized = Vector2.Normalize(distanceToBinder);
            Vector2 tele = normalized * length * 0.9999f;

            if (!bindingRectangle.Contains(boundObject.Position - tele))
            {

                float furtherU = LengthToCollisionFromFirstVector(playerStart, playerToCameraRay, rectangleCornerStart, rectangleFurtherSideRay);
                Vector2 furtherIntersection = playerStart + playerToCameraRay * furtherU;
                distanceToBinder = boundObject.Position - furtherIntersection;
                length = distanceToBinder.Length();

                normalized = Vector2.Normalize(distanceToBinder);
                tele = normalized * length * 0.9999f;
                return tele;
            }

            Vector2 playerToCorner = rectangleCornerStart - playerStart;
            Vector2 cameraToCorner = rectangleCornerStart - CentreInWorldSpace;

            playerToCorner.Normalize();
            cameraToCorner.Normalize();

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

        float LengthToCollisionFromFirstVector(Vector2 aAStart, Vector2 aADir, Vector2 aBStart, Vector2 aBDir)
        {
            float length = (aAStart.Y * aBDir.X + aBDir.Y * aBStart.X - aBStart.Y * aBDir.X - aBDir.Y * aAStart.X) / (aADir.X * aBDir.Y - aADir.Y * aBDir.X);

            return length;
        }

        Vector2 GetClosestRectangleCorner(Vector2 aRay)
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

        (Vector2, Vector2) GetRectangleRays(Vector2 aRectangleCorner)
        {
            Vector2 closestDirVector = CentreInWorldSpace - aRectangleCorner;
            Vector2 otherDirVector = CentreInWorldSpace - aRectangleCorner;
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

        void MoveCircleSoftBound()
        {
            if (boundObject == null)
            {
                SetCamera(CameraSettings.Free);
                return;
            }

            ApplyMouseVelocity();


            StayWithinCircleBind();


            ApplyMovementToCamera();
        }

        void MoveHardBound()
        {
            if (boundObject == null)
            {
                SetCamera(CameraSettings.Free);
                return;
            }

            CentreInWorldSpace = boundObject.Position;
        }

        void MoveFree()
        {
            ApplyMouseVelocity();
            ApplyMovementToCamera();
        }

        void StayWithinCircleBind()
        {
            Vector2 distanceToBinder = boundObject.Position - CentreInWorldSpace;
            if (distanceToBinder.Length() >= maxCircleCameraMove)
            {
                Vector2 normalized = Vector2.Normalize(distanceToBinder);
                Vector2 tele = normalized * maxCircleCameraMove * 0.9999f;
                CentreInWorldSpace = boundObject.Position - tele;
                //velocity = Vector2.Zero;
            }
        }

        void ApplyMovementToCamera()
        {
            momentum += velocity;

            velocity = Vector2.Zero;
            CentreInWorldSpace += momentum;
            momentum = new Vector2(momentum.X * drag.X, momentum.Y * drag.Y);
        }

    }
}
