using Microsoft.Xna.Framework;
using Project_1.Camera;
using Project_1.Managers;
using Project_1.Tiles;
using SharpDX.MediaFoundation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Metadata.Ecma335;
using System.Text;
using System.Threading.Tasks;

namespace Project_1.GameObjects.Spawners
{
    internal class Wander : MobPathing
    {
        public override PathingType UnderlyingType => PathingType.Wander;

        public override WorldSpace? GetNextSpace
        {
            get
            {
                StartTimer();
                if (!TimeForMove()) return null;
                
                return NewSpawn(GetLatestSpace);
            }
        }
        public override WorldSpace GetLatestSpace => lastDirection;


        WorldSpace lastDirection;


        Rectangle boundry;

        public Wander(Rectangle aBoundry)
        {
            boundry = aBoundry;
        }

        public override WorldSpace NewSpawn(WorldSpace aSize)
        {
            Reset();
            WorldSpace newSpace = new WorldSpace();
            newSpace.X = (float)(boundry.Location.X + RandomManager.RollDouble() * boundry.Width);
            newSpace.Y = (float)(boundry.Location.Y + RandomManager.RollDouble() * boundry.Height);
            newSpace = TileManager.FindClosestWalkableWorldSpace(newSpace, aSize);
            return newSpace;
        }
    }
}
