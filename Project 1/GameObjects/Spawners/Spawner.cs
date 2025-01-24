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
        enum Behaviour
        {
            Wander,
            Patrol
        };

        int spawnZoneId;
        int id;

        NonFriendly spawn;

        MobData[] unitToSpawn;

        SpawnGeometry xdd;

        double minSpawnTime;
        double maxSpawnTime;
        double nextSpawnTime;

        double lastSpawnDeathTime;

        public Spawner(int aSpawnZoneId, int aId, double aMinSpawnTime, double aMaxSpawnTime, SpawnGeometry aSpawnPlace, MobData aData, NonFriendly aUnit) : this(aSpawnZoneId, aId, aMinSpawnTime, aMaxSpawnTime, aSpawnPlace, new MobData[] { aData })
        {
            spawn = aUnit;
        }


        public Spawner(int aSpawnZoneId, int aId, double aMinSpawnTime, double aMaxSpawnTime, SpawnGeometry aSpawnPlace, MobData[] aData, NonFriendly aUnit) : this(aSpawnZoneId, aId, aMinSpawnTime, aMaxSpawnTime, aSpawnPlace, aData)
        {
            spawn = aUnit;
        }

        public Spawner(int aSpawnZoneId, int aId, double aMinSpawnTime, double aMaxSpawnTime, SpawnGeometry aSpawnPlace, MobData aData) : this(aSpawnZoneId, aId, aMinSpawnTime, aMaxSpawnTime, aSpawnPlace, new MobData[] { aData }) {}
        public Spawner(int aSpawnZoneId, int aId, double aMinSpawnTime, double aMaxSpawnTime, SpawnGeometry aSpawnPlace, MobData[] aData)
        {
            spawnZoneId = aSpawnZoneId;
            id = aId;
            unitToSpawn = aData;
            xdd = aSpawnPlace;
            minSpawnTime = aMinSpawnTime;
            maxSpawnTime = aMaxSpawnTime;
            nextSpawnTime = RandomManager.RollDouble(minSpawnTime, maxSpawnTime);


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

            if (unitToSpawn.Length == 1)
            {
                spawn = new NonFriendly(id, new UnitData(unitToSpawn[0], xdd.Position));

            }
            else
            {
                spawn = new NonFriendly(id, new UnitData(unitToSpawn[RandomManager.RollInt(unitToSpawn.Count())], xdd.Position));

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
    }
}
