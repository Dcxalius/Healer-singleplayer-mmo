using Microsoft.Xna.Framework;
using Project_1.Textures;

namespace Project_1.System.Models
{
    internal class Material
    {
        public string Name => name;
        readonly string name;

        public GfxPath DiffuseTexturePath => diffuseTexturePath;
        readonly GfxPath diffuseTexturePath;

        public Color Tint => tint;
        readonly Color tint;

        public Material(string aName, GfxPath aDiffuseTexturePath, Color aTint)
        {
            name = aName;
            diffuseTexturePath = aDiffuseTexturePath;
            tint = aTint;
        }

        public Material(string aName, GfxPath aDiffuseTexturePath)
            : this(aName, aDiffuseTexturePath, Color.White)
        {
        }
    }
}
