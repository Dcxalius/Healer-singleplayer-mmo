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
    internal class LevelCircle : UIElement
    {
        Label numberLabel;

        public LevelCircle(RelativeScreenPosition aPos, RelativeScreenPosition aSize) : base(new UITexture("LevelCircle", Color.White), aPos, aSize)
        {
            numberLabel = new Label("1", RelativeScreenPosition.Zero, RelativeScreenPosition.One, Label.TextAllignment.Centred, Color.Pink);
            capturesClick = false;

            AddChild(numberLabel);
        }

        public void Refresh(Entity aEntity)
        {
            numberLabel.Text = aEntity.CurrentLevel.ToString();
        }

        //public override void Resize(AbsoluteScreenPosition aSize)
        //{
        //    Resize(aSize.ToRelativeScreenPosition());
        //}

        public override void Resize(RelativeScreenPosition aSize)
        {
            base.Resize(aSize);
        }

        public override void Draw(SpriteBatch aBatch)
        {
            base.Draw(aBatch);
        }
    }
}
