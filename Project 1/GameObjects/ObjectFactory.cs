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
using System.Security;
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

        static string contentRootDirectory;

        public static void Init(ContentManager aC)
        {
            contentRootDirectory = aC.RootDirectory;

            ImportClassData();
            ImportMobData();
            ImportUnitData();
            ImportGuildData();
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

        static void ImportMobData()
        {
            mobData = new Dictionary<string, MobData>();
            string path = contentRootDirectory + "\\Data\\MobData\\";

           
            string[] files = System.IO.Directory.GetFiles(path);
            for (int i = 0; i < files.Length; i++)
            {
                string rawData = System.IO.File.ReadAllText(files[i]);
                MobData data = JsonConvert.DeserializeObject<MobData>(rawData);
                mobData.Add(data.Name, data);
                
            }
        }

        static void ImportUnitData()
        {
            unitData = new Dictionary<string, UnitData>();

            string path = contentRootDirectory + "\\SaveData\\Units\\World";

            string[] folders = System.IO.Directory.GetDirectories(path);

            string rawData = System.IO.File.ReadAllText(contentRootDirectory + "\\SaveData\\Units\\PlayerData.unit");
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

        static void ImportGuildData()
        {
            guildData = new List<UnitData>();

            string path = contentRootDirectory + "\\SaveData\\Units\\Guild\\";

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
            playerClassData = new Dictionary<string, ClassData>();
            allyClassData = new Dictionary<string, ClassData>();
            mobClassData = new Dictionary<string, ClassData>();

            string path = contentRootDirectory + "\\Data\\Class\\";

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

        public static void SaveData()
        {
            UnitData[] dataArray = unitData.Values.ToArray();

            ExportData("Units\\" + dataArray[0].Name + "Data.unit", dataArray[0]);
            for (int i = 1; i < unitData.Count; i++)
            {
                //TODO: Handle Friendly/Aggresive npcs once they are implemented
                ExportData("Units\\World\\Neutral\\" + dataArray[i].Name + ".unit", dataArray[i]);
            }

            for (int i = 0; i < guildData.Count; i++)
            {
                ExportData("Units\\Guild\\" + guildData[i].Name + ".unit", guildData[i]);
            }
        }

        static void ExportData(string aDestination, object aObjectToExport)
        {
            string json = JsonConvert.SerializeObject(aObjectToExport);
            System.IO.File.WriteAllText(contentRootDirectory + "\\SaveData\\" + aDestination, json);
        }
    }
}
