using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Project_1.GameObjects;
using Project_1.Textures;
using Project_1.UI.UIElements.Bars;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Project_1.UI.UIElements.PlateBoxes
{
    internal class PlateBoxHealthSegment : PlateBoxSegment
    {
        Bar healthBar;
        Entity ent;
        static Color backgroundColor = new Color(255, 211, 211, 120);

        public PlateBoxHealthSegment(Entity aEntity ,Vector2 aPos, Vector2 aSize) : base(null, aPos, aSize)
        {
            ent = aEntity;
            healthBar = new Bar(ent.MaxHealth, new BarTexture(BarTexture.FillingDirection.Right, Color.Red), new UITexture("WhiteBackground", backgroundColor), Vector2.Zero, aSize);
            children.Add(healthBar);
        }

        public override void Update(in UIElement aParent)
        {
            base.Update(aParent);
            healthBar.Value = ent.CurrentHealth;
        }

        public override void Draw(SpriteBatch aBatch)
        {
            base.Draw(aBatch);
        }
    }
}
