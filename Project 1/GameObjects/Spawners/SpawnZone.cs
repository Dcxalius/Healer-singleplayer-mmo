using Microsoft.Xna.Framework.Graphics;
using Newtonsoft.Json;
using Project_1.GameObjects.Entities;
using Project_1.GameObjects.Spawners.Pathing;
using Project_1.GameObjects.Unit;
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
        //[JsonProperty("Spawners")]
        List<Spawner> spawners;

        [JsonProperty]
        int id;

        [JsonProperty("MobNames", Order = 1)]
        string[] MobName
        {
            get
            {
                string[] names = new string[mobData.Length];
                for (int i = 0; i < mobData.Length; i++)
                {
                    names[i] = mobData[i].Name;
                }
                return names;
            }
            set
            {
                mobData = new MobData[value.Length];
                for (int i = 0; i < value.Length; i++)
                {
                    mobData[i] = ObjectFactory.GetMobData(value[i]);

                }
            }
        }

        [JsonProperty("Pathing", TypeNameHandling = TypeNameHandling.Auto, Order = 2)] //TODO: Remove all of this and rely solely on json construction of spawners, do this after different spawn times have been implemented
        MobPathing[] MobPathings
        {
            get
            {
                MobPathing[] returnable = new MobPathing[spawners.Count];
                for (int i = 0; i < returnable.Length; i++)
                {
                    returnable[i] = spawners[i].Pathing;
                }
                return returnable;
            }
            set
            {
                for (int i = 0; i < unitsToFindMatchFor.Count; i++)
                {
                    SavedMobData savedMobData = unitsToFindMatchFor.FirstOrDefault(x => x.SpawnerID == i);
                    if (savedMobData != null)
                    {
                        spawners.Add(new Spawner(id, i, value[i], debugMinSpawnTimer, debugMaxSpawnTimer, mobData, savedMobData));
                        continue;
                    }
                    spawners.Add(new Spawner(id, i, value[i], debugMinSpawnTimer, debugMaxSpawnTimer, mobData));
                }
            }
        }



        MobData[] mobData;

        const double debugMinSpawnTimer = 5000; //TODO: Change this
        const double debugMaxSpawnTimer = 10000;


        public SpawnZone(int aId, string[] aMobName) : this(aId, aMobName, new MobPathing[] { }) { }

        [JsonConstructor]
        public SpawnZone(int id, string[] mobName, MobPathing[] pathing)
        {
            this.id = id;
            spawners = new List<Spawner>();
            mobData = new MobData[mobName.Length];
            for (int i = 0; i < mobName.Length; i++)
            {
                mobData[i] = ObjectFactory.GetMobData(mobName[i]);

            }

            for (int i = 0; i < pathing.Length; i++)
            {
                CreateSpawner(pathing[i]);
            }
        }

        //public SpawnZone(int aId, string[] aMobName, MobPathing[] aPathing, NonFriendly[] aUnits)
        //{
        //    id = aId;
        //    spawners = new List<Spawner>();

        //    mobData = new MobData[aMobName.Length];
        //    for (int i = 0; i < aMobName.Length; i++)
        //    {
        //        mobData[i] = ObjectFactory.GetMobData(aMobName[i]);

        //    }

        //    Debug.Assert(aPathing.Length >= aUnits.Length);
        //    for (int i = 0; i < aPathing.Length; i++)
        //    {
        //        CreateSpawner(aPathing[i], aUnits[i]);
        //    }
        //}
        List<SavedMobData> unitsToFindMatchFor;

        public SpawnZone(int aId, SavedMobData[] aUnits)
        {
            unitsToFindMatchFor = aUnits.ToList();
            id = aId;
            spawners = new List<Spawner>();
        }

        //public void AddMobToSpawn(MobData aData) => AddMobsToSpawn(new MobData[] { aData });

        //public void AddMobsToSpawn(MobData[] aData)
        //{
        //    mobData
        //}

        void CreateSpawner(MobPathing aPath, SavedMobData aUnit)
        {
            Spawner s = new Spawner(id, spawners.Count, aPath, debugMinSpawnTimer, debugMaxSpawnTimer, mobData, aUnit);
            spawners.Add(s);
        }

        public void CreateSpawner(MobPathing aPath)
        {
            Spawner s = new Spawner(id, spawners.Count, aPath, debugMinSpawnTimer, debugMaxSpawnTimer, mobData);
            spawners.Add(s);
        }

        public void RemoveAllPlates()
        {
            for (int i = 0; i < spawners.Count; i++)
            {
                spawners[i].RemovePlates();
            }
        }

        public Spawner GetSpawner(int aId)
        {
            return spawners[aId];
        }

        internal void Update() 
        {
            for (int i = 0; i < spawners.Count; i++) spawners[i].Update();
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

        public SavedMobData[] GetSavedMobData()
        {
            List<SavedMobData> savedMobData = new List<SavedMobData>();
            for (int i = 0; i < spawners.Count; i++)
            {
                savedMobData.Add(spawners[i].GetSavedMobData());
            }
            return savedMobData.ToArray();
        }

        internal void Draw(SpriteBatch aBatch)
        {
            for (int i = 0; i < spawners.Count; i++) spawners[i].Draw(aBatch);
        }
    }
}
