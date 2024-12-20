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

        public Movement(float aSpeed, float aMaxSpeed)
        {
            speed = aSpeed;
            maxSpeed = aMaxSpeed;
        }
    }
}
