using Microsoft.Xna.Framework.Content;
using Newtonsoft.Json;
using Project_1.GameObjects.Entities;
using Project_1.GameObjects.Entities.Players;
using Project_1.GameObjects.Spawners;
using Project_1.GameObjects.Unit;
using Project_1.Managers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Project_1.GameObjects
{
    internal static class ObjectFactory
    {
        static Dictionary<string, UnitData> unitData;
        static List<UnitData> guildData;
        static Dictionary<string, MobData> mobData;
        //static UnitData defaultData = new UnitData();

        static Dictionary<string, ClassData> playerClassData;
        static Dictionary<string, ClassData> allyClassData;
        static Dictionary<string, ClassData> mobClassData;

        public static void Init(ContentManager aC)
        {
            //ExportData("C:\\Users\\Cassandra\\source\\repos\\Project 1\\Project 1\\Content\\UnitData.json", data);
            ImportClassData(aC);
            ImportMobData(aC);
            ImportUnitData(aC);
            ImportGuildData(aC);

            //ExportData(aC.RootDirectory + "\\UnitData.json", unitData["Sheep"]);
        }

        public static MobData GetMobData(string aName)
        {
            if (mobData.ContainsKey(aName))
            {
                return mobData[aName];
            }
            else
            {
                throw new NotImplementedException();
            }
        }

        public static UnitData GetData(string aName)
        {
            if (unitData.ContainsKey(aName))
            {
                return unitData[aName];
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

        static void ImportMobData(ContentManager aContentManager)
        {
            mobData = new Dictionary<string, MobData>();
            string path = aContentManager.RootDirectory + "\\Data\\MobData\\";

           
            string[] files = System.IO.Directory.GetFiles(path);
            for (int i = 0; i < files.Length; i++)
            {
                string rawData = System.IO.File.ReadAllText(files[i]);
                MobData data = JsonConvert.DeserializeObject<MobData>(rawData);
                mobData.Add(data.Name, data);
                
            }
        }

        static void ImportUnitData(ContentManager aContentManager)
        {
            unitData = new Dictionary<string, UnitData>();

            string path = aContentManager.RootDirectory + "\\SaveData\\Units\\World";

            string[] folders = System.IO.Directory.GetDirectories(path);

            string rawData = System.IO.File.ReadAllText(aContentManager.RootDirectory + "\\SaveData\\Units\\PlayerData.unit");
            PlayerData playerData = JsonConvert.DeserializeObject<PlayerData>(rawData);
            unitData.Add(playerData.Name, playerData);

            for (int i = 0; i < folders.Length; i++)
            {
                string[] files = System.IO.Directory.GetFiles(folders[i]);
                for (int j = 0; j < files.Length; j++)
                {
                    rawData = System.IO.File.ReadAllText(files[j]);
                    UnitData data = JsonConvert.DeserializeObject<UnitData>(rawData);
                    unitData.Add(data.Name, data);
                }
            }
        }

        static void ImportGuildData(ContentManager aContentManager)
        {
            guildData = new List<UnitData>();

            string path = aContentManager.RootDirectory + "\\SaveData\\Units\\Guild\\";

            string[] files = System.IO.Directory.GetFiles(path);


            for (int j = 0; j < files.Length; j++)
            {
                string rawData = System.IO.File.ReadAllText(files[j]);
                UnitData data = JsonConvert.DeserializeObject<UnitData>(rawData);
                guildData.Add(data);
            }
        }
        
        static void ImportClassData(ContentManager aContentManager)
        {
            playerClassData = new Dictionary<string, ClassData>();
            allyClassData = new Dictionary<string, ClassData>();
            mobClassData = new Dictionary<string, ClassData>();

            string path = aContentManager.RootDirectory + "\\Data\\Class\\";

            string[] folders = System.IO.Directory.GetDirectories(path);

            for (int i = 0; i < folders.Length; i++)
            {
                string[] files = System.IO.Directory.GetFiles(folders[i]);
                string type = folders[i].Substring(path.Length);
                for (int j = 0; j < files.Length; j++)
                {
                    string rawData = System.IO.File.ReadAllText(files[j]);
                    ClassData data = JsonConvert.DeserializeObject<ClassData>(rawData);
                    AddToClassData(data, Enum.Parse<ClassData.Type>(type));
                }
            }


        }

        static void AddToClassData(ClassData aData, ClassData.Type aType)
        {
            switch (aType)
            {
                case ClassData.Type.Player:
                    playerClassData.Add(aData.Name, aData);
                    break;
                case ClassData.Type.Ally:
                    allyClassData.Add(aData.Name, aData);
                    break;
                case ClassData.Type.Mob:
                    mobClassData.Add(aData.Name, aData);
                    break;
                default:
                    throw new Exception("Error");
            }
        }

        static void ExportData(string aDestination, object aObjectToExport)
        {
            string json = JsonConvert.SerializeObject(aObjectToExport);
            System.IO.File.WriteAllText(aDestination, json);
        }
    }
}
