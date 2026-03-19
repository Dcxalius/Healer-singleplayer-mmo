using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Project_1.DebugTools
{
    [DebuggerStepThrough]
    internal class DebugSquare : DebugShape
    {
        Rectangle rect;

        public DebugSquare(Rectangle aPosition, Color aColor) : base(aColor)
        {

            rect = aPosition;
        }

        public DebugSquare(Rectangle aPosition) : this(aPosition, Color.Blue) { }

        public override void Draw(SpriteBatch aBatch)
        {
            base.Draw(aBatch, rect);
        }
    }
}
