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

        MobData unitToSpawn;

        SpawnGeometry xdd;

        double spawnTime;

        double deathtimeSpawn;

        public Spawner(int aSpawnZoneId, int aId, SpawnGeometry aSpawnPlace, MobData aData, NonFriendly aUnit)
        {
            spawnZoneId = aSpawnZoneId;
            id = aId;
            xdd = aSpawnPlace;
            spawn = aUnit;
            unitToSpawn = aData;


            spawnTime = 5000;
            deathtimeSpawn = double.NegativeInfinity;
        }

        public Spawner(int aSpawnZoneId, int aId, SpawnGeometry aSpawnPlace, MobData aData)
        {
            spawnZoneId = aSpawnZoneId;
            id = aId;
            unitToSpawn = aData;
            xdd = aSpawnPlace;


            spawnTime = 5000;
            deathtimeSpawn = double.NegativeInfinity;
        }

        public void Update()
        {
            if (spawn != null)
            {
                spawn.Update();
                if (spawn.Alive) return;
                deathtimeSpawn = TimeManager.TotalFrameTime;
                spawn = null;
                return;
            }

            Spawn();
        }

        void Spawn()
        {
            if (deathtimeSpawn + spawnTime > TimeManager.TotalFrameTime) return;

            spawn = new NonFriendly(id, new UnitData(unitToSpawn), xdd.Position);
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
