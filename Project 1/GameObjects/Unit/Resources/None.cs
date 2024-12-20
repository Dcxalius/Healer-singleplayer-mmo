using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Project_1.GameObjects.Unit.Resources
{
    internal class None : Resource
    {

        public None() : base(ResourceType.None, Color.Gray)
        {
        }

        public override float MaxValue => 0;

        public override float Value { get => 0; }
    }
}
