using System;
using System.Linq;
using Project_1.Camera;
using Project_1.GameObjects.Entities;
using Project_1.GameObjects.Entities.Friendlies.Players;
using Project_1.GameObjects.FloatingTexts;
using Project_1.GameObjects.Entities.Corspes;
using Project_1.Managers;
using Project_1.Managers.Saves;
using Project_1.Particles;
using Project_1.Tiles;
using Project_1.Messaging;
using Project_1.Messaging.Events;

namespace Project_1.GameObjects
{
    internal static partial class ObjectManager
    {
        public static void Init()
        {
            ThreadAffinity.AssertMainThread();
            if (initialized) return;
            initialized = true;
            entities = new System.Collections.Generic.List<Entity>();
            guild = new System.Collections.Generic.List<Entities.Friendlies.GuildMembers.GuildMember>();
            npcs = new System.Collections.Generic.List<Entities.Friendlies.Npcs.Npc>();
        }

        public static void Update()
        {
            ThreadAffinity.AssertSimThread();
            System.Collections.Generic.List<Entity> all = BuildAllScratch();
            for (int i = all.Count - 1; i >= 0; i--)
            {
                all[i].Update();
            }

            LootState.Update();
            PublishPlayerUiSnapshot();
            timer += TimeManager.MilisecondSinceLastFrame;
            if (timer < 2000) return;

            timer -= 2000;
            for (int i = 0; i < all.Count; i++)
            {
                all[i].ServerTick();
            }
        }

        public static void RefreshPlates()
        {
            ThreadAffinity.AssertSimThread();
            System.Collections.Generic.List<Entity> all = BuildAllScratch();
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
            PlayerData playerData = new PlayerData(aName, aClass)
            {
                Position = spawnPoint,
                Momentum = WorldSpace.Zero,
                Velocity = WorldSpace.Zero
            };
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
            MailboxManager.PublishUiEvent(new GuildMemberAdded(guild.Last().BuildUiSnapshot()));
        }

        public static void Reset()
        {
            ThreadAffinity.AssertSimThread();
            for (int i = guild.Count - 1; i >= 0; i--)
            {
                guild[i].Delete();
            }

            MailboxManager.PublishUiEvent(new PartyCleared());
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
    }
}
