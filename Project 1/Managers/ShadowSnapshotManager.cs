using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Project_1.Camera;
using Project_1.GameObjects;
using Project_1.Tiles;

namespace Project_1.Managers
{
    internal static class ShadowSnapshotManager
    {
        const int SegmentPaddingTiles = 2;
        const int MaxSegmentsPerLight = 512;
        const int MaxTotalSegments = 2048;
        const int MaxTrackedLights = ObjectManager.LightSnapshot.MaxLights;

        static readonly List<TileShadowSegment> segmentScratch = new List<TileShadowSegment>(MaxSegmentsPerLight);
        static readonly PreparedShadowLight[] preparedLightScratch = new PreparedShadowLight[MaxTrackedLights];
        static readonly bool[] keepLightScratch = new bool[MaxTrackedLights];
        static readonly int[] dropCandidateScratch = new int[MaxTrackedLights];
        static readonly NonCoreDropComparer nonCoreDropComparer = new NonCoreDropComparer();
        static volatile ShadowFrameSnapshot snapshot = ShadowFrameSnapshot.Empty;

        public static ShadowFrameSnapshot Snapshot => snapshot;

        public static void BuildSnapshot(ObjectManager.LightSnapshot lightSnapshot)
        {
            ThreadAffinity.AssertSimThread();
            if (lightSnapshot == null || lightSnapshot.Count == 0)
            {
                snapshot = ShadowFrameSnapshot.Empty;
                ShadowRenderTelemetry.RecordBuild(0, 0, 0, 0, 0, 0);
                return;
            }

            int lightCount = Math.Clamp(lightSnapshot.Count, 0, MaxTrackedLights);
            int candidateLights = 0;
            int candidateSegments = 0;
            int cappedLights = 0;

            for (int i = 0; i < lightCount; i++)
            {
                WorldSpace lightPosition = lightSnapshot.GetPosition(i);
                float radiusTiles = Math.Max(ILightEmitter.DefaultLightRadiusTiles, lightSnapshot.GetRadiusTiles(i));
                bool isCoreLight = lightSnapshot.IsCoreLight(i);
                float distanceToPlayer = lightSnapshot.GetDistanceToPlayerWorld(i);

                int radius = (int)MathF.Ceiling(radiusTiles) + SegmentPaddingTiles;
                Point centerTile = TileManager.GetGridPos(lightPosition);
                Rectangle tileBounds = new Rectangle(centerTile.X - radius, centerTile.Y - radius, radius * 2 + 1, radius * 2 + 1);

                int segmentCount = TileShadowOccluderBuilder.BuildSegments(tileBounds, segmentScratch, MaxSegmentsPerLight, out bool cappedByLightLimit);
                if (cappedByLightLimit) cappedLights++;

                preparedLightScratch[i] = new PreparedShadowLight(lightPosition, radiusTiles, tileBounds, isCoreLight, distanceToPlayer, segmentCount);
                bool keep = segmentCount > 0;
                keepLightScratch[i] = keep;
                if (!keep) continue;

                candidateLights++;
                candidateSegments += segmentCount;
            }

            int droppedLights = 0;
            if (candidateSegments > MaxTotalSegments)
            {
                int dropCandidateCount = 0;
                for (int i = 0; i < lightCount; i++)
                {
                    if (!keepLightScratch[i]) continue;
                    if (preparedLightScratch[i].IsCoreLight) continue;
                    dropCandidateScratch[dropCandidateCount++] = i;
                }

                if (dropCandidateCount > 1)
                {
                    Array.Sort(dropCandidateScratch, 0, dropCandidateCount, nonCoreDropComparer);
                }

                int keptSegmentBudget = candidateSegments;
                for (int i = 0; i < dropCandidateCount && keptSegmentBudget > MaxTotalSegments; i++)
                {
                    int index = dropCandidateScratch[i];
                    if (!keepLightScratch[index]) continue;
                    keepLightScratch[index] = false;
                    keptSegmentBudget -= preparedLightScratch[index].SegmentCount;
                    droppedLights++;
                }
            }

            int activeLights = candidateLights - droppedLights;
            if (activeLights <= 0)
            {
                snapshot = ShadowFrameSnapshot.Empty;
                ShadowRenderTelemetry.RecordBuild(candidateLights, 0, droppedLights, 0, 0, cappedLights);
                return;
            }

            ShadowLightSnapshot[] lights = new ShadowLightSnapshot[activeLights];
            int write = 0;
            int totalSegments = 0;
            int maxSegments = 0;
            for (int i = 0; i < lightCount; i++)
            {
                if (!keepLightScratch[i]) continue;

                PreparedShadowLight prepared = preparedLightScratch[i];
                int segmentCount = TileShadowOccluderBuilder.BuildSegments(prepared.TileBounds, segmentScratch, MaxSegmentsPerLight, out _);
                if (segmentCount <= 0) continue;

                ShadowVertex[] vertices = new ShadowVertex[segmentCount * 4];
                short[] indices = new short[segmentCount * 6];
                TileShadowOccluderBuilder.BuildMesh(segmentScratch, vertices, indices, out int vertexCount, out int indexCount);
                lights[write++] = new ShadowLightSnapshot(prepared.Position, prepared.RadiusTiles, vertices, vertexCount, indices, indexCount);
                totalSegments += segmentCount;
                if (segmentCount > maxSegments) maxSegments = segmentCount;
            }

            if (write != lights.Length)
            {
                Array.Resize(ref lights, write);
                activeLights = write;
            }

            if (activeLights <= 0)
            {
                snapshot = ShadowFrameSnapshot.Empty;
                ShadowRenderTelemetry.RecordBuild(candidateLights, 0, droppedLights, 0, 0, cappedLights);
                return;
            }

            snapshot = new ShadowFrameSnapshot(lights, Camera.Camera.WorldRectangle);
            ShadowRenderTelemetry.RecordBuild(candidateLights, activeLights, droppedLights, totalSegments, maxSegments, cappedLights);
        }

        readonly struct PreparedShadowLight
        {
            public PreparedShadowLight(WorldSpace position, float radiusTiles, Rectangle tileBounds, bool isCoreLight, float distanceToPlayer, int segmentCount)
            {
                Position = position;
                RadiusTiles = radiusTiles;
                TileBounds = tileBounds;
                IsCoreLight = isCoreLight;
                DistanceToPlayer = distanceToPlayer;
                SegmentCount = segmentCount;
            }

            public WorldSpace Position { get; }
            public float RadiusTiles { get; }
            public Rectangle TileBounds { get; }
            public bool IsCoreLight { get; }
            public float DistanceToPlayer { get; }
            public int SegmentCount { get; }
        }

        sealed class NonCoreDropComparer : IComparer<int>
        {
            public int Compare(int left, int right)
            {
                PreparedShadowLight lhs = preparedLightScratch[left];
                PreparedShadowLight rhs = preparedLightScratch[right];

                int distanceCompare = rhs.DistanceToPlayer.CompareTo(lhs.DistanceToPlayer);
                if (distanceCompare != 0) return distanceCompare;

                int segmentCompare = rhs.SegmentCount.CompareTo(lhs.SegmentCount);
                if (segmentCompare != 0) return segmentCompare;

                return right.CompareTo(left);
            }
        }
    }

    internal sealed class ShadowFrameSnapshot
    {
        public static readonly ShadowFrameSnapshot Empty = new ShadowFrameSnapshot(Array.Empty<ShadowLightSnapshot>(), Rectangle.Empty);

        public ShadowFrameSnapshot(ShadowLightSnapshot[] lights, Rectangle worldBounds)
        {
            Lights = lights ?? Array.Empty<ShadowLightSnapshot>();
            WorldBounds = worldBounds;
        }

        public ShadowLightSnapshot[] Lights { get; }
        public Rectangle WorldBounds { get; }
        public int Count => Lights.Length;
    }

    internal readonly struct ShadowLightSnapshot
    {
        public ShadowLightSnapshot(WorldSpace position, float radiusTiles, ShadowVertex[] vertices, int vertexCount, short[] indices, int indexCount)
        {
            Position = position;
            RadiusTiles = radiusTiles;
            Vertices = vertices ?? Array.Empty<ShadowVertex>();
            VertexCount = Math.Max(0, vertexCount);
            Indices = indices ?? Array.Empty<short>();
            IndexCount = Math.Max(0, indexCount);
        }

        public WorldSpace Position { get; }
        public float RadiusTiles { get; }
        public ShadowVertex[] Vertices { get; }
        public int VertexCount { get; }
        public short[] Indices { get; }
        public int IndexCount { get; }

        public static ShadowLightSnapshot Empty(WorldSpace position, float radiusTiles)
        {
            return new ShadowLightSnapshot(position, radiusTiles, Array.Empty<ShadowVertex>(), 0, Array.Empty<short>(), 0);
        }
    }
}
