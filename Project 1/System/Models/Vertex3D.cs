using Project_1.Camera;
using System;

namespace Project_1.System.Models
{
    internal sealed class VertexInfluence3D
    {
        public Joint3D Joint => joint;
        readonly Joint3D joint;

        public WorldSpace3D LocalBindOffset => localBindOffset;
        readonly WorldSpace3D localBindOffset;

        public float Weight => weight;
        readonly float weight;

        public VertexInfluence3D(Joint3D aJoint, WorldSpace3D aLocalBindOffset, float aWeight)
        {
            joint = aJoint ?? throw new ArgumentNullException(nameof(aJoint));
            localBindOffset = aLocalBindOffset;
            weight = aWeight;
        }
    }

    internal sealed class Vertex3D : Vertex
    {
        public WorldSpace3D BindPosition => bindPosition;
        readonly WorldSpace3D bindPosition;

        public WorldSpace3D CurrentPosition => currentPosition;
        WorldSpace3D currentPosition;

        public TextureCoordinate2D Uv => uv;
        readonly TextureCoordinate2D uv;

        public VertexInfluence3D[] Influences => influences;
        readonly VertexInfluence3D[] influences;

        public Vertex3D(TextureCoordinate2D aUv, params VertexInfluence3D[] aInfluences)
        {
            uv = aUv;
            influences = aInfluences ?? Array.Empty<VertexInfluence3D>();
            bindPosition = CalculatePosition(useBindPose: true);
            currentPosition = bindPosition;
        }

        public Vertex3D(params VertexInfluence3D[] aInfluences)
            : this(TextureCoordinate2D.Zero, aInfluences)
        {
        }

        public override void Update()
        {
            currentPosition = CalculatePosition(useBindPose: false);
        }

        WorldSpace3D CalculatePosition(bool useBindPose)
        {
            if (influences.Length == 0) return WorldSpace3D.Zero;

            WorldSpace3D result = WorldSpace3D.Zero;
            float totalWeight = 0f;

            for (int i = 0; i < influences.Length; i++)
            {
                VertexInfluence3D influence = influences[i];
                if (influence == null) continue;

                WorldSpace3D transformed = useBindPose
                    ? influence.Joint.Anchor + influence.LocalBindOffset
                    : influence.Joint.TransformLocalPoint(influence.LocalBindOffset);

                result += transformed * influence.Weight;
                totalWeight += influence.Weight;
            }

            return result / NormalizeTotalWeight(totalWeight);
        }
    }
}
