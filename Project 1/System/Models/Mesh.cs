using System;

namespace Project_1.System.Models
{
    internal abstract class Mesh
    {
        public int[] Indices => indices;
        readonly int[] indices;

        public Material Material => material;
        readonly Material material;

        protected Mesh(Material aMaterial, params int[] aIndices)
        {
            material = aMaterial;
            indices = aIndices ?? Array.Empty<int>();
        }

        public abstract void Update();
    }
}
