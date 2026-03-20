using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Project_1.Camera;
using Project_1.Managers;
using Project_1.Messaging.Events;
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
        public int EffectId => effectId;
        public GfxPath GfxPath => gfxPath;
        public double Duration => durationRemainingMs;
        int effectId;
        readonly GfxPath gfxPath;
        double durationRemainingMs;
        Text xdd;
        public Buff(in BuffUiSnapshot aBuff, RelativeScreenPosition aPos, RelativeScreenPosition aSize) : base(new UITexture(aBuff.GfxPath, Color.White), aPos, aSize)
        {
            xdd = new Text("Comfortaa-msdf", "xdd", Color.Black);
            effectId = aBuff.EffectId;
            gfxPath = aBuff.GfxPath;
            durationRemainingMs = aBuff.DurationRemainingMs;
            
        }

        public void Refresh(in BuffUiSnapshot aBuff)
        {
            durationRemainingMs = aBuff.DurationRemainingMs;
        }

        public override void Update()
        {
            base.Update();
            durationRemainingMs = Math.Max(0, durationRemainingMs - TimeManager.SecondsSinceLastFrame * 1000d);
            xdd.Value = Math.Round(Duration / 1000, 1).ToString();
        }
        public override void Draw(SpriteBatch aBatch)
        {
            Project_1.Managers.ThreadAffinity.AssertMainThread();
            base.Draw(aBatch);

            xdd.CentredDraw(aBatch, new AbsoluteScreenPosition(AbsolutePos.Center.X, AbsolutePos.Center.Y + AbsolutePos.Size.Y - 3));
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
