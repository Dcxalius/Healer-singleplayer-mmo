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
        ResourceBar healthBar;
        Entity entity;
        static Color backgroundColor = new Color(255, 211, 211, 120);

        public PlateBoxHealthSegment(Entity aEntity ,Vector2 aPos, Vector2 aSize) : base(null, aPos, aSize)
        {
            entity = aEntity;
            healthBar = new ResourceBar(new BarTexture(BarTexture.FillingDirection.Right, Color.Red), new UITexture("WhiteBackground", backgroundColor), Vector2.Zero, aSize);
            if (entity != null)
            {
                healthBar.MaxValue = aEntity.Data.MaxHealth;
            }
            children.Add(healthBar);
        }

        public override void Update(in UIElement aParent)
        {
            base.Update(aParent);
            if(entity == null) { return; }
            healthBar.Value = entity.Data.MaxHealth;
        }

        public void SetTarget(Entity aEntity)
        {
            entity = aEntity;
            healthBar.MaxValue = aEntity.Data.MaxHealth;
            healthBar.Value = entity.Data.MaxHealth;
        }


        public override void Draw(SpriteBatch aBatch)
        {
            base.Draw(aBatch);
        }
    }
}
