using Project_1.Camera;
using System;

namespace Project_1.System.Models
{
    internal sealed class VertexInfluence2D
    {
        public Joint2D Joint => joint;
        readonly Joint2D joint;

        public WorldSpace LocalBindOffset => localBindOffset;
        readonly WorldSpace localBindOffset;

        public float Weight => weight;
        readonly float weight;

        public VertexInfluence2D(Joint2D aJoint, WorldSpace aLocalBindOffset, float aWeight)
        {
            joint = aJoint ?? throw new ArgumentNullException(nameof(aJoint));
            localBindOffset = aLocalBindOffset;
            weight = aWeight;
        }
    }

    internal sealed class Vertex2D : Vertex
    {
        public WorldSpace BindPosition => bindPosition;
        readonly WorldSpace bindPosition;

        public WorldSpace CurrentPosition => currentPosition;
        WorldSpace currentPosition;

        public TextureCoordinate2D Uv => uv;
        readonly TextureCoordinate2D uv;

        public VertexInfluence2D[] Influences => influences;
        readonly VertexInfluence2D[] influences;

        public Vertex2D(TextureCoordinate2D aUv, params VertexInfluence2D[] aInfluences)
        {
            uv = aUv;
            influences = aInfluences ?? Array.Empty<VertexInfluence2D>();
            bindPosition = CalculatePosition(useBindPose: true);
            currentPosition = bindPosition;
        }

        public Vertex2D(params VertexInfluence2D[] aInfluences)
            : this(TextureCoordinate2D.Zero, aInfluences)
        {
        }

        public override void Update()
        {
            currentPosition = CalculatePosition(useBindPose: false);
        }

        WorldSpace CalculatePosition(bool useBindPose)
        {
            if (influences.Length == 0) return WorldSpace.Zero;

            WorldSpace result = WorldSpace.Zero;
            float totalWeight = 0f;

            for (int i = 0; i < influences.Length; i++)
            {
                VertexInfluence2D influence = influences[i];
                if (influence == null) continue;

                WorldSpace transformed = useBindPose
                    ? influence.Joint.Anchor + influence.LocalBindOffset
                    : influence.Joint.TransformLocalPoint(influence.LocalBindOffset);

                result += transformed * influence.Weight;
                totalWeight += influence.Weight;
            }

            return result / NormalizeTotalWeight(totalWeight);
        }
    }
}
