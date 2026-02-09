using Microsoft.Xna.Framework;
using Project_1.Camera;
using Project_1.Input;
using Project_1.Messaging;
using Project_1.Messaging.Events;
using Project_1.UI.HUD.Managers;
using Project_1.UI.UIElements;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace Project_1.UI.HUD.PlateBoxes
{
    internal class PartyPlateBox : PlateBox
    {
        public bool VisibleBorder { get => border.Visible; set => border.Visible = value; }
        public int? GuildMemberRenderId => guildMemberRenderId;
        public EntityUiSnapshot? Snapshot => snapshot;
        int? guildMemberRenderId;
        EntityUiSnapshot? snapshot;

        PlateBoxNameSegment name;
        PlateBoxHealthSegment health;
        PlateBoxResourceSegment resource;

        CommandBorder border;

        public static int PartyBoxesActive => partyBoxesActive;
        static int partyBoxesActive = 0;
        public static void ClearPartyBoxes() => partyBoxesActive = 0;

        public PartyPlateBox(RelativeScreenPosition aPos, RelativeScreenPosition aSize, int aIndex) : base(aPos, aSize)
        {
            name = new PlateBoxNameSegment(null, Color.White, new RelativeScreenPosition(0, 0), new RelativeScreenPosition(1, 0.5f));
            health = new PlateBoxHealthSegment(new RelativeScreenPosition(0, 0.5f), new RelativeScreenPosition(1, 0.25f));
            resource = new PlateBoxResourceSegment(new RelativeScreenPosition(0, 0.75f), new RelativeScreenPosition(1, 0.25f));

            //health = new PlateBoxHealthSegment(walker, new Vector2(0, 0), new Vector2(aSize.X, aSize.Y / 2));


            leftVerticalSegments = new PlateBoxSegment[] { };
            rightVerticalSegments = new PlateBoxSegment[] { };
            horizontalSegments = new PlateBoxSegment[] { name, health, resource };

            AddSegmentsToChildren();

            border = new CommandBorder(Color.YellowGreen, RelativeScreenPosition.Zero, aSize);
            AddChild(border);
            VisibleBorder = false;
            Visible = false;
        }


        public bool BelongsTo(int aRenderId)
        {
            return guildMemberRenderId.HasValue && guildMemberRenderId.Value == aRenderId;
        }

        public void SetTarget(in EntityUiSnapshot aGuildMember)
        {
            if (!guildMemberRenderId.HasValue) partyBoxesActive += 1;

            guildMemberRenderId = aGuildMember.RenderId;
            snapshot = aGuildMember;
            health.SetTarget(aGuildMember);
            resource.SetTarget(aGuildMember);
            name.Refresh(aGuildMember);
            levelCircle.Refresh(aGuildMember);
            Visible = true;
        }

        public void RemoveTarget()
        {
            if (guildMemberRenderId.HasValue) partyBoxesActive -= 1;
            guildMemberRenderId = null;
            snapshot = null;
            Visible = false;
        }

        public override void ClickedOnAndReleasedOnMe()
        {
            base.ClickedOnAndReleasedOnMe();

            Mailboxes.Main.Publish(new TargetRequested(guildMemberRenderId));
        }

        protected override bool ClickedOnChildren(ClickEvent aClick)
        {
            return false;
        }

        protected override void ClickedOnMe(ClickEvent aClick)
        {
            if (HUDManager.HudMoving)
            {
                base.ClickedOnMe(aClick);
                return;
            }

            if (aClick.Modifier(InputManager.HoldModifier.Shift))
            {
                Mailboxes.Main.Publish(new PartyCommandRequested(PartyCommandAction.Add, guildMemberRenderId));
            }
            else if (aClick.Modifier(InputManager.HoldModifier.Ctrl))
            {
                Mailboxes.Main.Publish(new PartyCommandRequested(PartyCommandAction.NeedyAdd, guildMemberRenderId));
            }

            base.ClickedOnMe(aClick);
        }

        public override void Refresh(in EntityUiSnapshot snapshot)
        {
            health.Refresh(snapshot);
            resource.Refresh(snapshot);
            levelCircle.Refresh(snapshot);
        }
    }
}
