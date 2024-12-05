using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Project_1.Textures;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Remoting;
using System.Text;
using System.Threading.Tasks;

namespace Project_1.UI.UIElements
{
    internal class Buff : UIElement , IComparable
    {
        public double Duration => buff.DurationRemaining;
        GameObjects.Spells.Buff buff;
        Text xdd;
        public Buff(GameObjects.Spells.Buff aBuff, Vector2 aPos, Vector2 aSize) : base(new UITexture(aBuff.GfxPath, Color.White), aPos, aSize)
        {
            xdd = new Text("Gloryse", "xdd", Color.Black);
            buff = aBuff;
            
        }

        public override void Update(in UIElement aParent)
        {
            base.Update(aParent);

            xdd.Value = Math.Round(Duration / 1000, 1).ToString();
        }
        public override void Draw(SpriteBatch aBatch)
        {
            base.Draw(aBatch);

            xdd.CentredDraw(aBatch, new Vector2(AbsolutePos.Center.X, AbsolutePos.Center.Y + AbsolutePos.Size.Y - 3));
        }

        

        public int CompareTo(Buff aBuffToCompare)
        {
            if (aBuffToCompare == null) return 1;
            return Duration.CompareTo(aBuffToCompare.Duration);
        }

        public int CompareTo(object obj)
        {
            if (obj == null) return 1;
            
            Buff b = obj as Buff;
            if(b != null)
            {
                return b.Duration.CompareTo(Duration);
            }
            else
            {
                throw new ArgumentException("Tried to compare Buff to nonbuff.");
            }


            
        }
    }
}
