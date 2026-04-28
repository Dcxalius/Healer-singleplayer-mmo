using System;

namespace Project_1.System.Models
{
    internal sealed class Mesh2D : Mesh
    {
        public Vertex2D[] Vertices => vertices;
        readonly Vertex2D[] vertices;

        public Mesh2D(Material aMaterial, Vertex2D[] aVertices, params int[] aIndices)
            : base(aMaterial, aIndices)
        {
            vertices = aVertices ?? Array.Empty<Vertex2D>();
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
