using Microsoft.Xna.Framework.Graphics;
using Project_1.Messaging;
using Project_1.Messaging.Events;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using System.Reflection.Metadata;
using Microsoft.Xna.Framework.Content;
using Project_1.Managers;
using Project_1.GameObjects.Entities;
using Project_1.Camera;
using Project_1.GameObjects.Spawners;
using System.Diagnostics;
using Project_1.Tiles;
using Project_1.Managers.Saves;
using Project_1.GameObjects.FloatingTexts;
using Project_1.GameObjects.Entities.Corspes;
using Project_1.GameObjects.Entities.Projectiles;
using Project_1.GameObjects.Unit;
using Project_1.UI.HUD.Managers;
using Project_1.Particles;
using Project_1.GameObjects.Entities.Friendlies;
using Project_1.GameObjects.Entities.Friendlies.GuildMembers;
using Project_1.GameObjects.Entities.Friendlies.Players;
using Project_1.GameObjects.Entities.Friendlies.Npcs;
using Project_1.GameObjects.Spells;
using Microsoft.Xna.Framework;

namespace Project_1.GameObjects
{
    internal static class ObjectManager
    {
        public const float DistanceOfCircleAroundPlayer = 700;
        public static Player Player { get => player; }

        static readonly RenderCache<EntityRenderSnapshot> renderPlayers = new RenderCache<EntityRenderSnapshot>();
        static readonly RenderCache<EntityRenderSnapshot> renderEntities = new RenderCache<EntityRenderSnapshot>();
        static readonly RenderCache<EntityRenderSnapshot> renderNpcs = new RenderCache<EntityRenderSnapshot>();
        static readonly HashSet<int> knownPlayerIds = new HashSet<int>();
        static readonly HashSet<int> currentPlayerIds = new HashSet<int>();
        static readonly HashSet<int> knownEntityIds = new HashSet<int>();
        static readonly HashSet<int> currentEntityIds = new HashSet<int>();
        static readonly HashSet<int> knownNpcIds = new HashSet<int>();
        static readonly HashSet<int> currentNpcIds = new HashSet<int>();
        static volatile LightSnapshot renderLightSnapshot = LightSnapshot.Empty;
        static readonly int[] lightRenderIdScratch = new int[LightSnapshot.MaxLights];
        static readonly WorldSpace[] lightPositionScratch = new WorldSpace[LightSnapshot.MaxLights];
        static readonly float[] lightRadiusTilesScratch = new float[LightSnapshot.MaxLights];
        static readonly bool[] lightCoreScratch = new bool[LightSnapshot.MaxLights];
        static readonly float[] lightDistanceToPlayerScratch = new float[LightSnapshot.MaxLights];
        static readonly List<Entity> allScratch = new List<Entity>();
        static readonly HashSet<Entity> allScratchSet = new HashSet<Entity>();

        static double timer = 0; //TODO: Save and load this

        static List<Entity> entities;
        static List<GuildMember> guild;
        static List<Npc> npcs;

        public static GuildMember[] GetGuildMembers()
        {
            ThreadAffinity.AssertSimThread();
            if (guild == null || guild.Count == 0) return Array.Empty<GuildMember>();
            return guild.ToArray();
        }

        public static Entity[] GetEntitiesSnapshot()
        {
            ThreadAffinity.AssertSimThread();
            if (entities == null || entities.Count == 0) return Array.Empty<Entity>();
            return entities.ToArray();
        }

        public static Entity[] GetAllEntitiesSnapshot()
        {
            ThreadAffinity.AssertSimThread();
            List<Entity> all = BuildAllScratch();
            if (all == null || all.Count == 0) return Array.Empty<Entity>();
            return all.ToArray();
        }

        public static bool TryGetEntityByRenderId(int renderId, out Entity entity)
        {
            ThreadAffinity.AssertSimThread();
            entity = null;
            if (renderId <= 0) return false;

            if (player != null && player.RenderId == renderId)
            {
                entity = player;
                return true;
            }

            for (int i = 0; i < entities.Count; i++)
            {
                if (entities[i].RenderId != renderId) continue;
                entity = entities[i];
                return true;
            }

            for (int i = 0; i < guild.Count; i++)
            {
                if (guild[i].RenderId != renderId) continue;
                entity = guild[i];
                return true;
            }

            for (int i = 0; i < npcs.Count; i++)
            {
                if (npcs[i].RenderId != renderId) continue;
                entity = npcs[i];
                return true;
            }

            return false;
        }

        public static bool TryGetGuildMemberByRenderId(int renderId, out GuildMember member)
        {
            ThreadAffinity.AssertSimThread();
            member = null;
            if (renderId <= 0) return false;

            for (int i = 0; i < guild.Count; i++)
            {
                if (guild[i].RenderId != renderId) continue;
                member = guild[i];
                return true;
            }

            return false;
        }

        public static bool TryGetFriendlyByRenderId(int renderId, out Friendly friendly)
        {
            ThreadAffinity.AssertSimThread();
            friendly = null;
            if (renderId <= 0) return false;

            if (player != null && player.RenderId == renderId)
            {
                friendly = player;
                return true;
            }

            for (int i = 0; i < guild.Count; i++)
            {
                if (guild[i].RenderId != renderId) continue;
                friendly = guild[i];
                return true;
            }

            for (int i = 0; i < npcs.Count; i++)
            {
                if (npcs[i].RenderId != renderId) continue;
                friendly = npcs[i];
                return true;
            }

            return false;
        }

        public static bool TryGetFriendlyByName(string name, out Friendly friendly)
        {
            ThreadAffinity.AssertSimThread();
            friendly = null;
            if (string.IsNullOrWhiteSpace(name)) return false;

            if (player != null && string.Equals(player.Name, name, StringComparison.OrdinalIgnoreCase))
            {
                friendly = player;
                return true;
            }

            for (int i = 0; i < guild.Count; i++)
            {
                if (!string.Equals(guild[i].Name, name, StringComparison.OrdinalIgnoreCase)) continue;
                friendly = guild[i];
                return true;
            }

            for (int i = 0; i < npcs.Count; i++)
            {
                if (!string.Equals(npcs[i].Name, name, StringComparison.OrdinalIgnoreCase)) continue;
                friendly = npcs[i];
                return true;
            }

            return false;
        }
        static GuildMember GetClosestGuildMember() => entities.MinBy(x => x.DistanceTo(player.FeetPosition)) as GuildMember;
        static GuildMember[] GuildMembersInWorld => guild.Where(x => entities.Contains(x)).ToArray();

        static Player player = null;
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

            public WorldSpace GetPosition(int index)
            {
                return Positions[index];
            }

            public float GetRadiusTiles(int index)
            {
                return RadiusTiles[index];
            }

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

            // Match the old entities.Union(guild) behavior: dedupe guild members against entities.
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

        public static void Init()
        {
            ThreadAffinity.AssertMainThread();
            if (initialized) return;
            initialized = true;
            entities = new List<Entity>();
            guild = new List<GuildMember>();
            npcs = new List<Npc>();
        }

        public static void Update()
        {
            ThreadAffinity.AssertSimThread();
            List<Entity> all = BuildAllScratch();
            for (int i = all.Count - 1; i >= 0; i--)
            {
                all[i].Update();
            }

            LootState.Update();
            PublishPlayerUiSnapshot();
            timer += TimeManager.MilisecondSinceLastFrame;
            if (timer >= 2000)
            {
                timer -= 2000;
                for (int i = 0; i < all.Count; i++)
                {
                    all[i].ServerTick();
                }
            }
        }

        public static void RefreshPlates()
        {
            ThreadAffinity.AssertSimThread();
            List<Entity> all = BuildAllScratch();
            for (int i = 0; i < all.Count; i++)
            {
                all[i].RefreshPlates();
            }
        }

        public static void CreateNewPlayer(string aName, string aClass)
        {
            ThreadAffinity.AssertSimThread();
            Reset();
            WorldSpace spawnPoint = TileManager.FindSpawnPointNearChunkLevel(1);
            PlayerData playerData = new PlayerData(aName, aClass);
            playerData.Position = spawnPoint;
            playerData.Momentum = WorldSpace.Zero;
            playerData.Velocity = WorldSpace.Zero;
            player = new Player(playerData);
            ObjectFactory.PlayerData = player.PlayerData;
            Camera.Camera.BindCamera(player);
        }
        public static void RemoveEntity(Entity aObject)
        {
            ThreadAffinity.AssertSimThread();
            entities.Remove(aObject);
        }

        

        

        public static void Load(Save aSave)
        {
            ThreadAffinity.AssertSimThread();
            ObjectFactory.Load(aSave);
            Reset();
            guild.AddRange(ObjectFactory.GetGuildMemebers());
            
            player = new Player(ObjectFactory.PlayerData);
            player.GetPartyMembersFromGuild();
            Camera.Camera.BindCamera(player);
            npcs.AddRange(ObjectFactory.CreateNpcs());

        }

        public static void LoadFromFactoryData()
        {
            ThreadAffinity.AssertSimThread();
            Reset();
            guild.AddRange(ObjectFactory.GetGuildMemebers());

            if (ObjectFactory.PlayerData != null)
            {
                player = new Player(ObjectFactory.PlayerData);
                player.GetPartyMembersFromGuild();
                Camera.Camera.BindCamera(player);
            }

            npcs.AddRange(ObjectFactory.CreateNpcs());
        }

        public static void CreateNewGuildMember()
        {
            ThreadAffinity.AssertSimThread();
            ObjectFactory.AddGuildMember("xdddd", "Rogue");
            guild = ObjectFactory.GetGuildMemebers();
            Mailboxes.PublishUiEvent(new GuildMemberAdded(guild.Last().BuildUiSnapshot()));
        }

        public static void Reset()
        {
            ThreadAffinity.AssertSimThread();
            for (int i = guild.Count - 1; i >= 0; i--)
            {
                guild[i].Delete();
            }
            Mailboxes.PublishUiEvent(new PartyCleared());
            entities.Clear();
            guild.Clear();
            npcs.Clear();
            CorpseManager.Reset();
            FloatingTextManager.Reset();
            ParticleManager.Reset();
            renderPlayers.RequestClear();
            renderEntities.RequestClear();
            renderNpcs.RequestClear();
            renderLightSnapshot = LightSnapshot.Empty;
            if (player != null) player.Delete();
        }

        #region Party/Guild
        public static GuildMember FriendlyTargetCycle()
        {
            ThreadAffinity.AssertSimThread();

            if (player.Target == null) return GetClosestGuildMember();
            if (player.Target.GetType() != typeof(GuildMember)) return GetClosestGuildMember();
            return entities.Where(x => x.DistanceTo(player.FeetPosition) > player.Target.DistanceTo(player.FeetPosition)).MinBy(x => x.DistanceTo(player.FeetPosition)) as GuildMember;

        }

        public static bool SpawnGuildMemberToParty(GuildMember aMember, WorldSpace? aPosition)
        {
            ThreadAffinity.AssertSimThread();
            Debug.Assert(guild.Contains(aMember));
            Debug.Assert(!player.Party.IsInParty(aMember));

            WorldSpace position = ( aPosition ??= FindTileAroundPlayer());

            aMember.Teleport(position);
            entities.Add(guild.Find(member => member == aMember));
            return player.Party.AddToParty(entities[entities.Count - 1] as GuildMember);
        }

        public static bool RemoveGuildMemberFromParty(GuildMember aMember)
        {
            ThreadAffinity.AssertSimThread();
            Debug.Assert(guild.Contains(aMember));
            Debug.Assert(player.Party.IsInParty(aMember));

            WorldSpace position = FindTileAroundPlayer();
            aMember.RecieveDirectWalkingOrder(position);
            return player.Party.RemoveFromParty(aMember);
        }

        static WorldSpace FindTileAroundPlayer()
        {
            Tile[] tiles = new Tile[0];
            float start = DistanceOfCircleAroundPlayer; //TODO: Find better way to get these values relating to max camera distance
            float step = 50;
            while (tiles.Length == 0)
            {

                tiles = TileQuery.GetTilesAroundPosition(Player.FeetPosition, start);
                start -= step;
                Debug.Assert(start > 0);
            }
            Tile tile = tiles[RandomManager.RollInt(tiles.Length)];
            //tile = tiles[tiles.Length - 1];
            return tile.Position;
        }
        #endregion

        #region HitTest
        public static bool TryGetEntityAt(WorldSpace worldPos, out Entity entity)
        {
            ThreadAffinity.AssertSimThread();
            entity = null;
            if (player != null && player.WorldRectangle.Contains(worldPos.ToPoint()))
            {
                entity = player;
                return true;
            }

            for (int i = 0; i < entities.Count; i++)
            {
                if (!entities[i].WorldRectangle.Contains(worldPos.ToPoint())) continue;
                entity = entities[i];
                return true;
            }

            for (int i = 0; i < npcs.Count; i++)
            {
                if (!npcs[i].WorldRectangle.Contains(worldPos.ToPoint())) continue;
                entity = npcs[i];
                return true;
            }

            return false;
        }
        #endregion

        internal static void DrawMinimapSnapshots(SpriteBatch aBatch, WorldSpace aOrigin, AbsoluteScreenPosition aMinimapOffset, AbsoluteScreenPosition aMinimapSize)
        {
            ThreadAffinity.AssertMainThread();
            ApplyRenderUpdates();
            foreach (EntityRenderSnapshot snapshot in renderPlayers.Values)
            {
                snapshot.DrawMinimap(aBatch, aOrigin, aMinimapOffset, aMinimapSize);
            }
            foreach (EntityRenderSnapshot snapshot in renderEntities.Values)
            {
                snapshot.DrawMinimap(aBatch, aOrigin, aMinimapOffset, aMinimapSize);
            }
            foreach (EntityRenderSnapshot snapshot in renderNpcs.Values)
            {
                snapshot.DrawMinimap(aBatch, aOrigin, aMinimapOffset, aMinimapSize);
            }
        }

        internal static void DrawSnapshots(SpriteBatch aSpriteBatch)
        {
            ThreadAffinity.AssertMainThread();
            // Snapshot-only draw path. Do not read live sim lists here.
            ApplyRenderUpdates();
            foreach (EntityRenderSnapshot snapshot in renderPlayers.Values)
            {
                snapshot.Draw(aSpriteBatch);
            }
            foreach (EntityRenderSnapshot snapshot in renderEntities.Values)
            {
                snapshot.Draw(aSpriteBatch);
            }
            foreach (EntityRenderSnapshot snapshot in renderNpcs.Values)
            {
                snapshot.Draw(aSpriteBatch);
            }
        }

        public static LightSnapshot RenderLightSnapshot => renderLightSnapshot;

        internal static void BuildRenderSnapshot()
        {
            ThreadAffinity.AssertSimThread();
            PublishPlayerSnapshot();
            PublishEntitySnapshots(entities, renderEntities, knownEntityIds, currentEntityIds);
            PublishEntitySnapshots(npcs, renderNpcs, knownNpcIds, currentNpcIds);
        }

        static void ApplyRenderUpdates()
        {
            renderPlayers.ApplyUpdates();
            renderEntities.ApplyUpdates();
            renderNpcs.ApplyUpdates();
        }

        static void PublishPlayerSnapshot()
        {
            currentPlayerIds.Clear();
            if (player != null)
            {
                EntityRenderSnapshot snapshot = player.BuildRenderSnapshot();
                renderPlayers.EnqueueUpdate(snapshot);
                currentPlayerIds.Add(snapshot.RenderId);
                renderLightSnapshot = BuildRenderLightSnapshot();
            }
            else
            {
                renderLightSnapshot = LightSnapshot.Empty;
            }

            PublishRemovals(renderPlayers, knownPlayerIds, currentPlayerIds);
        }

        static LightSnapshot BuildRenderLightSnapshot()
        {
            ThreadAffinity.AssertSimThread();
            if (player == null) return LightSnapshot.Empty;

            int count = 0;
            float nearbyRangeWorld = LightSnapshot.NearbyEmitterRangeTiles * Tile.Size.X;
            WorldSpace playerFeet = player.FeetPosition;
            Point originTile = TileManager.GetGridPos(playerFeet);

            AddEmitterIfEligible(player, true, playerFeet, nearbyRangeWorld, ref count);

            for (int i = 0; i < guild.Count && count < LightSnapshot.MaxLights; i++)
            {
                GuildMember member = guild[i];
                if (member == null) continue;
                if (!player.Party.IsInParty(member)) continue;
                AddEmitterIfEligible(member, true, playerFeet, nearbyRangeWorld, ref count);
            }

            for (int i = 0; i < entities.Count && count < LightSnapshot.MaxLights; i++)
            {
                AddEmitterIfEligible(entities[i], false, playerFeet, nearbyRangeWorld, ref count);
            }

            for (int i = 0; i < guild.Count && count < LightSnapshot.MaxLights; i++)
            {
                AddEmitterIfEligible(guild[i], false, playerFeet, nearbyRangeWorld, ref count);
            }

            for (int i = 0; i < npcs.Count && count < LightSnapshot.MaxLights; i++)
            {
                AddEmitterIfEligible(npcs[i], false, playerFeet, nearbyRangeWorld, ref count);
            }

            return LightSnapshot.CreateFromScratch(count, lightPositionScratch, lightRadiusTilesScratch, lightCoreScratch, lightDistanceToPlayerScratch, originTile);
        }

        static void AddEmitterIfEligible(Entity entity, bool forceInclude, WorldSpace playerFeet, float nearbyRangeWorld, ref int count)
        {
            ThreadAffinity.AssertSimThread();
            if (entity == null) return;
            if (count >= LightSnapshot.MaxLights) return;
            if (entity is not ILightEmitter emitter) return;

            if (!forceInclude && entity.FeetPosition.DistanceTo(playerFeet) > nearbyRangeWorld)
            {
                return;
            }

            int renderId = entity.RenderId;
            for (int i = 0; i < count; i++)
            {
                if (lightRenderIdScratch[i] != renderId) continue;
                if (forceInclude) lightCoreScratch[i] = true;
                return;
            }

            lightRenderIdScratch[count] = renderId;
            WorldSpace feet = entity.FeetPosition;
            lightPositionScratch[count] = feet;
            lightRadiusTilesScratch[count] = Math.Max(0.1f, emitter.LightRadiusTiles);
            lightCoreScratch[count] = forceInclude;
            lightDistanceToPlayerScratch[count] = feet.DistanceTo(playerFeet);
            count++;
        }

        static void PublishEntitySnapshots<T>(IList<T> source, RenderCache<EntityRenderSnapshot> cache, HashSet<int> knownIds, HashSet<int> currentIds) where T : Entity
        {
            currentIds.Clear();
            for (int i = 0; i < source.Count; i++)
            {
                Entity entity = source[i];
                if (entity == null) continue;
                EntityRenderSnapshot snapshot = entity.BuildRenderSnapshot();
                cache.EnqueueUpdate(snapshot);
                currentIds.Add(snapshot.RenderId);
            }

            PublishRemovals(cache, knownIds, currentIds);
        }

        static void PublishRemovals(RenderCache<EntityRenderSnapshot> cache, HashSet<int> knownIds, HashSet<int> currentIds)
        {
            foreach (int id in knownIds)
            {
                if (!currentIds.Contains(id))
                {
                    cache.EnqueueRemove(id);
                }
            }
            knownIds.Clear();
            foreach (int id in currentIds)
            {
                knownIds.Add(id);
            }
        }

        static void PublishPlayerUiSnapshot()
        {
            if (player == null)
            {
                Mailboxes.PublishUiEvent(new PlayerUiSnapshot(false, false, 0, true, 1, WorldSpace.Zero, false, WorldSpace.Zero, Array.Empty<SpellUiSnapshot>()));
                return;
            }

            Spell[] knownSpells = player.SpellBook.Spells;
            SpellUiSnapshot[] spellSnapshots = new SpellUiSnapshot[knownSpells.Length];
            for (int i = 0; i < knownSpells.Length; i++)
            {
                Spell spell = knownSpells[i];
                spellSnapshots[i] = new SpellUiSnapshot(spell.Name, spell.GfxPath, spell.OffCooldown, spell.RatioOfCooldownDone);
            }

            Entity target = player.Target;
            Mailboxes.PublishUiEvent(new PlayerUiSnapshot(
                true,
                player.InCombatOrPartyInCombat,
                player.Gold,
                player.OffGlobalCooldown,
                player.RatioOfGlobalCooldownDone,
                player.FeetPosition,
                target != null,
                target?.FeetPosition ?? WorldSpace.Zero,
                spellSnapshots));
        }

        internal static void AppendMinimapDots(List<MinimapDotSnapshot> dots)
        {
            ThreadAffinity.AssertSimThread();
            if (dots == null) return;
            if (player != null)
            {
                dots.Add(new MinimapDotSnapshot(player.FeetPosition, player.MinimapColor, true));
            }
            for (int i = 0; i < entities.Count; i++)
            {
                Entity entity = entities[i];
                if (entity == null) continue;
                dots.Add(new MinimapDotSnapshot(entity.FeetPosition, entity.MinimapColor));
            }
            for (int i = 0; i < guild.Count; i++)
            {
                GuildMember member = guild[i];
                if (member == null) continue;
                dots.Add(new MinimapDotSnapshot(member.FeetPosition, member.MinimapColor));
            }
            for (int i = 0; i < npcs.Count; i++)
            {
                Npc npc = npcs[i];
                if (npc == null) continue;
                dots.Add(new MinimapDotSnapshot(npc.FeetPosition, npc.MinimapColor));
            }
        }
    }
}
