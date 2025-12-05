using Microsoft.Xna.Framework.Content;
using Newtonsoft.Json;
using Project_1.Camera;
using Project_1.GameObjects.Entities;
using Project_1.GameObjects.Entities.Npcs;
using Project_1.GameObjects.Entities.Players;
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

namespace Project_1.GameObjects
{
    internal static class ObjectFactory  //TODO: Split this in two, one to handle static data like classes and one to handle dynamic data
    {
        public static PlayerData PlayerData { get => playerData; set => playerData = value; }

        static PlayerData playerData;
        static List<UnitData> guildData;
        static List<UnitData> npcData;

        static Dictionary<string, MobData> mobData;
        //static UnitData defaultData = new UnitData();

        static Dictionary<string, FriendlyClassData> playerClassData;
        static Dictionary<string, FriendlyClassData> allyClassData;
        static Dictionary<string, MobClassData> mobClassData;

        static Dictionary<string, GossipData> gossipData;

        
        static ObjectFactory()
        {
            ImportClassData();
            ImportMobData();
            ImportNpcData();
            ImportGossipData();
        }

        public static void AddGuildMember(string aName, string aClassName)
        {
            UnitData xdd = new UnitData(aName, "", aClassName, Relation.RelationToPlayer.Friendly, 1, 0, float.MaxValue, float.MaxValue, null, WorldSpace.Zero, WorldSpace.Zero, WorldSpace.Zero, null, 1);

            guildData.Add(xdd);
        }

        public static void Load(Save aSave)
        {
            ResetUnitData();
            ImportPlayerData(aSave);
            ImportGuildData(aSave);
        }

        public static void ResetUnitData()
        {
            playerData = null;
            guildData?.Clear();
        }

        public static MobData GetMobData(string aName)
        {
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
            List<GuildMember> returnable = new List<GuildMember>();
            for (int i = 0; i < guildData.Count; i++)
            {
                returnable.Add(new GuildMember(guildData[i]));
            }
            return returnable;
        }

        public static ClassData GetPlayerClass(string aName) => playerClassData[aName];
        public static ClassData GetAllyClass(string aName) => allyClassData[aName];
        public static ClassData GetMobClass(string aName) => mobClassData[aName];
        public static GossipData GetGossip(string aName) => gossipData[aName];
        public static Npc[] CreateNpcs()
        {
            Npc[] returnable = new Npc[npcData.Count];
            for (int i = 0; i < npcData.Count; i++)
            {
                returnable[i] = new Npc(npcData[i]);
            }
            return returnable.ToArray();
        }

        static void ImportGossipData()
        {
            gossipData = new Dictionary<string, GossipData>();
            string path = Game1.ContentManager.RootDirectory + "\\Data\\NpcData\\Gossip\\";

            string[] files = System.IO.Directory.GetFiles(path);
            for (int i = 0; i < files.Length; i++)
            {
                string rawData = System.IO.File.ReadAllText(files[i]);
                
                GossipData data = JsonConvert.DeserializeObject<GossipData>(rawData);
                gossipData.Add(SaveManager.TrimToNameOnly(files[i]), data);

            }
        }


        static void ImportMobData()
        {
            mobData = new Dictionary<string, MobData>();
            string path = Game1.ContentManager.RootDirectory + "\\Data\\MobData\\";

           
            string[] files = System.IO.Directory.GetFiles(path);
            for (int i = 0; i < files.Length; i++)
            {
                string rawData = System.IO.File.ReadAllText(files[i]);
                MobData data = JsonConvert.DeserializeObject<MobData>(rawData);
                mobData.Add(data.Name.ToUpper(), data);
                
            }
        }

        static void ImportNpcData()
        {
            npcData = new List<UnitData>();
            string path = Game1.ContentManager.RootDirectory + "\\Data\\NpcData\\";


            string[] files = System.IO.Directory.GetFiles(path);
            for (int i = 0; i < files.Length; i++)
            {
                string rawData = System.IO.File.ReadAllText(files[i]);
                NpcData data = JsonConvert.DeserializeObject<NpcData>(rawData);
                npcData.Add(data);

            }
        }

        static void ImportPlayerData(Save aSave)
        {
            string rawData = System.IO.File.ReadAllText(aSave.Units + "\\PlayerData.unit");
            playerData = JsonConvert.DeserializeObject<PlayerData>(rawData);
        }

        static void ImportGuildData(Save aSave)
        {
            guildData = new List<UnitData>();

            string path = aSave.Guild + "\\";

            string[] files = System.IO.Directory.GetFiles(path);


            for (int j = 0; j < files.Length; j++)
            {
                string rawData = System.IO.File.ReadAllText(files[j]);
                UnitData data = JsonConvert.DeserializeObject<UnitData>(rawData);
                guildData.Add(data);
            }
        }
        
        static void ImportClassData()
        {
            playerClassData = new Dictionary<string, FriendlyClassData>();
            allyClassData = new Dictionary<string, FriendlyClassData>();
            mobClassData = new Dictionary<string, MobClassData>();

            string path = Game1.ContentManager.RootDirectory + "\\Data\\Class\\";

            string[] folders = System.IO.Directory.GetDirectories(path);

            for (int i = 0; i < folders.Length; i++)
            {
                string[] files = System.IO.Directory.GetFiles(folders[i]);
                string type = folders[i].Substring(path.Length);
                for (int j = 0; j < files.Length; j++)
                {
                    string rawData = System.IO.File.ReadAllText(files[j]);
                    AddToClassData(rawData, Enum.Parse<ClassData.Type>(type));
                }
            }


        }

        static void AddToClassData(string aRawData, ClassData.Type aType)
        {
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
                        allyClassData.Add(data.Name, data);
                    }
                    break;
                case ClassData.Type.Mob:
                    {
                        MobClassData data = JsonConvert.DeserializeObject<MobClassData>(aRawData);
                        mobClassData.Add(data.Name, data);
                    }
                    break;
                default:
                    throw new Exception("Error");
            }
        }

        public static void SaveData(Save aSave)
        {

            SaveManager.ExportData(aSave.Units + "\\" + "PlayerData.unit", playerData);

            aSave.ClearFolder(aSave.Guild);

            for (int i = 0; i < guildData.Count; i++)
            {
                SaveManager.ExportData(aSave.Guild + "\\" + guildData[i].Name + ".unit", guildData[i]);
            }
        }
    }
}
