using Microsoft.Xna.Framework;
using Project_1.Camera;
using Project_1.GameObjects;
using Project_1.GameObjects.Spawners;
using System;
using System.Collections.Generic;
using Project_1.GameObjects.Entities.Friendlies.Players;

namespace Project_1.Managers
{
    internal readonly struct MinimapDotSnapshot
    {
        public MinimapDotSnapshot(WorldSpace position, Color color, bool isPlayer = false)
        {
            Position = position;
            Color = color;
            IsPlayer = isPlayer;
        }

        public WorldSpace Position { get; }
        public Color Color { get; }
        public bool IsPlayer { get; }
    }

    internal static class MinimapSnapshotManager
    {
        const float minimapOriginSnapStep = 1f; // world units

        sealed class SnapshotData
        {
            MinimapDotSnapshot[] dots;

            public SnapshotData(int initialCapacity)
            {
                dots = initialCapacity > 0 ? new MinimapDotSnapshot[initialCapacity] : Array.Empty<MinimapDotSnapshot>();
            }

            public MinimapDotSnapshot[] Dots => dots;
            public int DotCount { get; private set; }
            public WorldSpace Origin { get; private set; }
            public bool OriginValid { get; private set; }

            public void Set(List<MinimapDotSnapshot> source, WorldSpace origin, bool originValid)
            {
                int count = source?.Count ?? 0;
                EnsureCapacity(count);
                for (int i = 0; i < count; i++)
                {
                    dots[i] = source[i];
                }

                DotCount = count;
                Origin = origin;
                OriginValid = originValid;
            }

            void EnsureCapacity(int count)
            {
                if (count <= dots.Length) return;
                int newCapacity = Math.Max(count, Math.Max(16, dots.Length * 2));
                dots = new MinimapDotSnapshot[newCapacity];
            }
        }

        static readonly List<MinimapDotSnapshot> dotsScratch = new List<MinimapDotSnapshot>(64);
        static readonly SnapshotData snapshotA = new SnapshotData(64);
        static readonly SnapshotData snapshotB = new SnapshotData(64);
        static volatile SnapshotData snapshot = snapshotA;

        public static bool TryGetSnapshot(out MinimapDotSnapshot[] dots, out int dotCount, out WorldSpace minimapOrigin)
        {
            ThreadAffinity.AssertMainThread();
            SnapshotData local = snapshot;
            dots = local.Dots;
            dotCount = local.DotCount;
            minimapOrigin = local.Origin;
            return local.OriginValid;
        }

        public static void BuildSnapshot()
        {
            ThreadAffinity.AssertSimThread();
            dotsScratch.Clear();
            ObjectManager.AppendMinimapDots(dotsScratch);
            SpawnerManager.AppendMinimapDots(dotsScratch);
            WorldSpace origin;
            bool originValid;
            if (ObjectManager.Player != null)
            {
                origin = NormalizeOrigin(ObjectManager.Player.FeetPosition);
                originValid = true;
            }
            else
            {
                origin = WorldSpace.Zero;
                originValid = false;
            }

            SnapshotData writeTarget = ReferenceEquals(snapshot, snapshotA) ? snapshotB : snapshotA;
            writeTarget.Set(dotsScratch, origin, originValid);
            snapshot = writeTarget;
        }

        static WorldSpace NormalizeOrigin(WorldSpace origin)
        {
            float inv = 1f / minimapOriginSnapStep;
            float snappedX = MathF.Round(origin.X * inv) / inv;
            float snappedY = MathF.Round(origin.Y * inv) / inv;
            return new WorldSpace(snappedX, snappedY);
        }
    }
}
