using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Project_1.Camera;
using Project_1.Input;
using Project_1.Messaging;
using Project_1.Messaging.Events;
using Project_1.Textures;
using Project_1.UI.UIElements;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace Project_1.UI.HUD.PlateBoxes
{
    internal class PlayerPlateBox : PlateBox
    {
        EntityUiSnapshot? latestSnapshot;
        static PlateBoxNameSegment name;
        static PlateBoxHealthSegment health;
        static PlateBoxResourceSegment resource;

        public PlayerPlateBox(RelativeScreenPosition aPos, RelativeScreenPosition aSize) : base(aPos, aSize)
        {

            name = new PlateBoxNameSegment(this, Color.White, Color.Black, new RelativeScreenPosition(0, 0), new RelativeScreenPosition(1, 0.5f));
            health = new PlateBoxHealthSegment(this, new RelativeScreenPosition(0, 0.5f), new RelativeScreenPosition(1, 0.25f));
            resource = new PlateBoxResourceSegment(this, new RelativeScreenPosition(0, 0.75f), new RelativeScreenPosition(1, 0.25f));


            leftVerticalSegments = new PlateBoxSegment[] { };
            rightVerticalSegments = new PlateBoxSegment[] { };
            horizontalSegments = new PlateBoxSegment[] { name, health, resource };

            AddSegmentsToChildren();

            health.SetTextSize(UIElements.Bars.ResourceBar.Labels.Percentage, Size.Y - 2);
        }

        public void SetData(in EntityUiSnapshot snapshot)
        {
            latestSnapshot = snapshot;
            name.Refresh(snapshot);
            resource.SetTarget(snapshot);
            health.Refresh(snapshot);
            levelCircle.Refresh(snapshot);
        }

        public override void Refresh(in EntityUiSnapshot snapshot)
        {
            latestSnapshot = snapshot;
            health.Refresh(snapshot);
            resource.Refresh(snapshot);
            levelCircle.Refresh(snapshot);
        }

        public override void ClickedOnAndReleasedOnMe()
        {
            base.ClickedOnAndReleasedOnMe();

            MailboxManager.PublishSimCommand(new TargetRequested(null));
        }

        protected override bool ClickedOnChildren(ClickEvent aClick)
        {
            return false;
        }
    }
}
