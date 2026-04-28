using System;

namespace Project_1.System.Models
{
    internal sealed class Mesh3D : Mesh
    {
        public Vertex3D[] Vertices => vertices;
        readonly Vertex3D[] vertices;

        public Mesh3D(Material aMaterial, Vertex3D[] aVertices, params int[] aIndices)
            : base(aMaterial, aIndices)
        {
            vertices = aVertices ?? Array.Empty<Vertex3D>();
        }

        public override void Update()
        {
            for (int i = 0; i < vertices.Length; i++)
            {
                vertices[i]?.Update();
            }
        }
    }
}
