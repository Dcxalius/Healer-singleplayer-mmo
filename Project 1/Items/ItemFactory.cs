using Microsoft.Xna.Framework.Content;
using Newtonsoft.Json;
using Project_1.GameObjects.Spells;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Project_1.Items
{
    internal static class ItemFactory
    {
        //static Dictionary<int, ItemData> itemData;
        static ItemData[] itemData;

        //static int Id { get => nextId++; }
        //static int nextId = 0;

        public static void Init(ContentManager aContentManager)
        {
            //itemData = new Dictionary<int, ItemData>();
            List<ItemData> xdd = new List<ItemData>();

            string path = aContentManager.RootDirectory + "\\Data\\Items\\";
            string[] files = Directory.GetFiles(path);

            for (int i = 0; i < files.Length; i++)
            {
                string rawData = File.ReadAllText(files[i]);
                ItemData data = JsonConvert.DeserializeObject<ItemData>(rawData);
                //data.ID = Id;
                xdd.Add(data);
            }
            itemData = xdd.ToArray();
        }

        public static ItemData GetItemData(int aId)
        {
            return itemData[aId];
        }

        public static ItemData GetItemDataByName(string a)
        {
            return itemData.Single(data => data.Name == a);
        }
    }
}
