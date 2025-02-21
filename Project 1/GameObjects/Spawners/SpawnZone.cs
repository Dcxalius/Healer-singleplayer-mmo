using Microsoft.Xna.Framework.Graphics;
using Project_1.GameObjects.Entities;
using Project_1.Input;
using System;
using System.CodeDom;
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

        public SpawnZone(int aId, string aMobName, MobPathing[] aPathing, NonFriendly[] aUnits)
        {

            id = aId;
            spawners = new List<Spawner>();
            mobData = ObjectFactory.GetMobData(aMobName);

            Debug.Assert(aPathing.Length == aUnits.Length);
            for (int i = 0; i < aPathing.Length; i++)
            {
                CreateSpawner(aPathing[i], aUnits[i]);
            }
        }

        void CreateSpawner(MobPathing aPath, NonFriendly aUnit)
        {
            Spawner s = new Spawner(id, spawners.Count, aPath, debugMinSpawnTimer, debugMaxSpawnTimer, mobData, aUnit);
            spawners.Add(s);
        }

        public void CreateSpawner(MobPathing aPath)
        {
            Spawner s = new Spawner(id, spawners.Count, aPath, debugMinSpawnTimer, debugMaxSpawnTimer, mobData);
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
