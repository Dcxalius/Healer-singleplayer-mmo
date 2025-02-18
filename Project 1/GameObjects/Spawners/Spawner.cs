using Microsoft.Xna.Framework.Graphics;
using Project_1.Camera;
using Project_1.GameObjects.Entities;
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
        int spawnZoneId;
        int id;

        NonFriendly spawn;

        MobData[] unitToSpawn;
        MobPathing pathing;

        SpawnGeometry spawnGeometry;

        double minSpawnTime;
        double maxSpawnTime;
        double nextSpawnTime;

        double lastSpawnDeathTime;

        public Spawner(int aSpawnZoneId, int aId, MobPathing aPathing, double aMinSpawnTime, double aMaxSpawnTime, SpawnGeometry aSpawnPlace, MobData aData, NonFriendly aUnit) : this(aSpawnZoneId, aId, aPathing, aMinSpawnTime, aMaxSpawnTime, aSpawnPlace, new MobData[] { aData })
        {
            spawn = aUnit;
        }


        public Spawner(int aSpawnZoneId, int aId, MobPathing aPathing, double aMinSpawnTime, double aMaxSpawnTime, SpawnGeometry aSpawnPlace, MobData[] aData, NonFriendly aUnit) : this(aSpawnZoneId, aId, aPathing, aMinSpawnTime, aMaxSpawnTime, aSpawnPlace, aData)
        {
            spawn = aUnit;
        }

        public Spawner(int aSpawnZoneId, int aId, MobPathing aPathing, double aMinSpawnTime, double aMaxSpawnTime, SpawnGeometry aSpawnPlace, MobData aData) : this(aSpawnZoneId, aId, aPathing, aMinSpawnTime, aMaxSpawnTime, aSpawnPlace, new MobData[] { aData }) {}
        public Spawner(int aSpawnZoneId, int aId, MobPathing aPathing, double aMinSpawnTime, double aMaxSpawnTime, SpawnGeometry aSpawnPlace, MobData[] aData)
        {
            spawnZoneId = aSpawnZoneId;
            id = aId;
            unitToSpawn = aData;
            spawnGeometry = aSpawnPlace;
            minSpawnTime = aMinSpawnTime;
            maxSpawnTime = aMaxSpawnTime;
            nextSpawnTime = RandomManager.RollDouble(minSpawnTime, maxSpawnTime);
            pathing = aPathing;


            lastSpawnDeathTime = double.NegativeInfinity;
        }

        public void Update()
        {
            if (spawn != null)
            {
                spawn.Update();
                if (spawn.Alive) return;
                lastSpawnDeathTime = TimeManager.TotalFrameTime;
                spawn = null;
                return;
            }

            Spawn();
        }

        void Spawn()
        {
            if (lastSpawnDeathTime + nextSpawnTime > TimeManager.TotalFrameTime) return;

            nextSpawnTime = RandomManager.RollDouble(minSpawnTime, maxSpawnTime);

            WorldSpace spawnPosition = spawnGeometry.GetNewSpawnPosition;
            pathing.Reset();
            if (unitToSpawn.Length == 1)
            {
                spawn = new NonFriendly(id, pathing, new UnitData(unitToSpawn[0], spawnPosition));

            }
            else
            {
                spawn = new NonFriendly(id, pathing, new UnitData(unitToSpawn[RandomManager.RollInt(unitToSpawn.Count())], spawnPosition));

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
