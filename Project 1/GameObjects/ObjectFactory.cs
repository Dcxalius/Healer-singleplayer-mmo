using Microsoft.Xna.Framework.Content;
using Newtonsoft.Json;
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
        static UnitData defaultData = new UnitData();

        static Dictionary<string, ClassData> playerClassData;
        static Dictionary<string, ClassData> allyClassData;
        static Dictionary<string, ClassData> mobClassData;

        public static void Init(ContentManager aC)
        {
            unitData = new Dictionary<string, UnitData>();
            playerClassData = new Dictionary<string, ClassData>();
            allyClassData = new Dictionary<string, ClassData>();
            mobClassData = new Dictionary<string, ClassData>();

            //UnitData data = new UnitData("Sheep", 100, UnitData.RelationToPlayer.Neutral, 50);
            //ExportData("C:\\Users\\Cassandra\\source\\repos\\Project 1\\Project 1\\Content\\UnitData.json", data);
            ImportUnitData(aC);
            ImportClassData(aC);
            //ExportData(aC.RootDirectory + "\\UnitData.json", unitData["Sheep"]);
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
                DebugManager.Print(typeof(ObjectManager), "Error getting data for unit " + aName);
                return defaultData;
            }
        }

        public static ClassData GetPlayerClass(string aName) => playerClassData[aName];
        public static ClassData GetAllyClass(string aName) => allyClassData[aName];
        public static ClassData GetMobClass(string aName) => mobClassData[aName];

        static void ImportUnitData(ContentManager aContentManager)
        {
            string path = aContentManager.RootDirectory + "\\Data\\Units\\";

            string[] folders = System.IO.Directory.GetDirectories(path);

            string playerData = System.IO.File.ReadAllText(path + "PlayerData.unit");
            UnitData data = JsonConvert.DeserializeObject<UnitData>(playerData);
            unitData.Add(data.Name, data);

            for (int i = 0; i < folders.Length; i++)
            {
                string[] files = System.IO.Directory.GetFiles(folders[i]);
                for (int j = 0; j < files.Length; j++)
                {
                    string rawData = System.IO.File.ReadAllText(files[j]);
                    data = JsonConvert.DeserializeObject<UnitData>(rawData);
                    unitData.Add(data.Name, data);
                }
            }
        }
        
        static void ImportClassData(ContentManager aContentManager)
        {
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
