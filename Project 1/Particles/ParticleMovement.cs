using Microsoft.Xna.Framework;
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

        public Vector2 Momentum => SplitVectorTuple(momentum);
        (Vector2, Vector2) momentum;
        public Vector2 Velocity { get => SplitVectorTuple(velocity); }
        (Vector2, Vector2) velocity;

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

        public ParticleMovement(Vector2 aMomentum, Vector2 aVelocity, float aDrag)
        {
            __constructor__((aMomentum, aMomentum), (aVelocity, aVelocity), (aDrag, aDrag));
        }
        public ParticleMovement((Vector2, Vector2) aMomentum, (Vector2, Vector2) aVelocity, float aDrag)
        {
            __constructor__(aMomentum, aVelocity, (aDrag, aDrag));
        }

        public ParticleMovement((Vector2, Vector2) aMomentum, (Vector2, Vector2) aVelocity, (float, float) aDrag)
        {
            __constructor__(aMomentum, aVelocity, aDrag);
        }

        void __constructor__((Vector2, Vector2) aMomentum, (Vector2, Vector2) aVelocity, (float, float) aDrag)
        {

            momentum = aMomentum;
            velocity = aVelocity;
            drag = aDrag;
        }


        Vector2 SplitVectorTuple((Vector2, Vector2) aPairToSplit)
        { 
            if (aPairToSplit.Item1 == aPairToSplit.Item2)
            {
                return aPairToSplit.Item1;
            }

            return new Vector2((float)RandomManager.RollDouble(aPairToSplit.Item1.X, aPairToSplit.Item2.X), (float)RandomManager.RollDouble(aPairToSplit.Item2.Y, aPairToSplit.Item2.Y));
        }
    }
}
