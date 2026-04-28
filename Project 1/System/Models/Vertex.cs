using System;

namespace Project_1.System.Models
{
    internal abstract class Vertex
    {
        protected static float NormalizeTotalWeight(float totalWeight)
        {
            return totalWeight <= float.Epsilon ? 1f : totalWeight;
        }

        public abstract void Update();
    }
}
