using Microsoft.Xna.Framework;
using Project_1.Camera;
using System;
using System.Linq;

namespace Project_1.System.Models
{
    internal class Model3D : Model
    {
        public Joint3D[] Joints => joints == null ? Array.Empty<Joint3D>() : joints.Cast<Joint3D>().ToArray();
        public Vertex3D[] Vertices => vertices == null ? Array.Empty<Vertex3D>() : vertices.Cast<Vertex3D>().ToArray();
        public Mesh3D[] Meshes => meshes == null ? Array.Empty<Mesh3D>() : meshes.Cast<Mesh3D>().ToArray();

        WorldSpace3D size;

        protected Model3D(int aId, string aName) : base(aId, aName)
        {
        }

        public override void Update()
        {
            throw new NotImplementedException();
        }

        public override void Render()
        {
            throw new NotImplementedException();
        }
    }
}
