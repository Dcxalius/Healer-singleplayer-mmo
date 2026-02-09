using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Project_1.Camera;
using Project_1.Messaging.Events;
using Project_1.Textures;
using Project_1.UI.UIElements.Bars;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Project_1.UI.HUD.PlateBoxes
{
    internal class PlateBoxHealthSegment : PlateBoxSegment
    {
        ResourceBar healthBar;
        static Color backgroundColor = new Color(255, 211, 211, 120);

        public PlateBoxHealthSegment(RelativeScreenPosition aPos, RelativeScreenPosition aSize) : base(null, aPos, aSize)
        {
            healthBar = new ResourceBar(new BarTexture(BarTexture.FillingDirection.Right, Color.Red), new UITexture("WhiteBackground", backgroundColor), RelativeScreenPosition.Zero, RelativeScreenPosition.One);

            AddChild(healthBar);
        }




        public override void Refresh(in EntityUiSnapshot snapshot)
        {
            healthBar.MaxValue = (float)snapshot.MaxHealth;
            healthBar.Value = (float)snapshot.CurrentHealth;
        }

        public void SetTarget(in EntityUiSnapshot snapshot)
        {
            healthBar.MaxValue = (float)snapshot.MaxHealth;
            healthBar.Value = (float)snapshot.CurrentHealth;
        }


        public override void Draw(SpriteBatch aBatch)
        {
            Project_1.Managers.ThreadAffinity.AssertMainThread();
            base.Draw(aBatch);
        }
    }
}
