using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Project_1.GameObjects;
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

namespace Project_1.UI.HUD
{
    internal class PlayerPlateBox : PlateBox
    {
        static Player player = ObjectManager.Player;

        

        static PlateBoxNameSegment name;
        static PlateBoxHealthSegment health;

        public PlayerPlateBox(Vector2 aPos, Vector2 aSize) : base(aPos, aSize)
        { 
            name = new PlateBoxNameSegment(player.Name, player.RelationColor, new Vector2(0, 0), new Vector2(aSize.X, aSize.Y / 2));
            health = new PlateBoxHealthSegment(player, new Vector2(0, aSize.Y / 2), new Vector2(aSize.X, aSize.Y / 4));


            leftVerticalSegments = new PlateBoxSegment[] { };
            rightVerticalSegments = new PlateBoxSegment[] { };
            horizontalSegments = new PlateBoxSegment[] { name, health};

            AddSegmentsToChildren();

        }


        public override void HoldReleaseOnMe()
        {
            base.HoldReleaseOnMe();

            HUDManager.SetNewTarget(player);
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
