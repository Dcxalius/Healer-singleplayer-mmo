using Project_1.Camera;
using Project_1.Textures;
using Project_1.UI.UIElements;
using Project_1.UI.UIElements.Bars;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Project_1.Managers;
using Project_1.Messaging.Events;
using Project_1.Rendering;
using Project_1.Tiles;
using Project_1.UI.UIElements.Boxes;

namespace Project_1.UI.HUD
{
    internal class NamePlate : Box
    {
        const int PreviewVerticalGapPixels = 8;
        Bar healthBar;
        public string Name => name.Text;
        Label name;
        static Color backgroundColor = new Color(120, 50, 50, 80);
        RelativeScreenPosition barSize = new RelativeScreenPosition(1f, 0.2f);
        WorldSpace feetPosition;
        int worldHeight;
        WorldSpace3D namePlateAnchorWorldPosition;
        AbsoluteScreenPosition anchorAbsolutePosition;
        AbsoluteScreenPosition collisionOffset;
        float healthRatio;
        //RelativeScreenPosition barSize = new RelativeScreenPosition(0.02f, 0.006f);

        static Color ColorTransform(Color aRelationColor)
        {
            float transp = 120f / 255f;

            byte r = (byte)((transp * aRelationColor.R / 255f) * 255);
            byte g = (byte)((transp * aRelationColor.G / 255f) * 255);
            byte b = (byte)((transp * aRelationColor.B / 255f) * 255);
            byte a = (byte)((transp * aRelationColor.A / 255f) * 255);

            return new Color(r, g, b, a);
        }

        public NamePlate(UIElement aParent, in EntityUiSnapshot snapshot) : base(aParent, new UITexture("GrayBackground", ColorTransform(snapshot.RelationColor)), RelativeScreenPosition.Zero, RelativeScreenPosition.Zero)
        {
            name = new Label(this, RelativeScreenPosition.Zero, RelativeScreenPosition.Zero, Label.TextAllignment.TopCentre, aText: snapshot.Name);
            healthBar = new Bar(this, new BarTexture(BarTexture.FillingDirection.Right, Color.Red), new UITexture("WhiteBackground", backgroundColor), RelativeScreenPosition.Zero, barSize);
            
            SetTarget(snapshot);
        }

        public AbsoluteScreenPosition CollisionOffset => collisionOffset;
        public AbsoluteScreenPosition AnchorAbsolutePosition => anchorAbsolutePosition;

        void SetTarget(in EntityUiSnapshot snapshot) //TODO: Add a minimum size for bar and a maximum size of name.
        {
            name.Text = snapshot.Name;
            healthRatio = (float)(snapshot.CurrentHealth / snapshot.MaxHealth);
            healthBar.Value = healthRatio;
            gfx.Color = ColorTransform(snapshot.RelationColor);

            AbsoluteScreenPosition textOffset = new AbsoluteScreenPosition((int)name.UnderlyingTextOffset.X, (int)name.UnderlyingTextOffset.Y);


            healthBar.Resize(barSize);

            Resize((textOffset.OnlyX + textOffset.OnlyY * (1 + barSize.Y)).ToRelativeScreenPosition());

            name.Resize(textOffset.ToRelativeScreenPosition(Size));     

            UpdateAnchor(snapshot);
            SyncToAnchor();

            healthBar.Move(RelativeScreenPosition.One.OnlyY - healthBar.RelativeSize.OnlyY);
        }

        public override void Rescale()
        {
            healthBar.Resize(barSize);
            AbsoluteScreenPosition textOffset = new AbsoluteScreenPosition((int)name.UnderlyingTextOffset.X, (int)name.UnderlyingTextOffset.Y);

            Resize((textOffset.OnlyX + textOffset.OnlyY * (1 + barSize.Y)).ToRelativeScreenPosition());

            name.Resize(textOffset.ToRelativeScreenPosition(Size));

            SyncToAnchor();
            healthBar.Move(RelativeScreenPosition.One.OnlyY - healthBar.RelativeSize.OnlyY);
        }

        public void Refresh(in EntityUiSnapshot snapshot)
        {
            healthRatio = (float)(snapshot.CurrentHealth / snapshot.MaxHealth);
            healthBar.Value = healthRatio;
            gfx.Color = ColorTransform(snapshot.RelationColor);
            if (name.Text != snapshot.Name)
            {
                SetTarget(snapshot);
                return;
            }
            UpdateAnchor(snapshot);
            SyncToAnchor();
        }

        public void SyncToAnchor()
        {
            RelativeScreenPosition basePos = ResolveBasePosition();
            if (!Visible) return;
            Move(basePos + collisionOffset.ToRelativeScreenPosition());
        }

        public void ResetCollisionOffset()
        {
            collisionOffset = AbsoluteScreenPosition.Zero;
            SyncToAnchor();
        }

        public void AddCollisionOffset(AbsoluteScreenPosition amount)
        {
            if (amount == AbsoluteScreenPosition.Zero) return;
            collisionOffset += amount;
            SyncToAnchor();
        }

        void UpdateAnchor(in EntityUiSnapshot snapshot)
        {
            feetPosition = snapshot.FeetPosition;
            worldHeight = snapshot.WorldHeight;
            namePlateAnchorWorldPosition = snapshot.NamePlateAnchorWorldPosition;
        }

        RelativeScreenPosition ResolveBasePosition()
        {
            if (DebugManager.Mode(DebugMode.ModelPreview))
            {
                if (!TryResolvePreviewAnchorAbsolutePosition(out anchorAbsolutePosition))
                {
                    Visible = false;
                    return RelativeScreenPosition.Zero;
                }

                Visible = true;
                return (anchorAbsolutePosition - new AbsoluteScreenPosition(Size.X / 2, PreviewVerticalGapPixels)).ToRelativeScreenPosition() - RelativeSize.OnlyY;
            }

            Visible = true;
            anchorAbsolutePosition = feetPosition.ToAbsoltueScreenPosition();
            return (anchorAbsolutePosition - new AbsoluteScreenPosition(Size.X / 2, worldHeight * 2)).ToRelativeScreenPosition() - RelativeSize.OnlyY;
        }

        bool TryResolvePreviewAnchorAbsolutePosition(out AbsoluteScreenPosition screenPosition)
        {
            Camera3D previewCamera = WorldBlockRenderer.CreatePreviewCamera(Camera.Camera.CentreInWorldSpace);
            return TryResolvePreviewAnchorAbsolutePosition(previewCamera, out screenPosition);
        }

        bool TryResolvePreviewAnchorAbsolutePosition(Camera3D aPreviewCamera, out AbsoluteScreenPosition screenPosition)
        {
            Vector3 toAnchor = namePlateAnchorWorldPosition.ToVector3() - aPreviewCamera.Position.ToVector3();
            if (Vector3.Dot(aPreviewCamera.Forward.ToVector3(), toAnchor) <= 0f)
            {
                screenPosition = AbsoluteScreenPosition.Zero;
                return false;
            }

            screenPosition = aPreviewCamera.WorldToScreen(namePlateAnchorWorldPosition);
            return true;
        }
    }
}
