using Microsoft.Xna.Framework.Graphics;
using Project_1.Input;
using Project_1.Managers;
using Project_1.Managers.Saves;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Project_1.GameObjects.Entities.Corspes
{
    internal static class CorpseManager
    {
        static List<Corpse> corpses;
        static volatile Corpse[] renderCorpses = Array.Empty<Corpse>();
        static bool initialized;

        public static void Init()
        {
            if (initialized) return;
            initialized = true;
            corpses = new List<Corpse>();
        }

        public static void AddCorpse(Corpse aCorpse) => corpses.Add(aCorpse);

        public static void RemoveCorpse(Corpse aCorpse) => corpses.Remove(aCorpse);

        public static void Reset() => corpses.Clear();

        internal static void Save(Save aSave)
        {
            aSave.ClearFolder(aSave.Corpses);
            for (int i = 0; i < corpses.Count; i++)
            {
                SaveManager.ExportData(aSave.Corpses + "\\" + i + ".corpse", corpses[i]);
            }
        }

        public static void Load(Save aSave)
        {
            corpses.Clear();
            string[] files = Directory.GetFiles(aSave.Corpses);
            for (int i = 0; i < files.Length; i++)
            {
                string json = File.ReadAllText(files[i]);
                Corpse c = SaveManager.ImportData<Corpse>(json);
                corpses.Add(c);
            }
        }

        public static void LoadFromTokens(IReadOnlyList<JToken> tokens, JsonSerializer serializer)
        {
            corpses.Clear();
            if (tokens == null || serializer == null) return;
            for (int i = 0; i < tokens.Count; i++)
            {
                if (tokens[i] == null) continue;
                _ = tokens[i].ToObject<Corpse>(serializer);
            }
        }

        public static bool Click(ClickEvent aClickEvent)
        {
            ThreadAffinity.AssertSimThread();
            for (int i = 0; i < corpses.Count; i++)
            {
                if (corpses[i].Click(aClickEvent)) return true;
            }

            return false;
        }

        public static void Update()
        {
            ThreadAffinity.AssertSimThread();
            for (int i = corpses.Count - 1; i >= 0; i--) corpses[i].Update();
        }

        public static void Draw(SpriteBatch aBatch)
        {
            ThreadAffinity.AssertMainThread();
            Corpse[] snapshot = renderCorpses;
            for (int i = 0; i < snapshot.Length; i++) snapshot[i].Draw(aBatch);
        }

        internal static void BuildRenderSnapshot()
        {
            renderCorpses = corpses.ToArray();
        }

        public static Corpse[] GetSnapshot()
        {
            if (corpses == null || corpses.Count == 0) return Array.Empty<Corpse>();
            return corpses.ToArray();
        }
    }
}
