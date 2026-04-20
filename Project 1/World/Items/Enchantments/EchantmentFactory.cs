using Newtonsoft.Json;
using Project_1.Managers;
using System;
using System.Collections.Generic;
using System.IO;

namespace Project_1.World.Items.Enchantments
{
    internal static class EnchantmentFactory
    {
        static readonly Dictionary<int, EnchantmentData> dataById = new Dictionary<int, EnchantmentData>();
        static readonly Dictionary<string, EnchantmentData> dataByName = new Dictionary<string, EnchantmentData>(StringComparer.OrdinalIgnoreCase);
        static bool initialized;

        public static void Init()
        {
            if (initialized) return;
            initialized = true;

            string path = Path.Combine(Game1.ContentManager.RootDirectory, "Data", "Enchantments");
            if (!Directory.Exists(path))
            {
                return;
            }

            string[] files = Directory.GetFiles(path, "*", SearchOption.AllDirectories);
            for (int i = 0; i < files.Length; i++)
            {
                string rawData = File.ReadAllText(files[i]);
                EnchantmentData data = JsonConvert.DeserializeObject<EnchantmentData>(rawData);
                if (data == null) continue;

                dataById.Add(data.Id, data);
                dataByName.Add(data.Name, data);
            }
        }

        public static EnchantmentData GetData(int id)
        {
            Init();
            return dataById[id];
        }

        public static EnchantmentData GetData(string name)
        {
            Init();
            return dataByName[name];
        }
    }
}
