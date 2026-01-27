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
using Project_1.GameObjects.Entities.Friendlies.GuildMembers;
using Project_1.GameObjects.Entities.Friendlies.Players;
using Project_1.GameObjects.Entities.Friendlies.Npcs;

namespace Project_1.GameObjects
{
    internal static class ObjectManager
    {
        public const float DistanceOfCircleAroundPlayer = 700;
        public static Player Player { get => player; }

        static volatile Entity[] renderAll = Array.Empty<Entity>();
        static volatile Entity[] renderEntities = Array.Empty<Entity>();
        static volatile Npc[] renderNpcs = Array.Empty<Npc>();
        static volatile Player renderPlayer;

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
        static GuildMember GetClosestGuildMember() => entities.MinBy(x => x.DistanceTo(player.FeetPosition)) as GuildMember;
        static GuildMember[] GuildMembersInWorld => guild.Where(x => entities.Contains(x)).ToArray();

        static Player player = null;
        static bool initialized;

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

                tiles = TileManager.GetTilesAroundPosition(Player.FeetPosition, start);
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
            Entity[] all = renderAll;
            for (int i = 0; i < all.Length; i++)
            {
                all[i].MinimapDraw(aBatch, aOrigin, aMinimapOffset, aMinimapSize);
            }
        }

        public static void Draw(SpriteBatch aSpriteBatch)
        {
            ThreadAffinity.AssertMainThread();
            Player p = renderPlayer;
            if (p != null)
            {
                p.Draw(aSpriteBatch);
            }
            Entity[] ents = renderEntities;
            for (int i = 0; i < ents.Length; i++)
            {
                ents[i].Draw(aSpriteBatch);
            }

            Npc[] snapshotNpcs = renderNpcs;
            for (int i = 0; i < snapshotNpcs.Length; i++)
            {
                snapshotNpcs[i].Draw(aSpriteBatch);
            }
        }

        internal static void BuildRenderSnapshot()
        {
            ThreadAffinity.AssertSimThread();
            renderPlayer = player;
            renderEntities = entities.ToArray();
            renderNpcs = npcs.ToArray();
            renderAll = entities.Union(guild).Concat(npcs).Append(player).Where(x => x != null).ToArray();
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
