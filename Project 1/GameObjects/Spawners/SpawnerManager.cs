using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Newtonsoft.Json;
using Project_1.Camera;
using Project_1.GameObjects.Entities;
using Project_1.GameObjects.Spawners.Pathing;
using Project_1.Input;
using Project_1.Managers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Project_1.GameObjects.Spawners
{
    internal static class SpawnerManager
    {
        static List<SpawnZone> spawnZones;



        public static void Init(ContentManager aContentManager)
        {
            spawnZones = new List<SpawnZone>();
            ImportZones(aContentManager);

            //DEBUG

            //CreateNewSpawnZone(new string[] { "Sheep" });
            //spawnZones[0].CreateSpawner(new Patrol(new WorldSpace[] { new WorldSpace(50, 50), new WorldSpace(50, 150), new WorldSpace(150, 150), new WorldSpace(150, 50) }, Patrol.PatrolType.Circular, new WorldSpace(32, 16)));
            //spawnZones[0].CreateSpawner(new Bound(new WorldSpace(150, 150), 100));
            //spawnZones[0].CreateSpawner(new Wander(new Rectangle(new Point(500, 200), new Point(1000, 1000))));
        }

        static void ImportZones(ContentManager aContentManager)
        {
            //string path = aContentManager.RootDirectory + "\\SaveData\\World\\SpawnZones";

            //string[] files = System.IO.Directory.GetFiles(path);
            //for (int i = 0; i < files.Length; i++)
            //{
            //    string lines = System.IO.File.ReadAllText(files[i]);

            //    string rawData = lines;
            //    spawnZones.Add(SaveManager.ImportData<SpawnZone>(rawData));
            //}

        }

        internal static void Update()
        {
            for (int i = 0; i < spawnZones.Count; i++)
            {
                spawnZones[i].Update();
            }
        }

        public static void SaveData()
        {
            for (int i = 0; i < spawnZones.Count; i++)
            {
                SaveManager.ExportData("World\\SpawnZones\\" + i + ".spawn", spawnZones[i]);
            }
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
