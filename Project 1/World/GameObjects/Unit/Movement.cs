using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Project_1.GameObjects.Unit
{
    internal class Movement
    {
        public float Speed => speed;
        float speed;
        public float MaxSpeed => maxSpeed;
        float maxSpeed;

        //TODO: Set this somewhere else? Also figure out what it should be
        static float minSpeed = 1;

        public Movement(float aSpeed, float aMaxSpeed)
        {
            speed = aSpeed;
            maxSpeed = aMaxSpeed;
        }
    }
}
