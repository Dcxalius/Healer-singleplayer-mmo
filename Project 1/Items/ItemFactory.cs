using Microsoft.Xna.Framework.Content;
using Newtonsoft.Json;
using Project_1.GameObjects.Spells;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

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
            string[] folders = Directory.GetDirectories(path);
            for (int i = 0; i < folders.Length; i++)
            {
                string[] files = Directory.GetFiles(folders[i]);

                for (int j = 0; j < files.Length; j++)
                {
                    string rawData = File.ReadAllText(files[j]);
                    ItemData data = CreateData(rawData, folders[i].Substring(path.Length));
                    //data.ID = Id;
                    xdd.Add(data);
                }
            }

           
            itemData = xdd.ToArray();
        }

        static ItemData CreateData(string aRawData, string aFolder)
        {
            switch (aFolder)
            {
                case "Trash":
                    return JsonConvert.DeserializeObject<ItemData>(aRawData);
                case "Container":
                    return JsonConvert.DeserializeObject<ContainerData>(aRawData);
                default:
                    throw new NotImplementedException();
            }
        }

        public static ItemData GetItemData(int aId)
        {
            return itemData[aId];
        }

        public static ItemData GetItemData(string a)
        {
            return itemData.Single(data => data.Name == a);
        }

        public static Item CreateItem(ItemData aData, int aCount = 0)
        {
            switch (aData.Type)
            {
                case ItemData.ItemType.NotSet:
                    throw new NotImplementedException();
                case ItemData.ItemType.Container:
                    return new Container(aData as ContainerData);
                case ItemData.ItemType.Trash:
                    return new Item(aData, aCount);
                default:
                    throw new NotImplementedException();
            }
            throw new NotImplementedException();
        }

        public static Item CreateItem(Loot aLoot)
        {
            switch (aLoot.ItemData.Type)
            {
                case ItemData.ItemType.NotSet:
                    throw new NotImplementedException();
                case ItemData.ItemType.Container:
                    return new Container(aLoot);
                case ItemData.ItemType.Trash:
                    return new Item(aLoot);
                default:
                    throw new NotImplementedException();
            }
            throw new NotImplementedException();
        }
    }
}
