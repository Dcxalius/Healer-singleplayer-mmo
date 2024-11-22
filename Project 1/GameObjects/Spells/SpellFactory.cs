using Microsoft.Xna.Framework.Content;
using Newtonsoft.Json;
using Project_1.Managers;
using SharpDX.MediaFoundation.DirectX;
using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Project_1.GameObjects.Spells
{
    internal static class SpellFactory
    {
        static Dictionary<string, SpellData> spellData;
        //static Dictionary<int, SpellEffect> spellEffect;
        static SpellEffect[] effectData;

        public static void Init(ContentManager aContentManager)
        {
            InitEffectData(aContentManager);
            InitSpellData(aContentManager);

        }

        static void InitSpellData(ContentManager aContentManager)
        {
            spellData = new Dictionary<string, SpellData>();
            string path = aContentManager.RootDirectory + "\\Data\\Spells\\";
            string[] files = Directory.GetFiles(path);

            for (int i = 0; i < files.Length; i++)
            {
                string rawData = File.ReadAllText(files[i]);
                SpellData data = JsonConvert.DeserializeObject<SpellData>(rawData);
                spellData.Add(data.Name, data);
            }
        }

        static void InitEffectData(ContentManager aContentManager)
        {
            string path = aContentManager.RootDirectory + "\\Data\\Effects\\";
            string[] folders = Directory.GetDirectories(path);

            List<SpellEffect> effects = new List<SpellEffect>();
            for (int i = 0; i < folders.Length; i++)
            {

                string[] files = Directory.GetFiles(folders[i]);
                for (int j = 0; j < files.Length; j++)
                {

                    string rawData = File.ReadAllText(files[i]);
                    SpellEffect data;
                    switch (folders[i])
                    {
                        case "Content\\Data\\Effects\\Instant":
                            data = JsonConvert.DeserializeObject<Instant>(rawData);
                            break;
                        case "Content\\Data\\Effects\\OverTime":
                            data = JsonConvert.DeserializeObject<OverTime>(rawData);
                            break;
                        default:
                            throw new NotImplementedException();
                    }
                    effects.Add(data);

                }
            }

            effectData = effects.ToArray();
        }

        public static SpellEffect GetSpellEffect(int aId)
        {
            return effectData[aId];
        }

        public static SpellEffect GetSpellEffect(string aName)
        {
            return effectData.Single(effect => effect.Name == aName);

        }

        public static SpellData GetSpell(String aName)
        {
            return spellData[aName];
        }
    }
}
