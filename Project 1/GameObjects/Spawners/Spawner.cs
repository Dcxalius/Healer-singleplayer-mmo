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

        MobData[] unitsToSpawn;
        MobPathing pathing;


        double minSpawnTime;
        double maxSpawnTime;
        double nextSpawnTime;

        double lastSpawnDeathTime;

        public Spawner(int aSpawnZoneId, int aId, MobPathing aPathing, double aMinSpawnTime, double aMaxSpawnTime, MobData aData, NonFriendly aUnit) : this(aSpawnZoneId, aId, aPathing, aMinSpawnTime, aMaxSpawnTime, new MobData[] { aData })
        {
            spawn = aUnit;
        }


        public Spawner(int aSpawnZoneId, int aId, MobPathing aPathing, double aMinSpawnTime, double aMaxSpawnTime, MobData[] aData, NonFriendly aUnit) : this(aSpawnZoneId, aId, aPathing, aMinSpawnTime, aMaxSpawnTime, aData)
        {
            spawn = aUnit;
        }

        public Spawner(int aSpawnZoneId, int aId, MobPathing aPathing, double aMinSpawnTime, double aMaxSpawnTime, MobData aData) : this(aSpawnZoneId, aId, aPathing, aMinSpawnTime, aMaxSpawnTime, new MobData[] { aData }) {}
        public Spawner(int aSpawnZoneId, int aId, MobPathing aPathing, double aMinSpawnTime, double aMaxSpawnTime, MobData[] aData)
        {
            spawnZoneId = aSpawnZoneId;
            id = aId;
            unitsToSpawn = aData;
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

            if (unitsToSpawn.Length == 1)
            {
                spawn = new NonFriendly(id, pathing, new UnitData(unitsToSpawn[0], pathing.NewSpawn(unitsToSpawn[0].Size)));

            }
            else
            {
                MobData unitToSpawn = unitsToSpawn[RandomManager.RollInt(unitsToSpawn.Count())];
                spawn = new NonFriendly(id, pathing, new UnitData(unitToSpawn, pathing.NewSpawn(unitToSpawn.Size)));

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
