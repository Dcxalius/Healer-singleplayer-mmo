using Microsoft.Xna.Framework.Content;
using Newtonsoft.Json;
using Project_1.Managers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Project_1.GameObjects.Entities
{
    internal static class ObjectFactory
    {
        static Dictionary<string, UnitData> unitData = new Dictionary<string, UnitData>();
        static UnitData defaultData = new UnitData();

        public static void Init(ContentManager aC)
        {
            //UnitData data = new UnitData("Sheep", 100, UnitData.RelationToPlayer.Neutral, 50);
            //ExportData("C:\\Users\\Cassandra\\source\\repos\\Project 1\\Project 1\\Content\\UnitData.json", data);
            ImportData(aC.RootDirectory, aC);

            ExportData(aC.RootDirectory + "\\UnitData.json", unitData["Sheep"]);
        }

        public static UnitData GetData(string aName)
        {
            if (unitData.ContainsKey(aName))
            {
                return unitData[aName];
            }
            else
            {
                DebugManager.Print(typeof(ObjectManager), "Error getting data for unit " + aName);
                return defaultData;
            }
        }

        static void ImportData(string aPathToData, ContentManager aContentManager)
        {
            string[] dataAsString = System.IO.File.ReadAllLines(aPathToData + "\\Data\\UnitData.json");

            for (int i = 0; i < dataAsString.Length; i++)
            {
                UnitData data = JsonConvert.DeserializeObject<UnitData>(dataAsString[i]);
                unitData.Add(data.Name, data);
            }
        }

        static void ExportData(string aDestination, object aObjectToExport)
        {
            string json = JsonConvert.SerializeObject(aObjectToExport);
            System.IO.File.WriteAllText(aDestination, json);
        }
    }
}
