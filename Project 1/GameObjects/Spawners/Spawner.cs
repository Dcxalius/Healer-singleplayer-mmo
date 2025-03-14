using Microsoft.Xna.Framework.Graphics;
using Newtonsoft.Json;
using Project_1.Camera;
using Project_1.GameObjects.Entities;
using Project_1.GameObjects.Spawners.Pathing;
using Project_1.GameObjects.Unit;
using Project_1.Input;
using Project_1.Managers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Project_1.GameObjects.Spawners
{
    internal class Spawner
    {
        [JsonProperty]
        int SpawnZoneId => spawnZoneId;
        int spawnZoneId;
        [JsonProperty]
        int ID => id;
        int id;

        NonFriendly spawn;

        MobData[] unitsToSpawn;
        [JsonProperty]
        public MobPathing Pathing => pathing;
        MobPathing pathing;

        [JsonProperty]
        double MinSpawnTime => minSpawnTime;
        double minSpawnTime;
        [JsonProperty]
        double MaxSpawnTime => maxSpawnTime;
        double maxSpawnTime;
        [JsonProperty]
        double NextSpawnTime => nextSpawnTime;
        double nextSpawnTime;

        [JsonProperty]
        double TimeSinceLastDeath => timeSinceLastDeath;
        double timeSinceLastDeath;

        public Spawner(int aSpawnZoneId, int aId, MobPathing aPathing, double aMinSpawnTime, double aMaxSpawnTime, MobData aData, SavedMobData aUnit) : this(aSpawnZoneId, aId, aPathing, aMinSpawnTime, aMaxSpawnTime, new MobData[] { aData })
        {
            spawn = new NonFriendly(aPathing, aUnit);
        }


        public Spawner(int aSpawnZoneId, int aId, MobPathing aPathing, double aMinSpawnTime, double aMaxSpawnTime, MobData[] aData, SavedMobData aUnit) : this(aSpawnZoneId, aId, aPathing, aMinSpawnTime, aMaxSpawnTime, aData)
        {
            spawn = new NonFriendly(aPathing, aUnit);
        }

        public Spawner(int aSpawnZoneId, int aId, MobPathing aPathing, double aMinSpawnTime, double aMaxSpawnTime, MobData aData) : this(aSpawnZoneId, aId, aPathing, aMinSpawnTime, aMaxSpawnTime, new MobData[] { aData }) { }
        public Spawner(int aSpawnZoneId, int aId, MobPathing aPathing, double aMinSpawnTime, double aMaxSpawnTime, MobData[] aData)
        {
            spawnZoneId = aSpawnZoneId;
            id = aId;
            unitsToSpawn = aData;
            minSpawnTime = aMinSpawnTime;
            maxSpawnTime = aMaxSpawnTime;
            nextSpawnTime = RandomManager.RollDouble(minSpawnTime, maxSpawnTime);
            pathing = aPathing;


            timeSinceLastDeath = double.NegativeInfinity;
        }

        public void RemovePlates()
        {
            if (spawn != null) spawn.Delete();
        }

        public void Update()
        {
            if (spawn != null)
            {
                spawn.Update();
                if (spawn.Alive) return;
                timeSinceLastDeath = TimeManager.TotalFrameTime;
                spawn = null;
                return;
            }

            Spawn();
        }

        public SavedMobData GetSavedMobData()
        {
            if (spawn == null) return null;
            return spawn.SavedMobData;
        }

        void Spawn()
        {
            if (timeSinceLastDeath + nextSpawnTime > TimeManager.TotalFrameTime) return;

            nextSpawnTime = RandomManager.RollDouble(minSpawnTime, maxSpawnTime);

            if (unitsToSpawn.Length == 1)
            {
                spawn = new NonFriendly(pathing, new SavedMobData(id, spawnZoneId, unitsToSpawn[0], pathing.NewSpawn(unitsToSpawn[0].Size)));

            }
            else
            {
                MobData unitToSpawn = unitsToSpawn[RandomManager.RollInt(unitsToSpawn.Count())];
                spawn = new NonFriendly(pathing, new SavedMobData(id, spawnZoneId, unitToSpawn, pathing.NewSpawn(unitToSpawn.Size)));

            }
        }

        internal void Draw(SpriteBatch aBatch)
        {
            if (spawn == null) return;

            spawn.Draw(aBatch);
        }

        internal bool Click(ClickEvent aClickEvent)
        {
            if (spawn == null) return false;

            return spawn.Click(aClickEvent);
        }

        internal void RefreshPlates()
        {
            if (spawn == null) return;

            spawn.RefreshPlates();
        }
    }
}
