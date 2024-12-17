using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Project_1.DebugTools
{
    internal class DebugSquare : DebugShape
    {
        Rectangle rect;

        public DebugSquare(Rectangle position) : base(Color.Blue)
        {
            rect = position;
        }

        public override void Draw(SpriteBatch aBatch)
        {
            base.Draw(aBatch, rect);
        }
    }
}
