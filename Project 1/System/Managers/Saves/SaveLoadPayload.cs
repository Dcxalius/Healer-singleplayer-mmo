using System.IO;
using Newtonsoft.Json.Linq;
using System;

namespace Project_1.Managers.Saves
{
    internal sealed class SaveLoadPayload
    {
        SaveLoadPayload(
            string saveName,
            JToken cameraPosition,
            JToken playerData,
            JToken[] guildData,
            JToken[] tileChunks,
            JToken[] corpses,
            JToken[] spawnZones,
            JToken[] savedMobs)
        {
            SaveName = saveName;
            CameraPosition = cameraPosition?.DeepClone();
            PlayerData = playerData?.DeepClone();
            GuildData = CloneTokens(guildData);
            TileChunks = CloneTokens(tileChunks);
            Corpses = CloneTokens(corpses);
            SpawnZones = CloneTokens(spawnZones);
            SavedMobs = CloneTokens(savedMobs);
        }

        public string SaveName { get; }
        public JToken CameraPosition { get; }
        public JToken PlayerData { get; }
        public JToken[] GuildData { get; }
        public JToken[] TileChunks { get; }
        public JToken[] Corpses { get; }
        public JToken[] SpawnZones { get; }
        public JToken[] SavedMobs { get; }

        public static SaveLoadPayload Parse(Save save)
        {
            if (save == null) return null;
            return new SaveLoadPayload(
                save.Name,
                ReadToken(save.CameraPosition),
                ReadToken(Path.Combine(save.Units, "PlayerData.unit")),
                ReadTokens(save.Guild),
                ReadTokens(save.Tiles),
                ReadTokens(save.Corpses),
                ReadTokens(save.SpawnZones),
                ReadTokensRecursive(save.InWorld));
        }

        static JToken ReadToken(string path)
        {
            if (!File.Exists(path)) return null;
            string json = File.ReadAllText(path);
            return JToken.Parse(json);
        }

        static JToken[] ReadTokens(string folder)
        {
            if (!Directory.Exists(folder)) return Array.Empty<JToken>();
            string[] files = Directory.GetFiles(folder);
            JToken[] tokens = new JToken[files.Length];
            for (int i = 0; i < files.Length; i++)
            {
                string json = File.ReadAllText(files[i]);
                tokens[i] = JToken.Parse(json);
            }
            return tokens;
        }

        static JToken[] ReadTokensRecursive(string folder)
        {
            if (!Directory.Exists(folder)) return Array.Empty<JToken>();
            string[] files = Directory.GetFiles(folder, "*", SearchOption.AllDirectories);
            JToken[] tokens = new JToken[files.Length];
            for (int i = 0; i < files.Length; i++)
            {
                string json = File.ReadAllText(files[i]);
                tokens[i] = JToken.Parse(json);
            }
            return tokens;
        }

        static JToken[] CloneTokens(JToken[] source)
        {
            if (source == null || source.Length == 0) return Array.Empty<JToken>();
            JToken[] clone = new JToken[source.Length];
            for (int i = 0; i < source.Length; i++)
            {
                clone[i] = source[i]?.DeepClone();
            }
            return clone;
        }
    }
}
