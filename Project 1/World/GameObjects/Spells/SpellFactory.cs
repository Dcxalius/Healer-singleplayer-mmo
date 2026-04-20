using Microsoft.Xna.Framework.Content;
using Newtonsoft.Json;
using Project_1.Managers;
using Project_1.World.GameObjects.Spells.SpellEffects;
using System;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Project_1.GameObjects.Spells
{
    internal static class SpellFactory
    {
        static Dictionary<string, SpellData> spellData;
        static Dictionary<int, SpellData> spellDataById;
        //static Dictionary<int, SpellEffect> spellEffect;
        static InstantEffect[] instantData;
        static OverTimeEffect[] overTimeData;
        static AbsorbEffect[] absorbData;
        static bool initialized;

        public static void Init()
        {
            if (initialized) return;
            initialized = true;
            InitEffectData();
            InitSpellData();
        }

        static void InitSpellData()
        {
            spellData = new Dictionary<string, SpellData>();
            spellDataById = new Dictionary<int, SpellData>();
            string path = Game1.ContentManager.RootDirectory + "\\Data\\Spells\\";
            string[] files = Directory.GetFiles(path);

            for (int i = 0; i < files.Length; i++)
            {
                string rawData = File.ReadAllText(files[i]);
                SpellData data = JsonConvert.DeserializeObject<SpellData>(rawData);
                spellData.Add(data.Name, data);
                spellDataById.Add(data.Id, data);
            }
        }

        static void InitEffectData()
        {
            
            //string[] folders = Directory.GetDirectories(path);
            
            InitInstant();
            InitOverTime();
            InitAbsorb();
        }

        static void InitInstant() //TODO: Ugly AF so find a better way
        {
            List<InstantEffect> effects = new List<InstantEffect>();

            string pathInstant = Game1.ContentManager.RootDirectory + "\\Data\\Effects\\Instant";
            string[] files = Directory.GetFiles(pathInstant);
            for (int j = 0; j < files.Length; j++)
            {

                string rawData = File.ReadAllText(files[j]);
                InstantEffect data = JsonConvert.DeserializeObject<InstantEffect>(rawData);


                effects.Add(data);


            }
            instantData = effects.ToArray();

        }


        static void InitAbsorb()
        {
            List<AbsorbEffect> effects = new List<AbsorbEffect>();
            string path = Game1.ContentManager.RootDirectory + "\\Data\\Effects\\Absorb";
            if (!Directory.Exists(path))
            {
                absorbData = effects.ToArray();
                return;
            }

            string[] files = Directory.GetFiles(path);
            for (int j = 0; j < files.Length; j++)
            {
                string rawData = File.ReadAllText(files[j]);
                AbsorbEffect data = JsonConvert.DeserializeObject<AbsorbEffect>(rawData);
                Debug.Assert(!instantData.Any(x => x.Name == data.Name), "Absorb effect name conflicts with an instant effect: " + data.Name);
                effects.Add(data);
            }
            absorbData = effects.ToArray();
        }

        static void InitOverTime()
        {
            List<OverTimeEffect> effects = new List<OverTimeEffect>();
            string pathOverTime = Game1.ContentManager.RootDirectory + "\\Data\\Effects\\OverTime";
            string[] files = Directory.GetFiles(pathOverTime);
            
            for (int j = 0; j < files.Length; j++)
            {

                string rawData = File.ReadAllText(files[j]);
                OverTimeEffect data = JsonConvert.DeserializeObject<OverTimeEffect>(rawData);
                Debug.Assert(!instantData.Any(xdd => xdd.Name == data.Name), "Tried to add an overtime effect with the same name of an instant, this will be unable to be accessed by name.");

                effects.Add(data);


            }
            overTimeData = effects.ToArray();
        }

        public static SpellEffect GetSpellEffect(int aId)
        {
            if (aId < instantData.Length)
            {
                return instantData[aId];
            }

            return instantData[aId - instantData.Length];
        }

        public static SpellEffect GetSpellEffect(string aName)
        {
            SpellEffect effect = instantData.SingleOrDefault(e => e.Name == aName);
            if (effect != null) return effect;

            effect = absorbData.SingleOrDefault(e => e.Name == aName);
            if (effect != null) return effect;

            effect = overTimeData.SingleOrDefault(e => e.Name == aName);
            Debug.Assert(effect != null, "Didn't find SpellEffect by the name " + aName);
            return effect;
        }

        public static SpellData GetSpell(String aName)
        {
            Init();
            return spellData[aName];
        }

        public static SpellData GetSpell(int aId)
        {
            Init();
            return spellDataById[aId];
        }
    }
}
