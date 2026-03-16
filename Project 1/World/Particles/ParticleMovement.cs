using Microsoft.Xna.Framework;
using Project_1.Camera;
using Project_1.Managers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Project_1.Particles
{
    internal class ParticleMovement
    {

        public WorldSpace Momentum => SplitVectorTuple(momentum);
        (WorldSpace, WorldSpace) momentum;
        public WorldSpace Velocity { get => SplitVectorTuple(velocity); }
        (WorldSpace, WorldSpace) velocity;

        public float Drag
        {
            get
            {
                if (drag.Item1 == drag.Item2)
                {
                    return drag.Item1;
                }

                return (float)RandomManager.RollDouble(drag.Item1, drag.Item2);
            }
        }
        (float, float) drag;

        public ParticleMovement(WorldSpace aMomentum, WorldSpace aVelocity, float aDrag) : this((aMomentum, aMomentum), (aVelocity, aVelocity), (aDrag, aDrag)) { }
        public ParticleMovement((WorldSpace, WorldSpace) aMomentum, (WorldSpace, WorldSpace) aVelocity, float aDrag) : this(aMomentum, aVelocity, (aDrag, aDrag)) { }

        public ParticleMovement((WorldSpace, WorldSpace) aMomentum, (WorldSpace, WorldSpace) aVelocity, (float, float) aDrag)
        {

            momentum = aMomentum;
            velocity = aVelocity;
            drag = aDrag;
        }



        WorldSpace SplitVectorTuple((WorldSpace, WorldSpace) aPairToSplit)
        { 
            if (aPairToSplit.Item1 == aPairToSplit.Item2)
            {
                return aPairToSplit.Item1;
            }

            return new WorldSpace((float)RandomManager.RollDouble(aPairToSplit.Item1.X, aPairToSplit.Item2.X), (float)RandomManager.RollDouble(aPairToSplit.Item2.Y, aPairToSplit.Item2.Y));
        }
    }
}
