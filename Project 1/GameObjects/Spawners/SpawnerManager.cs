using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Newtonsoft.Json;
using Project_1.Camera;
using Project_1.GameObjects.Entities;
using Project_1.Input;
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

            CreateNewSpawnZone("Sheep");
            spawnZones[0].CreateSpawner(new SpawnPoint(new WorldSpace(50, 50)));
            spawnZones[0].CreateSpawner(new SpawnRectangle(new WorldSpace(500, 200), new WorldSpace(600, 300)));
        }

        static void ImportZones(ContentManager aContentManager)
        {
            string path = aContentManager.RootDirectory + "\\SaveData\\SpawnZones";


            string[] lines = System.IO.File.ReadAllLines(path);
            for (int i = 0; i < lines.Length; i++)
            {
                //TODO
                string rawData = lines[i];
                int id = i;
                string mobName;
                SpawnGeometry[] geo;
                NonFriendly[] xd;
            }
        }


        public static void CreateNewSpawnZone(string aMobName)
        {
            SpawnZone spawnZone = new SpawnZone(spawnZones.Count, aMobName);
            spawnZones.Add(spawnZone);
        }

        public static Spawner GetSpawner(int aZoneId, int aSpawnerId)
        {
            return spawnZones[aZoneId].GetSpawner(aSpawnerId);
        }

        internal static void Update()
        {
            for (int i = 0; i < spawnZones.Count; i++)
            {
                spawnZones[i].Update();
            }
        }

        public static void RefreshPlates()
        {
            for (int i = 0; i < spawnZones.Count; i++)
            {
                spawnZones[i].RefreshPlates();
            }
        }

        internal static void Draw(SpriteBatch aBatch)
        {
            for (int i = 0; i < spawnZones.Count; i++)
            {
                spawnZones[i].Draw(aBatch);
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
    }
}
