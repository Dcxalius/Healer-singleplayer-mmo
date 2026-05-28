using Microsoft.Xna.Framework;
using Project_1.Managers;
using Project_1.System.Models.BaseModels;
using Project_1.Textures;
using Project_1.Tiles;
using System;

namespace Project_1.GameObjects.Entities
{
    internal partial class Entity
    {
        internal virtual GfxPath PresentationTexturePath => UnitData.GfxPath;
        internal virtual bool FaceCameraInPreview => false;
        internal virtual bool FrameFacesCameraInPreview => false;
        internal virtual bool PresentationFacesCameraInPreview => false;
        protected virtual bool TryResolvePreviewFacingYawRadians(out float aFacingYawRadians)
        {
            aFacingYawRadians = 0f;
            return false;
        }
        protected virtual int ResolvePreviewPresentationDirectionIndex() => 0;

        float modelFacingYawRadians = MathHelper.PiOver2;
        bool modelFacingYawInitialized;

        internal EntityModelRenderSnapshot BuildModelRenderSnapshot()
        {
            ThreadAffinity.AssertSimThread();
            EntityModelDefinition definition = EntityModelCatalog.Resolve(this);
            float baseFacingYaw = ResolveModelFacingYawRadians();

            return new EntityModelRenderSnapshot(
                RenderId,
                WorldBlockRenderer.ResolvePreviewWorldPosition(FeetPosition),
                definition.Key,
                definition.ShellType,
                definition.PresentationFrameSizePixels,
                PresentationTexturePath?.Type ?? GfxType.Debug,
                PresentationTexturePath?.Name,
                baseFacingYaw,
                baseFacingYaw,
                baseFacingYaw,
                FrameFacesCameraInPreview,
                PresentationFacesCameraInPreview,
                ResolvePreviewPresentationDirectionIndex(),
                VisualOpacity);
        }

        float ResolveModelFacingYawRadians()
        {
            //Q: Why do we have one for returning in Radians and one for a vector?
            //If they both should remain their calling feels like is should be linked
            if (TryResolvePreviewFacingYawRadians(out float previewFacingYawRadians))
            {
                modelFacingYawRadians = previewFacingYawRadians;
                modelFacingYawInitialized = true;
                return modelFacingYawRadians;
            }

            Vector2 facingDirection = ResolveModelFacingDirection();
            if (facingDirection.LengthSquared() > 0.0001f)
            {
                facingDirection.Normalize();
                modelFacingYawRadians = MathF.Atan2(facingDirection.X, facingDirection.Y);
                modelFacingYawInitialized = true;
            }
            else if (!modelFacingYawInitialized)
            {
                modelFacingYawRadians = HorizontalFacing == MovingObject.FacingDirection.Left
                    ? -MathHelper.PiOver2
                    : MathHelper.PiOver2;
                modelFacingYawInitialized = true;
            }

            return modelFacingYawRadians;
        }

        Vector2 ResolveModelFacingDirection()
        {
            //TODO: How do these Resolve the model direction? Seem to just be generic Entity direction rather than its model
            Vector2 momentumDirection = Momentum.ToVector2();
            if (momentumDirection.LengthSquared() > 0.0001f)
            {
                return momentumDirection;
            }

            if (Destination != null)
            {
                Vector2 destinationDirection = Destination.DirectionToWalk.ToVector2();
                if (destinationDirection.LengthSquared() > 0.0001f)
                {
                    return destinationDirection;
                }
            }

            return Vector2.Zero;
        }
    }
}
