using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Project_1.Camera;
using Project_1.GameObjects.Entities;
using Project_1.Textures;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Project_1.UI.UIElements.PlateBoxes
{
    internal class LevelCircle : UIElement
    {
        Label numberLabel;

        public LevelCircle(RelativeScreenPosition aPos, RelativeScreenPosition aSize) : base(new UITexture("LevelCircle", Color.White), aPos, aSize)
        {
            numberLabel = new Label("1", RelativeScreenPosition.Zero, aSize, Label.TextAllignment.Centred);
            capturesClick = false;
            children.Add(numberLabel);
        }

        public void Refresh(Entity aEntity)
        {
            numberLabel.Text = aEntity.CurrentLevel.ToString();
        }

        public override void Draw(SpriteBatch aBatch)
        {
            base.Draw(aBatch);
        }
    }
}
