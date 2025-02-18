using Microsoft.Xna.Framework;
using Project_1.Camera;
using Project_1.Managers;
using SharpDX.MediaFoundation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Project_1.GameObjects.Spawners
{
    internal class Wander : MobPathing
    {
        public override WorldSpace? GetNextSpace
        {
            get
            {
                StartTimer();
                if (!TimeForMove()) return null;
                WorldSpace newSpace = new WorldSpace();
                newSpace.X = (float)(boundry.Location.X + RandomManager.RollDouble() * boundry.Width);
                newSpace.Y = (float)(boundry.Location.Y + RandomManager.RollDouble() * boundry.Height);
                Reset();
                return newSpace;
            }
        }
        public override WorldSpace GetLatestSpace => lastDirection;


        WorldSpace lastDirection;
        Rectangle boundry;

        public Wander(Rectangle aBoundry)
        {
            boundry = aBoundry;
        }
    }
}
