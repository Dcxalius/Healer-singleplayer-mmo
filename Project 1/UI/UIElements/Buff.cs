using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Project_1.Textures;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Project_1.UI.UIElements
{
    internal class Buff : UIElement
    {
        public double duration => buff.Duration;
        GameObjects.Spells.Buff buff;
        Text xdd;
        public Buff(GameObjects.Spells.Buff aBuff, Vector2 aPos, Vector2 aSize) : base(new UITexture(aBuff.GfxPath, Color.White), aPos, aSize)
        {
            xdd = new Text("Gloryse", "xdd", Color.White);
            buff = aBuff;
            
        }
        public override void Draw(SpriteBatch aBatch)
        {
            base.Draw(aBatch);

            xdd.CentredDraw(aBatch, new Vector2(AbsolutePos.Center.X, AbsolutePos.Center.Y - AbsolutePos.Size.Y));
        }

        public int CompareTo(Buff aBuffToCompare)
        {
            if (aBuffToCompare == null) return 1;
            return duration.CompareTo(aBuffToCompare.duration);
        }
    }
}
