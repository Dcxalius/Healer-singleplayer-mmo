using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Project_1.Camera;
using Project_1.GameObjects.Entities;
using Project_1.GameObjects.Spawners.Pathing;
using Project_1.GameObjects.Unit;
using Project_1.Input;
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

namespace Project_1.GameObjects.Spawners
{
    internal static class SpawnerManager
    {
        static List<SpawnZone> spawnZones;
        static volatile SpawnZone[] renderSpawnZones = Array.Empty<SpawnZone>();

        static Dictionary<string, int> savedMobNames;
        static bool initialized;

        public static void Init()
        {
            if (initialized) return;
            initialized = true;
            savedMobNames = new Dictionary<string, int>();
            spawnZones = new List<SpawnZone>();

        }

        public static void Load(Save aSave)
        {
            for (int i = 0; i < spawnZones.Count; i++)
            {
                spawnZones[i].RemoveAllPlates();
            }
            spawnZones.Clear();
            SavedMobData[] unitData = ImportUnitData(aSave);
            ImportZones(aSave, unitData);
        }

        public static void LoadFromTokens(IReadOnlyList<JToken> zoneTokens, IReadOnlyList<JToken> mobTokens, JsonSerializer serializer)
        {
            for (int i = 0; i < spawnZones.Count; i++)
            {
                spawnZones[i].RemoveAllPlates();
            }
            spawnZones.Clear();

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

        static void ImportZones(Save aSave, SavedMobData[] aUnitData) //TODO: This doesn't load spawners that doesnt have mobs in them
        {
            string path = aSave.SpawnZones;

            string[] files = System.IO.Directory.GetFiles(path);
            SavedMobData[] x = aUnitData.Distinct(new SpawnZoneComparer()).ToArray();
            int[] spawnZonesWithMobs = new int[x.Count()];
            for (int i = 0; i < spawnZonesWithMobs.Length; i++)
            {
                spawnZonesWithMobs[i] = x[i].SpawnZoneID;
            }
            //TODO: Make sure files are sorted
            for (int i = 0; i < files.Length; i++)
            {
                string lines = System.IO.File.ReadAllText(files[i]);

                string rawData = lines;

                if (Array.IndexOf(spawnZonesWithMobs, int.Parse(SaveManager.TrimToNameOnly(files[i]))) == -1)
                {
                    spawnZones.Add(SaveManager.ImportData<SpawnZone>(rawData));
                    continue;
                }

                spawnZones.Add(new SpawnZone(i, aUnitData.Where(x => x.SpawnZoneID == i).ToArray()));
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
            aSave.ClearFolder(aSave.SpawnZones);
            aSave.ClearFolder(aSave.NonFriendly);
            for (int i = 0; i < spawnZones.Count; i++)
            {
                SaveManager.ExportData(aSave.SpawnZones + "\\" + i + ".spawn", spawnZones[i]);
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
            List<SavedMobData> unitData = new List<SavedMobData>();

            string path = aSave.InWorld;

            string[] folders = System.IO.Directory.GetDirectories(path);

            for (int i = 0; i < folders.Length; i++)
            {
                string[] files = System.IO.Directory.GetFiles(folders[i]);
                for (int j = 0; j < files.Length; j++)
                {
                    string rawData = System.IO.File.ReadAllText(files[j]);
                    SavedMobData data = JsonConvert.DeserializeObject<SavedMobData>(rawData);
                    unitData.Add(data);
                }
            }
            return unitData.ToArray();
        }

        public static void CreateNewSpawnZone(string[] aMobNames)
        {
            SpawnZone spawnZone = new SpawnZone(spawnZones.Count, aMobNames);
            spawnZones.Add(spawnZone);
        }

        public static void CreateNewSpawner(int aId, MobPathing aPathing) => spawnZones[aId].CreateSpawner(aPathing);

        public static Spawner GetSpawner(int aZoneId, int aSpawnerId)
        {
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

        internal static bool Click(ClickEvent aClickEvent)
        {
            ThreadAffinity.AssertSimThread();
            for (int i = 0; i < spawnZones.Count; i++)
            {
                if (spawnZones[i].Click(aClickEvent)) return true;
            }
            return false;
        }

        public static void MinimapDraw(SpriteBatch aBatch, WorldSpace aOrigin, AbsoluteScreenPosition aMinimapOffset, AbsoluteScreenPosition aMinimapSize)
        {
            ThreadAffinity.AssertMainThread();
            SpawnZone[] snapshot = renderSpawnZones;
            for (int i = 0; i < snapshot.Length; i++)
            {
                snapshot[i].MinimapDraw(aBatch, aOrigin, aMinimapOffset, aMinimapSize);
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

        internal static void Draw(SpriteBatch aBatch)
        {
            ThreadAffinity.AssertMainThread();
            SpawnZone[] snapshot = renderSpawnZones;
            for (int i = 0; i < snapshot.Length; i++)
            {
                snapshot[i].Draw(aBatch);
            }
        }

        internal static void BuildRenderSnapshot()
        {
            renderSpawnZones = spawnZones.ToArray();
        }
    }
}
