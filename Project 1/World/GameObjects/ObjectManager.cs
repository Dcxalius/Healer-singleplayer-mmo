using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Project_1.Camera;
using Project_1.GameObjects.Entities;
using Project_1.GameObjects.Entities.Friendlies.GuildMembers;
using Project_1.GameObjects.Entities.Friendlies.Npcs;
using Project_1.GameObjects.Entities.Friendlies.Players;
using Project_1.Managers;
using Project_1.System.Models.BaseModels;

namespace Project_1.GameObjects
{
    internal static partial class ObjectManager
    {
        public const float DistanceOfCircleAroundPlayer = 700;
        public static Player Player => player;

        static readonly RenderCache<EntityRenderSnapshot> renderPlayers = new RenderCache<EntityRenderSnapshot>();
        static readonly RenderCache<EntityRenderSnapshot> renderEntities = new RenderCache<EntityRenderSnapshot>();
        static readonly RenderCache<EntityRenderSnapshot> renderNpcs = new RenderCache<EntityRenderSnapshot>();
        static readonly RenderCache<EntityModelRenderSnapshot> renderModelEntities = new RenderCache<EntityModelRenderSnapshot>();
        static readonly HashSet<int> knownPlayerIds = new HashSet<int>();
        static readonly HashSet<int> currentPlayerIds = new HashSet<int>();
        static readonly HashSet<int> knownEntityIds = new HashSet<int>();
        static readonly HashSet<int> currentEntityIds = new HashSet<int>();
        static readonly HashSet<int> knownNpcIds = new HashSet<int>();
        static readonly HashSet<int> currentNpcIds = new HashSet<int>();
        static readonly HashSet<int> knownModelIds = new HashSet<int>();
        static readonly HashSet<int> currentModelIds = new HashSet<int>();
        static volatile LightSnapshot renderLightSnapshot = LightSnapshot.Empty;
        static readonly int[] lightRenderIdScratch = new int[LightSnapshot.MaxLights];
        static readonly WorldSpace[] lightPositionScratch = new WorldSpace[LightSnapshot.MaxLights];
        static readonly float[] lightRadiusTilesScratch = new float[LightSnapshot.MaxLights];
        static readonly bool[] lightCoreScratch = new bool[LightSnapshot.MaxLights];
        static readonly float[] lightDistanceToPlayerScratch = new float[LightSnapshot.MaxLights];
        static readonly List<Entity> allScratch = new List<Entity>();
        static readonly HashSet<Entity> allScratchSet = new HashSet<Entity>();

        static double timer;

        static List<Entity> entities;
        static List<GuildMember> guild;
        static List<Npc> npcs;
        static Player player;
        static bool initialized;

        public sealed class LightSnapshot
        {
            public const int MaxLights = 16;
            public const float NearbyEmitterRangeTiles = 40f;
            public static readonly LightSnapshot Empty = new LightSnapshot(Array.Empty<WorldSpace>(), Array.Empty<float>(), Array.Empty<bool>(), Array.Empty<float>(), Point.Zero);

            LightSnapshot(WorldSpace[] positions, float[] radiusTiles, bool[] isCoreLights, float[] distanceToPlayerWorld, Point originTile)
            {
                Positions = positions ?? Array.Empty<WorldSpace>();
                RadiusTiles = radiusTiles ?? Array.Empty<float>();
                IsCoreLights = isCoreLights ?? Array.Empty<bool>();
                DistanceToPlayerWorld = distanceToPlayerWorld ?? Array.Empty<float>();
                OriginTile = originTile;
            }

            public int Count => Positions.Length;
            public WorldSpace[] Positions { get; }
            public float[] RadiusTiles { get; }
            public bool[] IsCoreLights { get; }
            public float[] DistanceToPlayerWorld { get; }
            public Point OriginTile { get; }

            public WorldSpace GetPosition(int index) => Positions[index];
            public float GetRadiusTiles(int index) => RadiusTiles[index];

            public bool IsCoreLight(int index)
            {
                if (index < 0 || index >= IsCoreLights.Length) return false;
                return IsCoreLights[index];
            }

            public float GetDistanceToPlayerWorld(int index)
            {
                if (index < 0 || index >= DistanceToPlayerWorld.Length) return float.MaxValue;
                return DistanceToPlayerWorld[index];
            }

            public static LightSnapshot CreateFromScratch(int count, WorldSpace[] positions, float[] radiusTiles, bool[] isCoreLights, float[] distanceToPlayerWorld, Point originTile)
            {
                int clampedCount = Math.Clamp(count, 0, MaxLights);
                if (clampedCount == 0) return Empty;

                WorldSpace[] positionCopy = new WorldSpace[clampedCount];
                float[] radiusCopy = new float[clampedCount];
                bool[] coreCopy = new bool[clampedCount];
                float[] distanceCopy = new float[clampedCount];
                Array.Copy(positions, positionCopy, clampedCount);
                Array.Copy(radiusTiles, radiusCopy, clampedCount);
                Array.Copy(isCoreLights, coreCopy, clampedCount);
                Array.Copy(distanceToPlayerWorld, distanceCopy, clampedCount);
                return new LightSnapshot(positionCopy, radiusCopy, coreCopy, distanceCopy, originTile);
            }
        }

        static List<Entity> BuildAllScratch()
        {
            allScratch.Clear();
            allScratchSet.Clear();

            for (int i = 0; i < entities.Count; i++)
            {
                Entity entity = entities[i];
                if (entity == null) continue;
                allScratch.Add(entity);
                allScratchSet.Add(entity);
            }

            for (int i = 0; i < guild.Count; i++)
            {
                GuildMember member = guild[i];
                if (member == null) continue;
                if (!allScratchSet.Add(member)) continue;
                allScratch.Add(member);
            }

            for (int i = 0; i < npcs.Count; i++)
            {
                Npc npc = npcs[i];
                if (npc == null) continue;
                allScratch.Add(npc);
            }

            if (player != null)
            {
                allScratch.Add(player);
            }

            return allScratch;
        }
    }
}
