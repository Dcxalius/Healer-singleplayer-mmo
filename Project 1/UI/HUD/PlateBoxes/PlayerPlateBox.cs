﻿using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Project_1.Camera;
using Project_1.GameObjects;
using Project_1.GameObjects.Entities;
using Project_1.GameObjects.Entities.Players;
using Project_1.Input;
using Project_1.Textures;
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
        static PlateBoxNameSegment name;
        static PlateBoxHealthSegment health;
        static PlateBoxResourceSegment resource;

        public PlayerPlateBox(RelativeScreenPosition aPos, RelativeScreenPosition aSize) : base(aPos, aSize)
        {

            name = new PlateBoxNameSegment(null, Color.White, new RelativeScreenPosition(0, 0), new RelativeScreenPosition(1, 0.5f));
            health = new PlateBoxHealthSegment(new RelativeScreenPosition(0, 0.5f), new RelativeScreenPosition(1, 0.25f));
            resource = new PlateBoxResourceSegment(new RelativeScreenPosition(0, 0.75f), new RelativeScreenPosition(1, 0.25f));


            leftVerticalSegments = new PlateBoxSegment[] { };
            rightVerticalSegments = new PlateBoxSegment[] { };
            horizontalSegments = new PlateBoxSegment[] { name, health, resource };

            AddSegmentsToChildren();

        }

        public void SetData(Entity aEntity)
        {
            name.Refresh(aEntity);
            resource.SetTarget(aEntity);
            health.Refresh(aEntity);
            levelCircle.Refresh(aEntity);
        }

        public override void Refresh(Entity aEntity)
        {
            health.Refresh(aEntity);
            resource.Refresh(aEntity);
            levelCircle.Refresh(aEntity);
        }

        public override void ClickedOnAndReleasedOnMe()
        {
            base.ClickedOnAndReleasedOnMe();

            ObjectManager.Player.SetTarget(ObjectManager.Player);
        }

        protected override bool ClickedOnChildren(ClickEvent aClick)
        {
            return false;
        }
    }
}
