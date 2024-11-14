using Microsoft.Xna.Framework.Content;
using Newtonsoft.Json;
using Newtonsoft.Json.Bson;
using Project_1.GameObjects.Entities;
using Project_1.Managers;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static System.Windows.Forms.VisualStyles.VisualStyleElement.Header;

namespace Project_1.Items
{
    internal static class LootFactory
    {
        static Dictionary<string, LootTable> lootData;

        public static void Init(ContentManager aContentManager)
        {
            lootData = new Dictionary<string, LootTable>();
            ImportData(aContentManager);
        }

        static void ImportData(ContentManager aContentManager)
        {
            string path = aContentManager.RootDirectory + "\\Data\\Loot\\";
            string[] files = Directory.GetFiles(path);

            for (int i = 0; i < files.Length; i++)
            {
                string[] rawData = File.ReadAllLines(files[i]);

                string[] minMax = rawData[0].Split(",");
                int min = int.Parse(minMax[0]);
                int max = int.Parse(minMax[1]);

                string name = files[i].Substring(path.Length, files[i].LastIndexOf(".") - path.Length);
                lootData.Add(name, new LootTable(CreateLoot(rawData), min, max));
            }
        }

        static LootData[] CreateLoot(string[] aRawData)
        {
            List<LootData> itemList = new List<LootData>();

            for (int j = 1; j < aRawData.Length; j++)
            {
                LootData data = JsonConvert.DeserializeObject<LootData>(aRawData[j]);
                itemList.Add(data);
            }

            return itemList.ToArray();
        }

        public static LootTable GetData(string aName)
        {
            if (lootData.ContainsKey(aName))
            {
                return lootData[aName];
            }
            else
            {
                DebugManager.Print(typeof(LootFactory), "Error getting data for unit " + aName);
                return null;
            }
            
        }
    }
}
