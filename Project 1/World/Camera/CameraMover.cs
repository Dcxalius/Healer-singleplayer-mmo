using Microsoft.Xna.Framework;
using Project_1.Input;
using Project_1.Managers;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static Project_1.Camera.Camera;
using Project_1.GameObjects;
using Project_1.Tiles;
using Project_1.Rendering;

namespace Project_1.Camera
{
    internal class CameraMover
    {
        const float CameraRotationSpeedRadians = MathHelper.PiOver2;
        public WorldSpace CentreInWorldSpace { get => centreInWorldSpace; set => centreInWorldSpace = value; }
        WorldSpace centreInWorldSpace = new WorldSpace(100, 100);


        float cameraMoveBorderSize = 0.1f;
        WorldSpace velocity = WorldSpace.Zero;
        WorldSpace momentum = WorldSpace.Zero;
        int baseSpeed = 100;
        readonly Vector2 drag;

        MovingObject boundObject;
        public Rectangle bindingRectangle;
        public float maxCircleCameraMove;

        public CameraMover()
        {
            drag = new Vector2(0.9f, 0.9f);
            bindingRectangle = new Rectangle(new Point(0), new Point(WindowSize.X / 4 * 3, WindowSize.Y / 4 * 3));
            maxCircleCameraMove = WindowSize.Y / 3;
        }


        void ApplyMouseVelocity()
        {
            if (CurrentCameraSetting == CameraSettings.Follow.Hardbound)
            {
                return;
            }

            RelativeScreenPosition relativeMousePos = MouseStateCache.Relative;
            float relX = relativeMousePos.X;
            float relY = relativeMousePos.Y;
            if (float.IsNaN(relX) || float.IsNaN(relY) || float.IsInfinity(relX) || float.IsInfinity(relY))
            {
                return;
            }
            relX = Math.Clamp(relX, 0f, 1f);
            relY = Math.Clamp(relY, 0f, 1f);
            relativeMousePos = new RelativeScreenPosition(relX, relY);
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

            movementFactor = Math.Clamp(movementFactor, 0f, 1f);
            if (movementFactor <= 0f)
            {
                return;
            }

            //DebugManager.Print("Mouse pos = " + relativeMousePos);

            AbsoluteScreenPosition absoluteMosPos = MouseStateCache.Absolute;
            if (absoluteMosPos.X == int.MinValue || absoluteMosPos.Y == int.MinValue)
            {
                return;
            }

            Vector2 mouseAbsoluteToCentre = (absoluteMosPos - CentrePointInScreenSpace).ToVector2();
            if (mouseAbsoluteToCentre.LengthSquared() <= float.Epsilon)
            {
                velocity = WorldSpace.Zero;
                return;
            }
            mouseAbsoluteToCentre.Normalize();
            Vector2 worldDirection = ResolveMouseMoveDirection(mouseAbsoluteToCentre);
            velocity = new WorldSpace(worldDirection * (float)(baseSpeed * TimeManager.SecondsSinceLastFrame) * movementFactor);
            //DebugManager.Print("Velocity = " + velocity.ToString());
        }

        Vector2 ResolveMouseMoveDirection(Vector2 aNormalizedMouseDirection)
        {
            if (!DebugManager.Mode(DebugMode.ModelPreview))
            {
                return aNormalizedMouseDirection;
            }

            Vector2 right = WorldBlockRenderer.CameraGroundRight;
            Vector2 forward = WorldBlockRenderer.CameraGroundForward;
            Vector2 worldDirection = right * aNormalizedMouseDirection.X - forward * aNormalizedMouseDirection.Y;
            if (worldDirection.LengthSquared() <= float.Epsilon)
            {
                return Vector2.Zero;
            }

            worldDirection.Normalize();
            return worldDirection;
        }

        public void Move()
        {
            ThreadAffinity.AssertSimThread();
            ApplyCameraRotationInput();
            switch (CurrentCameraSetting)
            {
                case CameraSettings.Follow.Free:
                    MoveFree();
                    break;
                case CameraSettings.Follow.CircleSoftBound:
                    MoveCircleSoftBound();
                    //if (boundObject.FeetPosition.DistanceTo(centreInWorldSpace) > maxCircleCameraMove)
                    //{
                    //    DebugManager.Print("How did we get here?");
                    //}
                    break;
                case CameraSettings.Follow.RectangleSoftBound:
                    MoveRectangleSoftBound();
                    break;
                case CameraSettings.Follow.Hardbound:
                    MoveHardBound();
                    break;
                default:
                    break;
            }

            CheckForSpacePress();
        }
        public void BindCamera(MovingObject aBinder)
        {
            ThreadAffinity.AssertSimThread();
            boundObject = aBinder;
        }


        void CheckForSpacePress()
        {
            if (KeyBindStateCache.GetPress(KeyBindManager.KeyListner.CenterCamera))
            {
                if (boundObject == null)
                {
                    CurrentCameraSetting = (CameraSettings.Follow.Free);
                    return;
                }
                CentreInWorldSpace = boundObject.FeetPosition;
                velocity = WorldSpace.Zero;
                momentum = WorldSpace.Zero;
            }
        }

        void MoveRectangleSoftBound()
        {
            if (boundObject == null)
            {
                CurrentCameraSetting = (CameraSettings.Follow.Free);

                return;
            }

            ApplyMouseVelocity();

            CheckIfCameraTriesToLeavePlayer();



            ApplyMovementToCamera();
        }

        void CheckIfCameraTriesToLeavePlayer()
        {
            if (DebugManager.Mode(DebugMode.ModelPreview))
            {
                CheckIfCameraTriesToLeavePlayerInCameraSpace();
                return;
            }

            bindingRectangle.Location = (boundObject.FeetPosition - bindingRectangle.Size.ToVector2() / 2).ToPoint();

            if (!bindingRectangle.Contains(CentreInWorldSpace))
            {
                Vector2 cameraRectIntersection = CalculateIntersection();

                CentreInWorldSpace = boundObject.FeetPosition - (WorldSpace)cameraRectIntersection;
            }
        }

        void CheckIfCameraTriesToLeavePlayerInCameraSpace()
        {
            Rectangle screenBindingRectangle = BuildScreenBindingRectangle();
            WorldSpace3D playerWorldPosition = ResolveBoundObjectWorldPosition();

            for (int i = 0; i < 3; i++)
            {
                Camera3D previewCamera = WorldBlockRenderer.CreatePreviewCamera(CentreInWorldSpace);
                AbsoluteScreenPosition playerScreenPosition = previewCamera.WorldToScreen(playerWorldPosition);
                Point clampedPoint = new Point(
                    Math.Clamp(playerScreenPosition.X, screenBindingRectangle.Left, screenBindingRectangle.Right),
                    Math.Clamp(playerScreenPosition.Y, screenBindingRectangle.Top, screenBindingRectangle.Bottom));
                if (clampedPoint.X == playerScreenPosition.X && clampedPoint.Y == playerScreenPosition.Y)
                {
                    return;
                }

                Vector2 desiredScreenDelta = new Vector2(
                    clampedPoint.X - playerScreenPosition.X,
                    clampedPoint.Y - playerScreenPosition.Y);
                if (!TryResolveCameraCorrection(previewCamera, playerWorldPosition, desiredScreenDelta, out Vector2 worldCorrection))
                {
                    return;
                }

                CentreInWorldSpace = new WorldSpace(CentreInWorldSpace.ToVector2() + worldCorrection);
            }
        }

        void ApplyCameraRotationInput()
        {
            float rotationDelta = 0f;
            if (KeyBindStateCache.GetHold(KeyBindManager.KeyListner.RotateCameraLeft))
            {
                rotationDelta -= CameraRotationSpeedRadians * (float)TimeManager.SecondsSinceLastFrame;
            }
            if (KeyBindStateCache.GetHold(KeyBindManager.KeyListner.RotateCameraRight))
            {
                rotationDelta += CameraRotationSpeedRadians * (float)TimeManager.SecondsSinceLastFrame;
            }
            if (Math.Abs(rotationDelta) <= float.Epsilon)
            {
                return;
            }

            WorldBlockRenderer.RotateCameraYaw(rotationDelta);
            ReadjustBindingAfterRotation();
        }

        void ReadjustBindingAfterRotation()
        {
            if (boundObject == null) return;
            if (CurrentCameraSetting != CameraSettings.Follow.RectangleSoftBound) return;

            CheckIfCameraTriesToLeavePlayerInCameraSpace();
            velocity = WorldSpace.Zero;
            momentum = WorldSpace.Zero;
        }

        Rectangle BuildScreenBindingRectangle()
        {
            AbsoluteScreenPosition screenCentre = CentrePointInScreenSpace;
            Point halfSize = new Point(bindingRectangle.Width / 2, bindingRectangle.Height / 2);
            return new Rectangle(
                new Point(screenCentre.X - halfSize.X, screenCentre.Y - halfSize.Y),
                bindingRectangle.Size);
        }

        bool TryResolveCameraCorrection(Camera3D aPreviewCamera, WorldSpace3D aPlayerWorldPosition, Vector2 aDesiredScreenDelta, out Vector2 aWorldCorrection)
        {
            const float sampleDistanceInTiles = 1f;
            Vector2 rightPixels = new Vector2(WorldBlockRenderer.CameraGroundRight.X * Tile.Size.X, WorldBlockRenderer.CameraGroundRight.Y * Tile.Size.Y) * sampleDistanceInTiles;
            Vector2 forwardPixels = new Vector2(WorldBlockRenderer.CameraGroundForward.X * Tile.Size.X, WorldBlockRenderer.CameraGroundForward.Y * Tile.Size.Y) * sampleDistanceInTiles;

            Vector2 screenDeltaPerRight = SampleScreenDelta(aPlayerWorldPosition, rightPixels, aPreviewCamera);
            Vector2 screenDeltaPerForward = SampleScreenDelta(aPlayerWorldPosition, forwardPixels, aPreviewCamera);

            float determinant = screenDeltaPerRight.X * screenDeltaPerForward.Y - screenDeltaPerRight.Y * screenDeltaPerForward.X;
            if (Math.Abs(determinant) <= 0.0001f)
            {
                aWorldCorrection = Vector2.Zero;
                return false;
            }

            float rightScale = (aDesiredScreenDelta.X * screenDeltaPerForward.Y - aDesiredScreenDelta.Y * screenDeltaPerForward.X) / determinant;
            float forwardScale = (screenDeltaPerRight.X * aDesiredScreenDelta.Y - screenDeltaPerRight.Y * aDesiredScreenDelta.X) / determinant;
            aWorldCorrection = rightPixels * rightScale + forwardPixels * forwardScale;
            return !float.IsNaN(aWorldCorrection.X) && !float.IsNaN(aWorldCorrection.Y) && !float.IsInfinity(aWorldCorrection.X) && !float.IsInfinity(aWorldCorrection.Y);
        }

        Vector2 SampleScreenDelta(WorldSpace3D aPlayerWorldPosition, Vector2 aCameraOffsetPixels, Camera3D aCurrentCamera)
        {
            AbsoluteScreenPosition currentScreen = aCurrentCamera.WorldToScreen(aPlayerWorldPosition);
            WorldSpace shiftedCentre = new WorldSpace(CentreInWorldSpace.ToVector2() + aCameraOffsetPixels);
            Camera3D shiftedCamera = WorldBlockRenderer.CreatePreviewCamera(shiftedCentre);
            AbsoluteScreenPosition shiftedScreen = shiftedCamera.WorldToScreen(aPlayerWorldPosition);
            return new Vector2(shiftedScreen.X - currentScreen.X, shiftedScreen.Y - currentScreen.Y);
        }

        WorldSpace3D ResolveBoundObjectWorldPosition()
        {
            WorldSpace feetPosition = boundObject?.FeetPosition ?? WorldSpace.Zero;
            return new WorldSpace3D(
                feetPosition.X / Tile.Size.X,
                ResolveSurfaceHeight(feetPosition),
                feetPosition.Y / Tile.Size.Y);
        }

        float ResolveSurfaceHeight(WorldSpace aFeetPosition)
        {
            Chunk chunk = TileManager.GetChunk(aFeetPosition);
            if (chunk == null) return 0f;

            Point gridPosition = TileManager.GetGridPos(aFeetPosition);
            int localX = PositiveModulo(gridPosition.X, Chunk.ChunkSize.X);
            int localY = PositiveModulo(gridPosition.Y, Chunk.ChunkSize.Y);

            for (int z = Chunk.ChunkHeight - 1; z >= 0; z--)
            {
                if (chunk.GetBlock(localX, localY, z) != null)
                {
                    return z + 1f;
                }
            }

            return 0f;
        }

        int PositiveModulo(int aValue, int aDivisor)
        {
            int result = aValue % aDivisor;
            return result < 0 ? result + aDivisor : result;
        }

        Vector2 CalculateIntersection() //TODO: split this function more
        {
            Vector2 playerStart = boundObject.FeetPosition;
            Vector2 delta = CentreInWorldSpace - playerStart;
            if (delta.LengthSquared() <= float.Epsilon)
            {
                return Vector2.Zero;
            }

            Vector2 halfSize = bindingRectangle.Size.ToVector2() / 2f;
            float tX = Math.Abs(delta.X) > float.Epsilon ? halfSize.X / Math.Abs(delta.X) : float.PositiveInfinity;
            float tY = Math.Abs(delta.Y) > float.Epsilon ? halfSize.Y / Math.Abs(delta.Y) : float.PositiveInfinity;
            float t = Math.Min(tX, tY);

            Vector2 intersectionOffset = delta * t;
            return -intersectionOffset * 0.9999f;
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

            //DebugManager.Print(msg);

            return returnable;
        }

        (Vector2, Vector2) GetRectangleRays(Vector2 aRectangleCorner)
        {
            Vector2 closestDirVector = CentreInWorldSpace - aRectangleCorner;
            Vector2 otherDirVector = CentreInWorldSpace - aRectangleCorner;
            //string msg = "";
            if (Math.Abs(closestDirVector.X) > Math.Abs(closestDirVector.Y))
            {
                //msg = "x > Y";
                closestDirVector.Y = 0;
                otherDirVector.X = 0;
            }
            else
            {
                //msg = "y > x";
                closestDirVector.X = 0;
                otherDirVector.Y = 0;
            }

            closestDirVector.Normalize();
            otherDirVector.Normalize();


            //DebugManager.Print(msg);


            return (closestDirVector, otherDirVector);
        }

        void MoveCircleSoftBound()
        {
            if (boundObject == null)
            {
                CurrentCameraSetting = (CameraSettings.Follow.Free);
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
                CurrentCameraSetting = (CameraSettings.Follow.Free);
                return;
            }

            CentreInWorldSpace = boundObject.FeetPosition;
        }

        void MoveFree()
        {
            ApplyMouseVelocity();
            ApplyMovementToCamera();
        }

        void StayWithinCircleBind()
        {
            Vector2 distanceToBinder = boundObject.FeetPosition - CentreInWorldSpace;
            if (distanceToBinder.Length() >= maxCircleCameraMove)
            {
                Vector2 normalized = Vector2.Normalize(distanceToBinder);
                Vector2 tele = normalized * maxCircleCameraMove * 0.9999f;
                CentreInWorldSpace = (WorldSpace)(boundObject.FeetPosition - tele);
                //velocity = Vector2.Zero;
            }
        }

        void ApplyMovementToCamera()
        {
            if (float.IsNaN(velocity.X) || float.IsNaN(velocity.Y) || float.IsInfinity(velocity.X) || float.IsInfinity(velocity.Y))
            {
                Debug.Assert(false);
                velocity = WorldSpace.Zero;
                momentum = WorldSpace.Zero;
                return;
            }
            if (float.IsNaN(momentum.X) || float.IsNaN(momentum.Y) || float.IsInfinity(momentum.X) || float.IsInfinity(momentum.Y))
            {
                Debug.Assert(false);
                momentum = WorldSpace.Zero;
                velocity = WorldSpace.Zero;
                return;
            }
            momentum += velocity;

            velocity = WorldSpace.Zero;
            CentreInWorldSpace += momentum;
            momentum = new WorldSpace(momentum.X * drag.X, momentum.Y * drag.Y);
        }

    }
}
