using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System.Runtime.InteropServices;

[StructLayout(LayoutKind.Sequential)]
public struct ShadowVertex : IVertexType
{
    public Vector4 Segment;     // POSITION0
    public Vector2 ShadowCoord; // TEXCOORD0

    public static readonly VertexDeclaration VertexDeclaration = new VertexDeclaration(
        new VertexElement(0, VertexElementFormat.Vector4, VertexElementUsage.Position, 0),
        new VertexElement(16, VertexElementFormat.Vector2, VertexElementUsage.TextureCoordinate, 0)
    );

    VertexDeclaration IVertexType.VertexDeclaration => VertexDeclaration;

    public ShadowVertex(Vector4 segment, Vector2 shadowCoord)
    {
        Segment = segment;
        ShadowCoord = shadowCoord;
    }
}