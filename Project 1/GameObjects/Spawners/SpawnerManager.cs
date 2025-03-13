using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Newtonsoft.Json;
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

namespace Project_1.GameObjects.Spawners
{
    internal static class SpawnerManager
    {
        static List<SpawnZone> spawnZones;

        static Dictionary<string, int> savedMobNames;

        public static void Init(ContentManager aContentManager)
        {
            savedMobNames = new Dictionary<string, int>();
            spawnZones = new List<SpawnZone>();
            SavedMobData[] unitData = ImportUnitData(aContentManager);
            ImportZones(aContentManager, unitData);
        }

        static void ImportZones(ContentManager aContentManager, SavedMobData[] aUnitData)
        {
            string path = aContentManager.RootDirectory + "\\SaveData\\World\\SpawnZones";

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
                    int nrOfCopies = 0;
                    string name = savedMobData[j].Name;

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

        static SavedMobData[] ImportUnitData(ContentManager aContentManager)
        {
            List<SavedMobData> unitData = new List<SavedMobData>();

            string path = aContentManager.RootDirectory + "\\SaveData\\Units\\InWorld";

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

        public static Spawner GetSpawner(int aZoneId, int aSpawnerId)
        {
            return spawnZones[aZoneId].GetSpawner(aSpawnerId);
        }

        public static void RefreshPlates()
        {
            for (int i = 0; i < spawnZones.Count; i++)
            {
                spawnZones[i].RefreshPlates();
            }
        }

        internal static bool Click(ClickEvent aClickEvent)
        {
            for (int i = 0; i < spawnZones.Count; i++)
            {
                if (spawnZones[i].Click(aClickEvent)) return true;
            }
            return false;
        }

        internal static void Draw(SpriteBatch aBatch)
        {
            for (int i = 0; i < spawnZones.Count; i++)
            {
                spawnZones[i].Draw(aBatch);
            }
        }
    }
}
