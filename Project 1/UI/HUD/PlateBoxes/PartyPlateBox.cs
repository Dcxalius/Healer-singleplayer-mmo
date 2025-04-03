using Microsoft.Xna.Framework;
using Project_1.Camera;
using Project_1.GameObjects;
using Project_1.GameObjects.Entities;
using Project_1.Input;
using Project_1.UI.UIElements;
using Project_1.UI.UIElements.PlateBoxes;
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
        public GuildMember GuildMember => guildMember;
        GuildMember guildMember;

        PlateBoxNameSegment name;
        PlateBoxHealthSegment health;
        PlateBoxResourceSegment resource;

        CommandBorder border;

        static float spacing = 0.05f;

        public static int PartyBoxesActive => partyBoxesActive;
        static int partyBoxesActive = 0;
        public static void ClearPartyBoxes() => partyBoxesActive = 0;

        public PartyPlateBox(RelativeScreenPosition aPos, RelativeScreenPosition aSize, int aIndex) : base(aPos, aSize)
        {
            name = new PlateBoxNameSegment(null, Color.White, new RelativeScreenPosition(0, 0), new RelativeScreenPosition(1, aSize.Y / 2));
            health = new PlateBoxHealthSegment(new RelativeScreenPosition(0, aSize.Y / 2), new RelativeScreenPosition(1, aSize.Y / 4));

            resource = new PlateBoxResourceSegment(new RelativeScreenPosition(0, aSize.Y / 4 * 3), new RelativeScreenPosition(1, aSize.Y / 4));

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


        public bool BelongsTo(GuildMember aGuildMember)
        {
            return aGuildMember == guildMember;

        }

        public void SetTarget(GuildMember aGuildMember)
        {
            if (guildMember == null) partyBoxesActive += 1;

            guildMember = aGuildMember;
            health.SetTarget(aGuildMember);
            resource.SetTarget(aGuildMember);
            name.Refresh(aGuildMember);
            Visible = true;

        }

        public void RemoveTarget()
        {
            guildMember = null;
            partyBoxesActive -= 1;
            Visible = false;
        }

        public override void ClickedOnAndReleasedOnMe()
        {
            base.ClickedOnAndReleasedOnMe();

            ObjectManager.Player.SetTarget(guildMember);
        }

        protected override bool ClickedOnChildren(ClickEvent aClick)
        {
            return false;
        }

        protected override void ClickedOnMe(ClickEvent aClick)
        {
            if (hudMoving)
            {
                base.ClickedOnMe(aClick);
                return;
            }

            guildMember.Command(aClick);

            base.ClickedOnMe(aClick);
        }

        public override void Refresh(Entity aEntity)
        {
            //name.Name = aEntity.Name;
            health.Refresh(aEntity);
            resource.Refresh(aEntity);
            levelCircle.Refresh(aEntity);
        }
    }
}
