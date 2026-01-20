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
        static volatile MinimapDotSnapshot[] dots = Array.Empty<MinimapDotSnapshot>();
        static WorldSpace origin;
        static volatile bool originValid;

        public static MinimapDotSnapshot[] Snapshot
        {
            get
            {
                ThreadAffinity.AssertMainThread();
                return dots;
            }
        }

        public static bool TryGetOrigin(out WorldSpace minimapOrigin)
        {
            ThreadAffinity.AssertMainThread();
            minimapOrigin = origin;
            return originValid;
        }

        public static void BuildSnapshot()
        {
            ThreadAffinity.AssertSimThread();
            List<MinimapDotSnapshot> list = new List<MinimapDotSnapshot>();
            ObjectManager.AppendMinimapDots(list);
            SpawnerManager.AppendMinimapDots(list);
            dots = list.ToArray();
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
        }
    }
}
