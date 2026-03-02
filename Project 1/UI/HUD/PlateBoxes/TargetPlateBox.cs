using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Project_1.Camera;
using Project_1.Input;
using Project_1.Managers.States;
using Project_1.Messaging.Events;
using Project_1.UI.UIElements;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace Project_1.UI.HUD.PlateBoxes
{
    internal class TargetPlateBox : PlateBox
    {

        int? targetRenderId;
        EntityUiSnapshot? targetSnapshot;

        PlateBoxNameSegment nameSegment;
        PlateBoxHealthSegment healthSegment;
        PlateBoxResourceSegment resourceSegment;


        public TargetPlateBox(RelativeScreenPosition aPos, RelativeScreenPosition aSize) : base(aPos, aSize)
        {
            nameSegment = new PlateBoxNameSegment(null, Color.White, RelativeScreenPosition.Zero, new RelativeScreenPosition(1, 0.5f));
            healthSegment = new PlateBoxHealthSegment(new RelativeScreenPosition(0, 0.5f), new RelativeScreenPosition(1, 0.25f));
            resourceSegment = new PlateBoxResourceSegment(new RelativeScreenPosition(0, 0.75f), new RelativeScreenPosition(1, 0.25f));

            leftVerticalSegments = new PlateBoxSegment[] { };
            rightVerticalSegments = new PlateBoxSegment[] { };
            horizontalSegments = new PlateBoxSegment[] { nameSegment, healthSegment, resourceSegment };
            Visible = false;

            AddSegmentsToChildren();
        }


        public override void Refresh(in EntityUiSnapshot snapshot)
        {
            targetSnapshot = snapshot;
            healthSegment.Refresh(snapshot);
            levelCircle.Refresh(snapshot);
            resourceSegment.Refresh(snapshot);
        }

        public bool BelongsTo(int? aRenderId) => targetRenderId.HasValue && aRenderId.HasValue && targetRenderId.Value == aRenderId.Value;

        public void SetTarget(EntityUiSnapshot? aTarget)
        {
            if (!aTarget.HasValue)
            {
                targetRenderId = null;
                targetSnapshot = null;
                nameSegment.Name = null;
                Visible = false;
                return;
            }

            EntityUiSnapshot snapshot = aTarget.Value;
            targetRenderId = snapshot.RenderId;
            targetSnapshot = snapshot;
            nameSegment.Refresh(snapshot);
            healthSegment.SetTarget(snapshot);
            levelCircle.Refresh(snapshot);
            resourceSegment.SetTarget(snapshot);
            Visible = true;
        }


        public override void Draw(SpriteBatch aBatch)
        {
            Project_1.Managers.ThreadAffinity.AssertMainThread();
            if (!targetRenderId.HasValue) return;

            base.Draw(aBatch);
        }

        internal override PlateBoxRenderSnapshot BuildRenderSnapshot()
        {
            if (!targetSnapshot.HasValue || !targetRenderId.HasValue)
            {
                return new PlateBoxRenderSnapshot(false, AbsolutePos, string.Empty, Color.White, 0f, 0f, 0f, 0f, Color.White, 1, false);
            }

            EntityUiSnapshot snapshot = targetSnapshot.Value;
            return new PlateBoxRenderSnapshot(
                Visible,
                AbsolutePos,
                snapshot.Name,
                snapshot.RelationColor,
                (float)snapshot.CurrentHealth,
                (float)snapshot.MaxHealth,
                snapshot.CurrentResource,
                snapshot.MaxResource,
                snapshot.ResourceColor,
                snapshot.Level,
                false);
        }

    }
}
