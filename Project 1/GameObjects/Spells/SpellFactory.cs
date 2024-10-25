using Microsoft.Xna.Framework.Content;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Project_1.GameObjects.Spells
{
    internal static class SpellFactory
    {
        static Dictionary<string, SpellData> spellData;
        static Dictionary<int, SpellEffect> spellEffect;

        public static void Init(ContentManager aContentManager)
        {
            string path = aContentManager.RootDirectory + "\\Spells\\";
            string[] files = Directory.GetFiles(path);

            for (int i = 0; i < files.Length; i++)
            {
                string rawData = File.ReadAllText(files[i]);
                SpellData data = JsonConvert.DeserializeObject<SpellData>(rawData);
                spellData.Add(files[i], data);
            }
        }

        public static SpellEffect GetSpellEffect(int id)
        {
            return spellEffect[id];
        }
    }
}
