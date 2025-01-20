using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Project_1.Camera;
using Project_1.GameObjects;
using Project_1.GameObjects.Entities;
using Project_1.GameObjects.Entities.Players;
using Project_1.Input;
using Project_1.Textures;
using Project_1.UI.UIElements.PlateBoxes;
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
        static Player player;



        static PlateBoxNameSegment name;
        static PlateBoxHealthSegment health;
        static PlateBoxResourceSegment resource;

        public PlayerPlateBox(RelativeScreenPosition aPos, RelativeScreenPosition aSize) : base(aPos, aSize)
        {
            player = ObjectManager.Player;

            name = new PlateBoxNameSegment(player.Name, player.RelationColor, new RelativeScreenPosition(0, 0), new RelativeScreenPosition(aSize.X, aSize.Y / 2));
            health = new PlateBoxHealthSegment(player, new RelativeScreenPosition(0, aSize.Y / 2), new RelativeScreenPosition(aSize.X, aSize.Y / 4));
            resource = new PlateBoxResourceSegment(player, new RelativeScreenPosition(0, aSize.Y / 4 * 3), new RelativeScreenPosition(aSize.X, aSize.Y / 4));


            leftVerticalSegments = new PlateBoxSegment[] { };
            rightVerticalSegments = new PlateBoxSegment[] { };
            horizontalSegments = new PlateBoxSegment[] { name, health, resource };

            AddSegmentsToChildren();

        }

        public override void Refresh(Entity aEntity)
        {
            name.Name = aEntity.Name;
            health.Refresh(aEntity);
            resource.Refresh(aEntity);
            levelCircle.Refresh(aEntity);
        }

        public override void HoldReleaseOnMe()
        {
            base.HoldReleaseOnMe();

            player.SetTarget(player);
        }

        protected override bool ClickedOnChildren(ClickEvent aClick)
        {
            return false;
        }

        public override void Draw(SpriteBatch aBatch)
        {
            base.Draw(aBatch);
        }
    }
}
