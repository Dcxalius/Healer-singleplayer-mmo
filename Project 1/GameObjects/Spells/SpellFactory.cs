using Microsoft.Xna.Framework.Content;
using Newtonsoft.Json;
using Project_1.GameObjects.Spells.Buff;
using Project_1.Managers;
using SharpDX.MediaFoundation.DirectX;
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
        //static Dictionary<int, SpellEffect> spellEffect;
        static Instant[] instantData;
        static OverTime[] overTimeData;

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
            
            //string[] folders = Directory.GetDirectories(path);
            
            InitInstant(aContentManager);
            InitOverTime(aContentManager);

        }

        static void InitInstant(ContentManager aContentManager) //TODO: Ugly AF so find a better way
        {
            List<Instant> effects = new List<Instant>();

            string pathInstant = aContentManager.RootDirectory + "\\Data\\Effects\\Instant";
            string[] files = Directory.GetFiles(pathInstant);
            for (int j = 0; j < files.Length; j++)
            {

                string rawData = File.ReadAllText(files[j]);
                Instant data = JsonConvert.DeserializeObject<Instant>(rawData);


                effects.Add(data);


            }
            instantData = effects.ToArray();

        }


        static void InitOverTime(ContentManager aContentManager)
        {
            List<OverTime> effects = new List<OverTime>();
            string pathOverTime = aContentManager.RootDirectory + "\\Data\\Effects\\OverTime";
            string[] files = Directory.GetFiles(pathOverTime);
            
            for (int j = 0; j < files.Length; j++)
            {

                string rawData = File.ReadAllText(files[j]);
                OverTime data = JsonConvert.DeserializeObject<OverTime>(rawData);
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

            SpellEffect effect = instantData.SingleOrDefault(effect => effect.Name == aName);

            if (effect != null) return effect;

            effect = overTimeData.SingleOrDefault(effect => effect.Name == aName);

            Debug.Assert(effect != null, "Didn't find SpellEffect by the name " + aName);
            return effect;
        }

        public static SpellData GetSpell(String aName)
        {
            return spellData[aName];
        }
    }
}
