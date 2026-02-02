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
        static volatile PartyLightSnapshot renderLightSnapshot = PartyLightSnapshot.Empty;

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
        static GuildMember GetClosestGuildMember() => entities.MinBy(x => x.DistanceTo(player.FeetPosition)) as GuildMember;
        static GuildMember[] GuildMembersInWorld => guild.Where(x => entities.Contains(x)).ToArray();

        static Player player = null;
        static bool initialized;

        public sealed class PartyLightSnapshot
        {
            public static readonly PartyLightSnapshot Empty = new PartyLightSnapshot(Array.Empty<WorldSpace>(), Point.Zero);

            public PartyLightSnapshot(WorldSpace[] positions, Point originTile)
            {
                Positions = positions ?? Array.Empty<WorldSpace>();
                OriginTile = originTile;
            }

            public WorldSpace[] Positions { get; }
            public Point OriginTile { get; }
        }

        static List<Entity> All
        {
            get
            {
                var r = entities.Union(guild).ToList();
                r.AddRange(npcs);
                r.Add(player);
                return r;
            }
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
            for (int i = All.Count - 1; i >= 0; i--)
            {
                All[i].Update();
            }

            LootState.Update();
            PublishPlayerUiSnapshot();

            if (TimeManager.TotalFrameTime % 2000 < 1) //TODO: This can cause issues at lower framerate
            {
                for (int i = 0; i < All.Count; i++)
                {
                    All[i].ServerTick();
                }
            }
        }

        public static void RefreshPlates()
        {
            ThreadAffinity.AssertSimThread();
            for (int i = 0; i < All.Count; i++)
            {
                All[i].RefreshPlates();
            }
        }

        public static void CreateNewPlayer(string aName, string aClass)
        {
            ThreadAffinity.AssertSimThread();
            Reset();
            player = new Player(aName, aClass);
            ObjectFactory.PlayerData = player.PlayerData;
            Camera.Camera.BindCamera(player);
        }
        public static void RemoveEntity(Entity aObject) => entities.Remove(aObject);

        

        

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
            Mailboxes.Ui.Publish(new GuildMemberAdded(guild.Last()));
        }

        public static void Reset()
        {
            ThreadAffinity.AssertSimThread();
            for (int i = guild.Count - 1; i >= 0; i--)
            {
                guild[i].Delete();
            }
            Mailboxes.Ui.Publish(new PartyCleared());
            entities.Clear();
            guild.Clear();
            npcs.Clear();
            CorpseManager.Reset();
            FloatingTextManager.Reset();
            ParticleManager.Reset();
            renderPlayers.RequestClear();
            renderEntities.RequestClear();
            renderNpcs.RequestClear();
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

        public static void MinimapDraw(SpriteBatch aBatch, WorldSpace aOrigin, AbsoluteScreenPosition aMinimapOffset, AbsoluteScreenPosition aMinimapSize)
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

        public static void Draw(SpriteBatch aSpriteBatch)
        {
            ThreadAffinity.AssertMainThread();
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

        public static PartyLightSnapshot RenderLightSnapshot => renderLightSnapshot;

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

                WorldSpace[] positions = player.Party.GetPositions;
                Point originTile = positions.Length > 0 ? TileManager.GetGridPos(positions[0]) : Point.Zero;
                renderLightSnapshot = new PartyLightSnapshot(positions, originTile);
            }
            else
            {
                renderLightSnapshot = PartyLightSnapshot.Empty;
            }

            PublishRemovals(renderPlayers, knownPlayerIds, currentPlayerIds);
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
                Mailboxes.Ui.Publish(new PlayerUiSnapshot(false, false, 0, true, 1, WorldSpace.Zero, false, WorldSpace.Zero));
                return;
            }

            Entity target = player.Target;
            Mailboxes.Ui.Publish(new PlayerUiSnapshot(
                true,
                player.InCombatOrPartyInCombat,
                player.Gold,
                player.OffGlobalCooldown,
                player.RatioOfGlobalCooldownDone,
                player.FeetPosition,
                target != null,
                target?.FeetPosition ?? WorldSpace.Zero));
        }

        internal static void AppendMinimapDots(List<MinimapDotSnapshot> dots)
        {
            ThreadAffinity.AssertSimThread();
            if (dots == null) return;
            if (player != null)
            {
                dots.Add(new MinimapDotSnapshot(player.FeetPosition, player.MinimapColor));
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
