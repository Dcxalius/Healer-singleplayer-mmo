using System.Collections.Generic;
using System.IO;
using Newtonsoft.Json.Linq;

namespace Project_1.Managers.Saves
{
    internal sealed class SaveLoadPayload
    {
        SaveLoadPayload(string saveName)
        {
            SaveName = saveName;
        }

        public string SaveName { get; }
        public JToken CameraPosition { get; private set; }
        public JToken PlayerData { get; private set; }
        public List<JToken> GuildData { get; private set; }
        public List<JToken> TileChunks { get; private set; }
        public List<JToken> Corpses { get; private set; }
        public List<JToken> SpawnZones { get; private set; }
        public List<JToken> SavedMobs { get; private set; }

        public static SaveLoadPayload Parse(Save save)
        {
            if (save == null) return null;
            SaveLoadPayload payload = new SaveLoadPayload(save.Name);
            payload.CameraPosition = ReadToken(save.CameraPosition);
            payload.PlayerData = ReadToken(Path.Combine(save.Units, "PlayerData.unit"));
            payload.GuildData = ReadTokens(save.Guild);
            payload.TileChunks = ReadTokens(save.Tiles);
            payload.Corpses = ReadTokens(save.Corpses);
            payload.SpawnZones = ReadTokens(save.SpawnZones);
            payload.SavedMobs = ReadTokensRecursive(save.InWorld);
            return payload;
        }

        static JToken ReadToken(string path)
        {
            if (!File.Exists(path)) return null;
            string json = File.ReadAllText(path);
            return JToken.Parse(json);
        }

        static List<JToken> ReadTokens(string folder)
        {
            List<JToken> tokens = new List<JToken>();
            if (!Directory.Exists(folder)) return tokens;
            string[] files = Directory.GetFiles(folder);
            for (int i = 0; i < files.Length; i++)
            {
                string json = File.ReadAllText(files[i]);
                tokens.Add(JToken.Parse(json));
            }
            return tokens;
        }

        static List<JToken> ReadTokensRecursive(string folder)
        {
            List<JToken> tokens = new List<JToken>();
            if (!Directory.Exists(folder)) return tokens;
            string[] files = Directory.GetFiles(folder, "*", SearchOption.AllDirectories);
            for (int i = 0; i < files.Length; i++)
            {
                string json = File.ReadAllText(files[i]);
                tokens.Add(JToken.Parse(json));
            }
            return tokens;
        }
    }
}
