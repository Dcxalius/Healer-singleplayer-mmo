using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Project_1.Camera;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Project_1.DebugTools
{
    [DebuggerStepThrough]
    internal class DebugPoint : DebugShape
    {
        WorldSpace pos;

        public DebugPoint(WorldSpace aPosition, float aSize) : base(Color.Yellow)
        {
            pos = aPosition;
            size = aSize;
        }


        public override void Draw(SpriteBatch aBatch)
        {
            base.Draw(aBatch, pos);
        }
    }
}
