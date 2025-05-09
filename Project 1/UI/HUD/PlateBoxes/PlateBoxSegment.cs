using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Project_1.Camera;
using Project_1.GameObjects.Entities;
using Project_1.Textures;
using Project_1.UI.UIElements;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Project_1.UI.HUD.PlateBoxes
{
    internal abstract class PlateBoxSegment : UIElement
    {
        protected string Text { get => text.Value; set => text.Value = value; }

        Text text;

        public PlateBoxSegment(UITexture aGfx, RelativeScreenPosition aPos, RelativeScreenPosition aSize) : base(aGfx, aPos, aSize)
        {
            text = new Text("Gloryse", Color.Black);
            capturesClick = false;
        }

        //public override void Resize(AbsoluteScreenPosition aSize)
        //{
        //    base.Resize(aSize);

        //    ForAllChildren((child) => child.Resize(aSize));
        //}

        public override void Resize(RelativeScreenPosition aSize)
        {
            base.Resize(aSize);
            ForAllChildren((child) =>
            {
                child.Resize(aSize);
            });
        }


        public abstract void Refresh(Entity aEntity);

        public override void Draw(SpriteBatch aBatch)
        {
            base.Draw(aBatch);

            text.CentredDraw(aBatch, new AbsoluteScreenPosition(AbsolutePos.Center));
        }
    }
}
