using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Project_1.Camera;
using Project_1.GameObjects.Entities;
using Project_1.GameObjects.Spawners.Pathing;
using Project_1.GameObjects.Unit;
using Project_1.Managers;
using Project_1.Managers.Saves;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json;
using System.IO;

namespace Project_1.GameObjects.Spawners
{
    internal static class SpawnerManager
    {
        static List<SpawnZone> spawnZones;
        static readonly RenderCache<SpawnZoneRenderSnapshot> renderSpawnZones = new RenderCache<SpawnZoneRenderSnapshot>();
        static readonly HashSet<int> knownZoneIds = new HashSet<int>();
        static readonly HashSet<int> currentZoneIds = new HashSet<int>();

        static Dictionary<string, int> savedMobNames;
        static bool initialized;

        public static void Init()
        {
            ThreadAffinity.AssertMainThread();
            if (initialized) return;
            initialized = true;
            savedMobNames = new Dictionary<string, int>();
            spawnZones = new List<SpawnZone>();

        }

        public static void Load(Save aSave)
        {
            ThreadAffinity.AssertSimThread();
            for (int i = 0; i < spawnZones.Count; i++)
            {
                spawnZones[i].RemoveAllPlates();
            }
            spawnZones.Clear();
            ClearRenderCache();
            SavedMobData[] unitData = ImportUnitData(aSave);
            ImportZones(aSave, unitData);
        }

        public static void LoadFromTokens(IReadOnlyList<JToken> zoneTokens, IReadOnlyList<JToken> mobTokens, JsonSerializer serializer)
        {
            ThreadAffinity.AssertSimThread();
            for (int i = 0; i < spawnZones.Count; i++)
            {
                spawnZones[i].RemoveAllPlates();
            }
            spawnZones.Clear();
            ClearRenderCache();

            if (serializer == null)
            {
                return;
            }

            SavedMobData[] unitData = Array.Empty<SavedMobData>();
            if (mobTokens != null && mobTokens.Count > 0)
            {
                unitData = new SavedMobData[mobTokens.Count];
                for (int i = 0; i < mobTokens.Count; i++)
                {
                    unitData[i] = mobTokens[i].ToObject<SavedMobData>(serializer);
                }
            }

            if (zoneTokens == null || zoneTokens.Count == 0)
            {
                return;
            }

            for (int i = 0; i < zoneTokens.Count; i++)
            {
                SpawnZoneSaveData zoneData = zoneTokens[i].ToObject<SpawnZoneSaveData>(serializer);
                if (zoneData == null) continue;
                SavedMobData[] zoneUnits = unitData.Where(x => x != null && x.SpawnZoneID == zoneData.Id).ToArray();
                if (zoneUnits.Length == 0)
                {
                    spawnZones.Add(new SpawnZone(zoneData.Id, zoneData.MobNames ?? Array.Empty<string>(), zoneData.Pathing ?? Array.Empty<MobPathing>()));
                    continue;
                }
                SpawnZone zone = new SpawnZone(zoneData.Id, zoneUnits);
                zone.ApplySaveData(zoneData);
                spawnZones.Add(zone);
            }
        }

        public static void GetSaveSnapshot(out SpawnZone[] zones, out SavedMobData[] savedMobs)
        {
            ThreadAffinity.AssertSimThread();
            zones = spawnZones == null || spawnZones.Count == 0 ? Array.Empty<SpawnZone>() : spawnZones.ToArray();
            if (zones.Length == 0)
            {
                savedMobs = Array.Empty<SavedMobData>();
                return;
            }

            List<SavedMobData> mobData = new List<SavedMobData>();
            for (int i = 0; i < zones.Length; i++)
            {
                SavedMobData[] zoneMobs = zones[i].GetSavedMobData();
                if (zoneMobs == null) continue;
                for (int j = 0; j < zoneMobs.Length; j++)
                {
                    if (zoneMobs[j] != null) mobData.Add(zoneMobs[j]);
                }
            }
            savedMobs = mobData.ToArray();
        }

        static void ClearRenderCache()
        {
            renderSpawnZones.RequestClear();
            knownZoneIds.Clear();
            currentZoneIds.Clear();
        }

        static void ImportZones(Save aSave, SavedMobData[] aUnitData)
        {
            ThreadAffinity.AssertSimThread();
            string path = aSave.SpawnZones;

            string[] files = Directory.GetFiles(path);
            SavedMobData[] distinctUnitZones = aUnitData
                .Where(unit => unit != null)
                .Distinct(new SpawnZoneComparer())
                .ToArray();
            HashSet<int> spawnZonesWithMobs = new HashSet<int>(distinctUnitZones.Select(unit => unit.SpawnZoneID));

            for (int i = 0; i < files.Length; i++)
            {
                string lines = File.ReadAllText(files[i]);

                string rawData = lines;
                int zoneId = int.Parse(SaveManager.TrimToNameOnly(files[i]));

                if (!spawnZonesWithMobs.Contains(zoneId))
                {
                    spawnZones.Add(SaveManager.ImportData<SpawnZone>(rawData));
                    continue;
                }

                spawnZones.Add(new SpawnZone(zoneId, aUnitData.Where(x => x != null && x.SpawnZoneID == zoneId).ToArray()));
                JsonSerializerSettings settings = new JsonSerializerSettings() {  ObjectCreationHandling = ObjectCreationHandling.Replace, TypeNameHandling = TypeNameHandling.Auto};
                JsonConvert.PopulateObject(rawData, spawnZones.Last(), settings);
            }

        }

        class SpawnZoneComparer : IComparer<SavedMobData>, IEqualityComparer<SavedMobData>
        {
            public int Compare(SavedMobData x, SavedMobData y)
            {
                if (x.SpawnZoneID < y.SpawnZoneID) return -1;
                if (x.SpawnZoneID > y.SpawnZoneID) return 1;
                return 0;
            }

            public bool Equals(SavedMobData x, SavedMobData y)
            {
                return x.SpawnZoneID == y.SpawnZoneID;
            }

            public int GetHashCode([DisallowNull] SavedMobData obj)
            {
                return obj.SpawnZoneID.GetHashCode();
            }
        }

        internal static void Update()
        {
            ThreadAffinity.AssertSimThread();
            for (int i = 0; i < spawnZones.Count; i++)
            {
                spawnZones[i].Update();
            }
        }

        public static void SaveData(Save aSave)
        {
            ThreadAffinity.AssertSimThread();
            aSave.ClearFolder(aSave.SpawnZones);
            aSave.ClearFolder(aSave.NonFriendly);
            for (int i = 0; i < spawnZones.Count; i++)
            {
                SaveManager.ExportData(aSave.SpawnZones + "\\" + spawnZones[i].Id + ".spawn", spawnZones[i]);
            }

            savedMobNames.Clear();
            for (int i = 0; i < spawnZones.Count; i++)
            {
                SavedMobData[] savedMobData = spawnZones[i].GetSavedMobData();
                for (int j = 0; j < savedMobData.Length; j++)
                {
                    if (savedMobData[j] == null) continue;

                    string name = savedMobData[j].Name;
                    int nrOfCopies = 0;
                    if (savedMobNames.ContainsKey(name))
                    {
                        nrOfCopies = savedMobNames[name]++;
                    }
                    else
                    {
                        savedMobNames.Add(name, 1);
                    }
                    SaveManager.ExportData(aSave.NonFriendly + "\\" + name + nrOfCopies + ".unit", savedMobData[j]);
                }
            }
        }

        static SavedMobData[] ImportUnitData(Save aSave)
        {
            ThreadAffinity.AssertSimThread();
            List<SavedMobData> unitData = new List<SavedMobData>();

            string path = aSave.InWorld;

            string[] folders = Directory.GetDirectories(path);

            for (int i = 0; i < folders.Length; i++)
            {
                string[] files = Directory.GetFiles(folders[i]);
                for (int j = 0; j < files.Length; j++)
                {
                    string rawData = File.ReadAllText(files[j]);
                    SavedMobData data = JsonConvert.DeserializeObject<SavedMobData>(rawData);
                    unitData.Add(data);
                }
            }
            return unitData.ToArray();
        }

        public static void CreateNewSpawnZone(string[] aMobNames)
        {
            ThreadAffinity.AssertSimThread();
            SpawnZone spawnZone = new SpawnZone(spawnZones.Count, aMobNames);
            spawnZones.Add(spawnZone);
        }

        public static void CreateNewSpawner(int aId, MobPathing aPathing)
        {
            ThreadAffinity.AssertSimThread();
            spawnZones[aId].CreateSpawner(aPathing);
        }

        public static Spawner GetSpawner(int aZoneId, int aSpawnerId)
        {
            ThreadAffinity.AssertSimThread();
            return spawnZones[aZoneId].GetSpawner(aSpawnerId);
        }

        public static void RefreshPlates()
        {
            ThreadAffinity.AssertSimThread();
            for (int i = 0; i < spawnZones.Count; i++)
            {
                spawnZones[i].RefreshPlates();
            }
        }

        internal static bool TryGetSpawnAt(WorldSpace worldPos, out Entity entity)
        {
            ThreadAffinity.AssertSimThread();
            for (int i = 0; i < spawnZones.Count; i++)
            {
                if (spawnZones[i].TryGetSpawnAt(worldPos, out entity)) return true;
            }
            entity = null;
            return false;
        }

        internal static bool TryGetSpawnByRenderId(int renderId, out Entity entity)
        {
            ThreadAffinity.AssertSimThread();
            if (renderId <= 0)
            {
                entity = null;
                return false;
            }

            for (int i = 0; i < spawnZones.Count; i++)
            {
                if (spawnZones[i].TryGetSpawnByRenderId(renderId, out entity)) return true;
            }

            entity = null;
            return false;
        }

        internal static void DrawMinimapSnapshots(SpriteBatch aBatch, WorldSpace aOrigin, AbsoluteScreenPosition aMinimapOffset, AbsoluteScreenPosition aMinimapSize)
        {
            ThreadAffinity.AssertMainThread();
            renderSpawnZones.ApplyUpdates();
            foreach (SpawnZoneRenderSnapshot snapshot in renderSpawnZones.Values)
            {
                snapshot.MinimapDraw(aBatch, aOrigin, aMinimapOffset, aMinimapSize);
            }
        }

        internal static void AppendMinimapDots(List<MinimapDotSnapshot> dots)
        {
            ThreadAffinity.AssertSimThread();
            for (int i = 0; i < spawnZones.Count; i++)
            {
                spawnZones[i].AppendMinimapDots(dots);
            }
        }

        internal static void DrawSnapshots(SpriteBatch aBatch)
        {
            ThreadAffinity.AssertMainThread();
            // Snapshot-only draw path. Do not read live sim spawn-zone state here.
            renderSpawnZones.ApplyUpdates();
            foreach (SpawnZoneRenderSnapshot snapshot in renderSpawnZones.Values)
            {
                snapshot.Draw(aBatch);
            }
        }

        internal static void BuildRenderSnapshot()
        {
            ThreadAffinity.AssertSimThread();
            currentZoneIds.Clear();
            for (int i = 0; i < spawnZones.Count; i++)
            {
                SpawnZoneRenderSnapshot snapshot = spawnZones[i].BuildRenderSnapshot();
                renderSpawnZones.EnqueueUpdate(snapshot);
                currentZoneIds.Add(snapshot.RenderId);
            }
            PublishRemovals();
        }

        static void PublishRemovals()
        {
            foreach (int id in knownZoneIds)
            {
                if (!currentZoneIds.Contains(id))
                {
                    renderSpawnZones.EnqueueRemove(id);
                }
            }
            knownZoneIds.Clear();
            foreach (int id in currentZoneIds)
            {
                knownZoneIds.Add(id);
            }
        }
    }
}
