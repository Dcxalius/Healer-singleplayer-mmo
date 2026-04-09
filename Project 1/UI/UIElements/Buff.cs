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
        public GfxPath GfxPath => gfxPath;
        readonly GfxPath gfxPath;

        double DurationRemainingMs => finalTime - TimeManager.InstanceTotalFrameTime;
        double finalTime;

        public int BuffId => buffId;
        int buffId;
        
        Label timer;
        Label counter;
        public Buff(UIElement aParent, in BuffUiSnapshot aBuff, RelativeScreenPosition aPos, RelativeScreenPosition aSize) : base(aParent, new UITexture(aBuff.GfxPath, Color.White), aPos, aSize)
        {
            if (aBuff.MaxCount > 1)
            {
                counter = new Label(this, aPos, aSize, Label.TextAllignment.TopLeft, Color.Black, "Comfortaa-msdf", 8, aBuff.Count.ToString());
                AddChild(counter);
            }

            timer = new Label(this, aPos, aSize, Label.TextAllignment.BottomRight, Color.Black, "Comfortaa-msdf", 8, "xdd");
            AddChild(timer);
            buffId = aBuff.BuffId;
            gfxPath = aBuff.GfxPath;
            finalTime = aBuff.FinalTime;
            SetTimer(DurationRemainingMs);
        }

        public void Refresh(in BuffUiSnapshot aBuff)
        {
            finalTime = aBuff.FinalTime;
            if (aBuff.MaxCount > 1) counter.Text = aBuff.Count.ToString();
            SetTimer(DurationRemainingMs);
        }

        public override void Update()
        {
            base.Update();
            SetTimer(DurationRemainingMs);
        }

        void SetTimer(double aTime)
        {
            double seconds = aTime / 1000;
            if (seconds >= 10) timer.Text = ((int)Math.Round(seconds, MidpointRounding.ToZero)).ToString();
            else timer.Text = Math.Round(seconds, 2).ToString();
        }



        public int CompareTo(Buff aBuffToCompare) => DurationRemainingMs.CompareTo(aBuffToCompare.DurationRemainingMs);

        public int CompareTo(object obj)
        {
            if (obj is Buff buff)
            {
                return buff.DurationRemainingMs.CompareTo(DurationRemainingMs);
            }
            throw new ArgumentException("Tried to compare Buff to nonbuff.");
        }
    }
}
