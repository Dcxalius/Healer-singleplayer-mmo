using Microsoft.Xna.Framework.Content;
using Newtonsoft.Json;
using Project_1.Camera;
using Project_1.GameObjects.Spawners;
using Project_1.GameObjects.Unit;
using Project_1.GameObjects.Unit.Classes;
using Project_1.Managers;
using Project_1.Managers.Saves;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security;
using System.Text;
using System.Threading.Tasks;
using Project_1.GameObjects.Entities.Friendlies.GuildMembers;
using Project_1.GameObjects.Entities.Friendlies.Players;
using Project_1.GameObjects.Entities.Friendlies.Npcs;
using System.IO;

namespace Project_1.GameObjects
{
    internal static class ObjectFactory  
    {
        //TODO: Split this in two, one to handle static data like classes and one to handle dynamic data
        //TODO: Should also probably be split into lines along UnitTypes. A PlayerFactory, GuildMemberFactory, PlayerManager, GuildMemberManager etc
        //That refactor should also rework ObjectManager along similar lines and it's responsibilties broken up by the same logic
        public static PlayerData PlayerData { get => playerData; set => playerData = value; }

        static PlayerData playerData;
        static List<UnitData> guildData;
        static List<UnitData> npcData;

        static Dictionary<string, MobData> mobData;
        //static UnitData defaultData = new UnitData();

        static Dictionary<string, FriendlyClassData> playerClassData;
        static Dictionary<string, FriendlyClassData> guildMemberClassData; //TODO: Check for missed renames
        static Dictionary<string, MobClassData> npcClassData; //TODO: Check for other references to mob. Should be called npc in the future

        static Dictionary<string, GossipData> gossipData; //Q: Unsure where this should be in the afformentioned refactor.

        static bool initialized;

        public static void Init()
        {
            ThreadAffinity.AssertMainThread();
            if (initialized) return;
            initialized = true;
            guildData = new List<UnitData>();
            ImportClassData();
            ImportNpcData();
            ImportGuildMemberData();
            ImportGossipData();
        }

        public static void AddGuildMember(string aName, string aClassName)
        {
            //TODO: Should be developed further, current implementation is a fine placeholder for now
            ThreadAffinity.AssertSimThread();
            guildData ??= new List<UnitData>();
            UnitData xdd = new UnitData(aName, "", aClassName, Relation.RelationToPlayer.Friendly, 1, 0, null, float.MaxValue, float.MaxValue, null, WorldSpace.Zero, WorldSpace.Zero, WorldSpace.Zero, null, 1);

            guildData.Add(xdd);
        }

        public static void Load(Save aSave)
        {
            ThreadAffinity.AssertSimThread();
            ResetUnitData();
            ImportPlayerData(aSave);
            ImportGuildData(aSave);
        }

        public static void ApplyLoadedData(PlayerData loadedPlayer, List<UnitData> loadedGuild)
        {
            ThreadAffinity.AssertSimThread();
            ResetUnitData();
            playerData = loadedPlayer;
            guildData = loadedGuild != null ? new List<UnitData>(loadedGuild) : new List<UnitData>();
        }

        public static UnitData[] GetGuildDataSnapshot()
        {
            ThreadAffinity.AssertSimThread();
            if (guildData == null || guildData.Count == 0) return Array.Empty<UnitData>();
            return guildData.ToArray();
        }

        public static void ResetUnitData()
        {
            ThreadAffinity.AssertSimThread();
            playerData = null;
            guildData = new List<UnitData>();
        }

        public static MobData GetMobData(string aName)
        {
            ThreadAffinity.AssertGameThread();
            aName = aName.ToUpper();
            if (mobData.ContainsKey(aName))
            {
                return mobData[aName];
            }
            else
            {
                throw new NotImplementedException();
            }
        }


        public static List<GuildMember> GetGuildMemebers()
        {
            ThreadAffinity.AssertSimThread();
            List<GuildMember> returnable = new List<GuildMember>();
            if (guildData == null || guildData.Count == 0) return returnable;
            for (int i = 0; i < guildData.Count; i++)
            {
                returnable.Add(new GuildMember(guildData[i]));
            }
            return returnable;
        }

        public static ClassData GetPlayerClass(string aName)
        {
            ThreadAffinity.AssertGameThread();
            return playerClassData[aName];
        }

        public static ClassData GetAllyClass(string aName)
        {
            ThreadAffinity.AssertGameThread();
            return guildMemberClassData[aName];
        }

        public static ClassData GetMobClass(string aName)
        {
            ThreadAffinity.AssertGameThread();
            return npcClassData[aName];
        }

        public static GossipData GetGossip(string aName)
        {
            ThreadAffinity.AssertGameThread();
            return gossipData[aName];
        }
        public static Npc[] CreateNpcs()
        {
            ThreadAffinity.AssertSimThread();
            Npc[] returnable = new Npc[npcData.Count];
            for (int i = 0; i < npcData.Count; i++)
            {
                returnable[i] = new Npc(npcData[i]);
            }
            return returnable.ToArray();
        }

        static void ImportGossipData()
        {
            ThreadAffinity.AssertMainThread();
            gossipData = new Dictionary<string, GossipData>();
            string path = Game1.ContentManager.RootDirectory + "\\Data\\NpcData\\Gossip\\";

            string[] files = Directory.GetFiles(path);
            for (int i = 0; i < files.Length; i++)
            {
                string rawData = File.ReadAllText(files[i]);
                
                GossipData data = JsonConvert.DeserializeObject<GossipData>(rawData);
                gossipData.Add(SaveManager.TrimToNameOnly(files[i]), data);

            }
        }


        static void ImportNpcData()
        {
            ThreadAffinity.AssertMainThread();
            mobData = new Dictionary<string, MobData>();
            string path = Game1.ContentManager.RootDirectory + "\\Data\\MobData\\";

           
            string[] files = Directory.GetFiles(path);
            for (int i = 0; i < files.Length; i++)
            {
                string rawData = File.ReadAllText(files[i]);
                MobData data = JsonConvert.DeserializeObject<MobData>(rawData);
                mobData.Add(data.Name.ToUpper(), data);
                
            }
        }

        static void ImportGuildMemberData()
        {
            ThreadAffinity.AssertMainThread();
            npcData = new List<UnitData>();
            string path = Game1.ContentManager.RootDirectory + "\\Data\\NpcData\\";


            string[] files = Directory.GetFiles(path);
            for (int i = 0; i < files.Length; i++)
            {
                string rawData = File.ReadAllText(files[i]);
                NpcData data = JsonConvert.DeserializeObject<NpcData>(rawData);
                npcData.Add(data);

            }
        }

        static void ImportPlayerData(Save aSave)
        {
            ThreadAffinity.AssertSimThread();
            string rawData = File.ReadAllText(aSave.Units + "\\PlayerData.unit");
            playerData = JsonConvert.DeserializeObject<PlayerData>(rawData);
        }

        static void ImportGuildData(Save aSave)
        {
            ThreadAffinity.AssertSimThread();
            guildData = new List<UnitData>();

            string path = aSave.Guild + "\\";

            string[] files = Directory.GetFiles(path);


            for (int j = 0; j < files.Length; j++)
            {
                string rawData = File.ReadAllText(files[j]);
                UnitData data = JsonConvert.DeserializeObject<UnitData>(rawData);
                guildData.Add(data);
            }
        }
        
        static void ImportClassData()
        {
            ThreadAffinity.AssertMainThread();
            playerClassData = new Dictionary<string, FriendlyClassData>();
            guildMemberClassData = new Dictionary<string, FriendlyClassData>();
            npcClassData = new Dictionary<string, MobClassData>();

            string path = Game1.ContentManager.RootDirectory + "\\Data\\Class\\";

            string[] folders =
                {
                path + "Player",
                path + "Ally",
                path + "Mob"
            };

            for (int i = 0; i < folders.Length; i++)
            {
                string[] files = Directory.GetFiles(folders[i]);
                string type = folders[i].Substring(path.Length);
                for (int j = 0; j < files.Length; j++)
                {
                    string rawData = File.ReadAllText(files[j]);
                    AddToClassData(rawData, Enum.Parse<ClassData.Type>(type));
                }
            }


        }

        static void AddToClassData(string aRawData, ClassData.Type aType)
        {
            ThreadAffinity.AssertMainThread();
            switch (aType)
            {
                case ClassData.Type.Player:
                    {
                        FriendlyClassData data = JsonConvert.DeserializeObject<FriendlyClassData>(aRawData);
                        playerClassData.Add(data.Name, data);
                    }
                    break;
                case ClassData.Type.Ally:
                    {
                        FriendlyClassData data = JsonConvert.DeserializeObject<FriendlyClassData>(aRawData);
                        guildMemberClassData.Add(data.Name, data);
                    }
                    break;
                case ClassData.Type.Mob:
                    {
                        MobClassData data = JsonConvert.DeserializeObject<MobClassData>(aRawData);
                        npcClassData.Add(data.Name, data);
                    }
                    break;
                default:
                    throw new Exception("Error");
            }
        }

        public static void SaveData(Save aSave)
        {
            ThreadAffinity.AssertSimThread();

            SaveManager.ExportData(aSave.Units + "\\" + "PlayerData.unit", playerData);

            aSave.ClearFolder(aSave.Guild);

            if (guildData == null || guildData.Count == 0) return;
            for (int i = 0; i < guildData.Count; i++)
            {
                SaveManager.ExportData(aSave.Guild + "\\" + guildData[i].Name + ".unit", guildData[i]);
            }
        }
    }
}
