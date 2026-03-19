using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Project_1.Camera;
using Project_1.Managers;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Project_1.DebugTools
{
    internal class DebugLine : DebugShape
    {
        public WorldSpace startPos;
        public WorldSpace dirV;

        float length;
        //int width;


        [DebuggerStepThrough]
        public DebugLine(WorldSpace aStartPos, WorldSpace aDirV, float aLength) : base (Color.Red)
        {
            
            length = aLength;
            startPos = aStartPos;
            dirV = aDirV;
        }

        public override void Draw(SpriteBatch aBatch)
        {
            
            for (int i = 0; i < length; i++)
            {
                base.Draw(aBatch, (startPos + dirV * i));
            }
        }
    }
}
