
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
namespace Project_1.Textures
{
    struct ShadowVertex : IVertexType
    {
        // POSITION0: (b.x, b.y, a.x, a.y)
        public Vector4 Segment;

        // TEXCOORD0: (x = 0/1 endpoint selector, y = 0/1 near/far)
        public Vector2 ShadowCoord;

        public ShadowVertex(Vector4 segment, Vector2 shadowCoord)
        {
            Segment = segment;
            ShadowCoord = shadowCoord;
        }

        public readonly static VertexDeclaration VertexDeclaration =
            new VertexDeclaration(
                new VertexElement(0, VertexElementFormat.Vector4, VertexElementUsage.Position, 0),
                new VertexElement(16, VertexElementFormat.Vector2, VertexElementUsage.TextureCoordinate, 0)
            );

        VertexDeclaration IVertexType.VertexDeclaration => VertexDeclaration;
    }
}
