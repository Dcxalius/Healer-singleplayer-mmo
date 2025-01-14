using Microsoft.Xna.Framework.Content;
using Newtonsoft.Json;
using Project_1.GameObjects.Spells;
using Project_1.Items.SubTypes;
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
            List<ItemData> itemList = new List<ItemData>();

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
                    itemList.Add(data);
                }
            }

           
            itemData = itemList.ToArray();
        }

        static ItemData CreateData(string aRawData, string aFolder)
        {
            JsonSerializerSettings settings = new JsonSerializerSettings() { ConstructorHandling = ConstructorHandling.AllowNonPublicDefaultConstructor};
            switch (aFolder)
            {
                case "Trash":
                    return JsonConvert.DeserializeObject<ItemData>(aRawData);
                case "Container":
                    return JsonConvert.DeserializeObject<ContainerData>(aRawData);
                case "Consumable":
                    return JsonConvert.DeserializeObject<ConsumableData>(aRawData);
                case "Equipment":
                    return JsonConvert.DeserializeObject<EquipmentData>(aRawData, settings);
                case "Weapon":
                    return JsonConvert.DeserializeObject<WeaponData>(aRawData, settings);
                default:
                    throw new NotImplementedException();
            }
        }

        public static bool DoesDataExist(string aName)
        {
            return itemData.Where(data => data.Name == aName).Count() > 0;
        }

        public static ItemData GetItemData(int aId)
        {
            return itemData[aId];
        }

        public static ItemData GetItemData(string a)
        {
            return itemData.Single(data => data.Name == a); //TODO: Add handling for if multiple items have the same name.
        }

        public static Item CreateItem(ItemData aData, int aCount = 1)
        {
            switch (aData.Type)
            {
                case ItemData.ItemType.NotSet:
                    throw new NotImplementedException();
                case ItemData.ItemType.Container:
                    return new Container(aData as ContainerData);
                case ItemData.ItemType.Trash:
                    return new Item(aData, aCount);
                case ItemData.ItemType.Consumable:
                    return new Consumable(aData as ConsumableData, aCount);
                case ItemData.ItemType.Equipment:
                    return new Equipment(aData as EquipmentData);
                case ItemData.ItemType.Weapon:
                    return new Weapon(aData as WeaponData);
                default:
                    throw new NotImplementedException();
            }
        }


        public static Item CreateItem(LootData aLoot)
        {
            switch (aLoot.ItemData.Type)
            {
                case ItemData.ItemType.NotSet:
                    throw new NotImplementedException();
                case ItemData.ItemType.Container:
                    return new Container(aLoot);
                case ItemData.ItemType.Trash:
                    return new Item(aLoot);
                case ItemData.ItemType.Consumable:
                    return new Consumable(aLoot);

                default:
                    throw new NotImplementedException();
            }
        }
    }
}
