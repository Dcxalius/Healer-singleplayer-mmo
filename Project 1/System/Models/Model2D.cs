using Project_1.Camera;
using Project_1.Textures;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Project_1.System.Models
{
    internal abstract class Model2D : Model
    {
        public Joint2D[] Joints => joints == null ? Array.Empty<Joint2D>() : joints.Cast<Joint2D>().ToArray();
        public Vertex2D[] Vertices => vertices == null ? Array.Empty<Vertex2D>() : vertices.Cast<Vertex2D>().ToArray();
        public Mesh2D[] Meshes => meshes == null ? Array.Empty<Mesh2D>() : meshes.Cast<Mesh2D>().ToArray();

        public virtual float AspectRatio => 1f;

        AbsoluteScreenPosition size;

        protected Model2D(int aId, string aName) : base(aId, aName)
        {
        }
    }
}
