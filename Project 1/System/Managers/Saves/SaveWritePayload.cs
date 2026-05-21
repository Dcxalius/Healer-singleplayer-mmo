using System.Collections.Generic;
using System.IO;
using Project_1.Camera;
using Project_1.GameObjects;
using Project_1.GameObjects.Entities.Corspes;
using Project_1.GameObjects.Spawners;
using Project_1.GameObjects.Unit;
using Project_1.Managers;
using Project_1.Tiles;
using Project_1.GameObjects.Entities.Friendlies.Players;
using Path = System.IO.Path;

namespace Project_1.Managers.Saves
{
    internal sealed class SaveWritePayload
    {
        SaveWritePayload(Save save)
        {
            Save = save;
        }

        public Save Save { get; }
        public SaveDetails SaveDetails { get; private set; }
        public WorldSpace CameraPosition { get; private set; }
        public PlayerData PlayerData { get; private set; }
        public UnitData[] GuildData { get; private set; }
        public Chunk[] Chunks { get; private set; }
        public Corpse[] Corpses { get; private set; }
        public SpawnZone[] SpawnZones { get; private set; }
        public SavedMobData[] SavedMobs { get; private set; }

        public static SaveWritePayload Capture(Save save)
        {
            ThreadAffinity.AssertSimThread();
            SaveWritePayload payload = new SaveWritePayload(save);
            Player player = ObjectManager.Player;
            string playerName = player?.Name ?? save?.Name ?? "UNKNOWN";
            string className = player?.ClassData?.Name ?? string.Empty;
            int level = player?.Level?.CurrentLevel ?? 0;
            SaveDetails details = new SaveDetails(playerName, className, level, TimeManager.TotalFrameTimeAsTimeSpan);
            save?.SetSaveDetails(details);
            payload.SaveDetails = details;
            payload.CameraPosition = Camera.Camera.CentreInWorldSpace;
            payload.PlayerData = ObjectFactory.PlayerData;
            payload.PlayerData?.CaptureSaveState();
            payload.GuildData = ObjectFactory.GetGuildDataSnapshot();
            payload.Chunks = TileManager.GetChunks();
            payload.Corpses = CorpseManager.GetSnapshot();
            SpawnerManager.GetSaveSnapshot(out SpawnZone[] zones, out SavedMobData[] mobs);
            payload.SpawnZones = zones;
            payload.SavedMobs = mobs;
            return payload;
        }

        public void Write()
        {
            if (Save == null) return;
            SaveManager.ExportData(Save.SaveDetailsPathForWrite, SaveDetails);
            SaveManager.ExportData(Save.CameraPosition, CameraPosition);

            if (PlayerData != null)
            {
                SaveManager.ExportData(Path.Combine(Save.Units, "PlayerData.unit"), PlayerData);
            }

            Save.ClearFolder(Save.Guild);
            if (GuildData != null)
            {
                for (int i = 0; i < GuildData.Length; i++)
                {
                    if (GuildData[i] == null) continue;
                    SaveManager.ExportData(Path.Combine(Save.Guild, GuildData[i].Name + ".unit"), GuildData[i]);
                }
            }

            if (Chunks != null)
            {
                for (int i = 0; i < Chunks.Length; i++)
                {
                    if (Chunks[i] == null) continue;
                    SaveManager.ExportData(Path.Combine(Save.Tiles, Chunks[i].Id + ".tilemap"), Chunks[i]);
                }
            }

            Save.ClearFolder(Save.Corpses);
            if (Corpses != null)
            {
                for (int i = 0; i < Corpses.Length; i++)
                {
                    if (Corpses[i] == null) continue;
                    SaveManager.ExportData(Path.Combine(Save.Corpses, i + ".corpse"), Corpses[i]);
                }
            }

            Save.ClearFolder(Save.SpawnZones);
            if (SpawnZones != null)
            {
                for (int i = 0; i < SpawnZones.Length; i++)
                {
                    if (SpawnZones[i] == null) continue;
                    SaveManager.ExportData(Path.Combine(Save.SpawnZones, i + ".spawn"), SpawnZones[i]);
                }
            }

            Save.ClearFolder(Save.NonFriendly);
            if (SavedMobs != null)
            {
                Dictionary<string, int> savedMobNames = new Dictionary<string, int>();
                for (int i = 0; i < SavedMobs.Length; i++)
                {
                    SavedMobData data = SavedMobs[i];
                    if (data == null) continue;
                    string name = data.Name;
                    int nrOfCopies = 0;
                    if (savedMobNames.ContainsKey(name))
                    {
                        nrOfCopies = savedMobNames[name]++;
                    }
                    else
                    {
                        savedMobNames.Add(name, 1);
                    }
                    SaveManager.ExportData(Path.Combine(Save.NonFriendly, name + nrOfCopies + ".unit"), data);
                }
            }
        }
    }
}
