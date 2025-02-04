using Microsoft.Xna.Framework.Graphics;
using Project_1.GameObjects.Entities;
using Project_1.Input;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Project_1.GameObjects.Spawners
{
    internal class SpawnZone
    {
        List<Spawner> spawners;
        int id;

        MobData mobData;

        const double debugMinSpawnTimer = 5000; //TODO: Change this
        const double debugMaxSpawnTimer = 10000;

        public SpawnZone(int aId, string aMobName)
        {
            id = aId;
            spawners = new List<Spawner>();
            mobData = ObjectFactory.GetMobData(aMobName);
        }

        public SpawnZone(int aId, string aMobName, SpawnGeometry[] aGeometries, NonFriendly[] aUnits)
        {

            id = aId;
            spawners = new List<Spawner>();
            mobData = ObjectFactory.GetMobData(aMobName);

            Debug.Assert(aGeometries.Length == aUnits.Length);
            for (int i = 0; i < aGeometries.Length; i++)
            {
                CreateSpawner(aGeometries[i], aUnits[i]);
            }
        }

        void CreateSpawner(SpawnGeometry aSpawnGeometry, NonFriendly aUnit)
        {
            Spawner s = new Spawner(id, spawners.Count, debugMinSpawnTimer, debugMaxSpawnTimer, aSpawnGeometry, mobData, aUnit);
            spawners.Add(s);
        }

        public void CreateSpawner(SpawnGeometry aSpawnGeometry)
        {
            Spawner s = new Spawner(id, spawners.Count, debugMinSpawnTimer, debugMaxSpawnTimer, aSpawnGeometry, mobData);
            spawners.Add(s);
        }

        public Spawner GetSpawner(int aId)
        {
            return spawners[aId];
        }

        internal void Update()
        {
            for (int i = 0; i < spawners.Count; i++)
            {
                spawners[i].Update();
            }
        }

        internal void Draw(SpriteBatch aBatch)
        {
            for (int i = 0; i < spawners.Count; i++)
            {
                spawners[i].Draw(aBatch);
            }
        }

        internal bool Click(ClickEvent aClickEvent)
        {
            for (int i = 0; i < spawners.Count; i++)
            {
                if (spawners[i].Click(aClickEvent)) return true;
            }
            return false;
        }

        internal void RefreshPlates()
        {
            for (int i = 0; i < spawners.Count; i++)
            {
                spawners[i].RefreshPlates();
            }
        }
    }
}
