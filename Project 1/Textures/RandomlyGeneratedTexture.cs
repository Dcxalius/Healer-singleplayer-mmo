﻿using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Project_1.Managers;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Project_1.Textures
{
    internal class RandomlyGeneratedTexture : Texture
    {
        public RandomlyGeneratedTexture(bool aFreelyRandomized, Point aVisibleSize, GfxPath aPath) : base(aPath, aVisibleSize)
        {
            if (aFreelyRandomized) 
            { 
                Point validPoints = new Point(gfx.Width-aVisibleSize.X, gfx.Height-aVisibleSize.Y);

                int xRoll = (int)(validPoints.X * RandomManager.RollDouble());
                int yRoll = (int)(validPoints.Y * RandomManager.RollDouble());

                Visible = new Rectangle(new Point(xRoll, yRoll), aVisibleSize);
            }
            else 
            {
                Point RectsFitted = new Point(gfx.Width/aVisibleSize.X, gfx.Height/aVisibleSize.Y);

                int xRoll = aVisibleSize.X * RandomManager.RollInt(RectsFitted.X);
                int yRoll = aVisibleSize.Y * RandomManager.RollInt(RectsFitted.Y);

                Visible = new Rectangle(new Point(xRoll, yRoll), aVisibleSize);
            }

        }
    }
}
