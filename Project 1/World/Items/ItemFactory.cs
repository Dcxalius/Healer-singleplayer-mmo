using Microsoft.Xna.Framework.Content;
using Newtonsoft.Json;
using Project_1.GameObjects.Spells;
using Project_1.Managers;
using Project_1.Items.SubTypes;
using Project_1.World.Items.Enchantments;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection.Metadata;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;
using Project_1.World.Items.SubTypes;

namespace Project_1.Items
{
    internal static class ItemFactory
    {
        //static Dictionary<int, ItemData> itemData;
        static ItemData[] itemData; //TODO: Remember why this is used instead of the above outcommented dict
        static bool initialized;

        public static void Init()
        {
            ThreadAffinity.AssertMainThread();
            if (initialized) return;
            initialized = true;
            EquipmentData.Init();
            EnchantmentFactory.Init();
            //itemData = new Dictionary<int, ItemData>();
            List<ItemData> itemList = new List<ItemData>();

            string path = Game1.ContentManager.RootDirectory + "\\Data\\Items\\";
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
            itemList.Sort();
            itemData = itemList.ToArray();
        }

        static ItemData CreateData(string aRawData, string aFolder)
        {
            JsonSerializerSettings settings = new JsonSerializerSettings() { ConstructorHandling = ConstructorHandling.AllowNonPublicDefaultConstructor};
            switch (aFolder)
            {
                case "Trash":
                    return JsonConvert.DeserializeObject<ItemData>(aRawData);
                case "Bags":
                    return JsonConvert.DeserializeObject<BagData>(aRawData);
                case "Container":
                    return JsonConvert.DeserializeObject<ContainerData>(aRawData);
                case "Reagents":
                    return JsonConvert.DeserializeObject<ReagentData>(aRawData);
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

        public static T GetItemData<T>(int aId) where T : ItemData
        {
            if (itemData[aId] is not T) throw new InvalidCastException();

            return itemData[aId] as T;
        }

        public static ItemData GetItemData(int aId)
        {
            return itemData[aId];
        }

        public static ItemData GetItemData(string a)
        {
            return itemData.Single(data => data.Name == a); //TODO: Add handling for if multiple items have the same name.
        }

        public static ItemData[] GetAllItemDataSnapshot()
        {
            if (itemData == null || itemData.Length == 0)
            {
                return Array.Empty<ItemData>();
            }

            ItemData[] snapshot = new ItemData[itemData.Length];
            Array.Copy(itemData, snapshot, itemData.Length);
            return snapshot;
        }

        public static Item CreateItem(ItemData aData, int aCount = 1)
        {
            switch (aData.Type)
            {
                case ItemData.ItemType.NotSet:
                    throw new NotImplementedException();
                case ItemData.ItemType.Container:
                    Debug.Assert(aCount == 1, "Containers cannot be stacked. Count should be 1.");
                    return new Container(aData as ContainerData);
                case ItemData.ItemType.Bag:
                    Debug.Assert(aCount == 1, "Bags cannot be stacked. Count should be 1.");
                    return new Bag(aData as BagData);
                case ItemData.ItemType.Trash:
                    return new Item(aData, aCount);
                case ItemData.ItemType.Consumable:
                    return new Consumable(aData as ConsumableData, aCount);
                case ItemData.ItemType.Equipment:
                    Debug.Assert(aCount == 1, "Equipment cannot be stacked. Count should be 1.");
                    return new Equipment(aData as EquipmentData);
                case ItemData.ItemType.Weapon:
                    Debug.Assert(aCount == 1, "Equipment cannot be stacked. Count should be 1.");
                    return new Weapon(aData as WeaponData);
                case ItemData.ItemType.Reagent:
                    return new Reagent(aData, aCount);
                default:
                    throw new NotImplementedException();
            }
        }

        public static Item CreateItem(int aId, int aCount = 1) => CreateItem(GetItemData(aId), aCount);

        public static Item CreateItem(LootData aLoot)
        {
            switch (aLoot.ItemData.Type)
            {
                case ItemData.ItemType.NotSet:
                    throw new NotImplementedException();
                case ItemData.ItemType.Container:
                    return new Container(aLoot);
                case ItemData.ItemType.Bag:
                    return new Bag(aLoot);
                case ItemData.ItemType.Trash:
                    return new Item(aLoot);
                case ItemData.ItemType.Consumable:
                    return new Consumable(aLoot);
                case ItemData.ItemType.Weapon:
                    return new Weapon(aLoot);
                case ItemData.ItemType.Equipment:
                    return new Equipment(aLoot);
                case ItemData.ItemType.Reagent:
                    return new Reagent(aLoot);
                default:
                    throw new NotImplementedException();
            }
        }

        internal static EquipmentProcEffect GetEquipmentProcEffect(int id)
        {
            throw new NotImplementedException();
        }
    }
}
