using Microsoft.Xna.Framework;
using Project_1.Camera;
using Project_1.GameObjects;
using Project_1.GameObjects.Spawners;
using System;
using System.Collections.Generic;

namespace Project_1.Managers
{
    internal readonly struct MinimapDotSnapshot
    {
        public MinimapDotSnapshot(WorldSpace position, Color color)
        {
            Position = position;
            Color = color;
        }

        public WorldSpace Position { get; }
        public Color Color { get; }
    }

    internal static class MinimapSnapshotManager
    {
        sealed class SnapshotData
        {
            public SnapshotData(MinimapDotSnapshot[] dots, WorldSpace origin, bool originValid)
            {
                Dots = dots ?? Array.Empty<MinimapDotSnapshot>();
                Origin = origin;
                OriginValid = originValid;
            }

            public MinimapDotSnapshot[] Dots { get; }
            public WorldSpace Origin { get; }
            public bool OriginValid { get; }
        }

        static volatile SnapshotData snapshot = new SnapshotData(Array.Empty<MinimapDotSnapshot>(), WorldSpace.Zero, false);

        public static MinimapDotSnapshot[] Snapshot
        {
            get
            {
                ThreadAffinity.AssertMainThread();
                return snapshot.Dots;
            }
        }

        public static bool TryGetOrigin(out WorldSpace minimapOrigin)
        {
            ThreadAffinity.AssertMainThread();
            SnapshotData local = snapshot;
            minimapOrigin = local.Origin;
            return local.OriginValid;
        }

        public static void BuildSnapshot()
        {
            ThreadAffinity.AssertSimThread();
            List<MinimapDotSnapshot> list = new List<MinimapDotSnapshot>();
            ObjectManager.AppendMinimapDots(list);
            SpawnerManager.AppendMinimapDots(list);
            WorldSpace origin;
            bool originValid;
            if (ObjectManager.Player != null)
            {
                origin = ObjectManager.Player.FeetPosition;
                originValid = true;
            }
            else
            {
                origin = WorldSpace.Zero;
                originValid = false;
            }
            snapshot = new SnapshotData(list.ToArray(), origin, originValid);
        }
    }
}
