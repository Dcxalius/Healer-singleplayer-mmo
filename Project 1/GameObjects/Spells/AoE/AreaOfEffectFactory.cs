using Microsoft.Xna.Framework.Content;
using Newtonsoft.Json;
using Project_1.GameObjects.Entities;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Project_1.GameObjects.Spells.AoE
{
    internal static class AreaOfEffectFactory
    {
        static Dictionary<string, AreaOfEffectData> AoEData = new Dictionary<string, AreaOfEffectData>();


        public static void Init(ContentManager aContentManager)
        {
            string path = aContentManager.RootDirectory + "\\AreaOfEffects\\";
            string[] files = Directory.GetFiles(path);

            for (int i = 0; i < files.Length; i++)
            {
                string rawData = File.ReadAllText(files[i]);
                AreaOfEffectData data = JsonConvert.DeserializeObject<AreaOfEffectData>(rawData);
                AoEData.Add(files[i], data);
            }
        }

    }
}
