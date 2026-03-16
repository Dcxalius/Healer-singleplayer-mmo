using Newtonsoft.Json;
using Project_1.Camera;
using Project_1.GameObjects.Entities;
using Project_1.GameObjects.Spawners.Pathing;
using Project_1.GameObjects.Unit;
using Project_1.Managers;
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
        public int Id => id;

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
                if (value == null)
                {
                    return;
                }

                List<SavedMobData> units = unitsToFindMatchFor ?? new List<SavedMobData>();
                for (int i = 0; i < value.Length; i++)
                {
                    SavedMobData savedMobData = units.FirstOrDefault(x => x != null && x.SpawnerID == i);
                    if (savedMobData != null)
                    {
                        spawners.Add(new Spawner(id, i, value[i], debugMinSpawnTimer, debugMaxSpawnTimer, mobData, savedMobData));
                        continue;
                    }
                    spawners.Add(new Spawner(id, i, value[i], debugMinSpawnTimer, debugMaxSpawnTimer, mobData));
                }

                ApplySpawnerTimerData();
            }
        }

        [JsonProperty("SpawnerTimers", Order = 3)]
        SpawnerTimerSaveData[] SpawnerTimers
        {
            get
            {
                SpawnerTimerSaveData[] data = new SpawnerTimerSaveData[spawners.Count];
                for (int i = 0; i < spawners.Count; i++)
                {
                    data[i] = spawners[i].GetTimerData();
                }
                return data;
            }
            set
            {
                pendingSpawnerTimers = value ?? Array.Empty<SpawnerTimerSaveData>();
                ApplySpawnerTimerData();
            }
        }



        MobData[] mobData;
        SpawnerTimerSaveData[] pendingSpawnerTimers = Array.Empty<SpawnerTimerSaveData>();

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
            unitsToFindMatchFor = aUnits?.ToList() ?? new List<SavedMobData>();
            id = aId;
            spawners = new List<Spawner>();
        }

        internal void ApplySaveData(SpawnZoneSaveData data)
        {
            if (data == null) return;
            pendingSpawnerTimers = data.SpawnerTimers ?? Array.Empty<SpawnerTimerSaveData>();
            MobName = data.MobNames ?? Array.Empty<string>();
            MobPathings = data.Pathing ?? Array.Empty<MobPathing>();
        }

        void ApplySpawnerTimerData()
        {
            if (pendingSpawnerTimers == null || pendingSpawnerTimers.Length == 0) return;
            if (spawners == null || spawners.Count == 0) return;

            int count = Math.Min(spawners.Count, pendingSpawnerTimers.Length);
            for (int i = 0; i < count; i++)
            {
                spawners[i].ApplyTimerData(pendingSpawnerTimers[i]);
            }
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

        

        internal bool TryGetSpawnAt(WorldSpace worldPos, out Entity entity)
        {
            for (int i = 0; i < spawners.Count; i++)
            {
                if (spawners[i].TryGetSpawnAt(worldPos, out entity)) return true;
            }
            entity = null;
            return false;
        }

        internal bool TryGetSpawnByRenderId(int renderId, out Entity entity)
        {
            for (int i = 0; i < spawners.Count; i++)
            {
                if (spawners[i].TryGetSpawnByRenderId(renderId, out entity)) return true;
            }

            entity = null;
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

        internal void AppendMinimapDots(List<MinimapDotSnapshot> dots)
        {
            for (int i = 0; i < spawners.Count; i++) spawners[i].AppendMinimapDots(dots);
        }

        internal SpawnZoneRenderSnapshot BuildRenderSnapshot()
        {
            ThreadAffinity.AssertSimThread();
            SpawnerRenderSnapshot[] snapshots = new SpawnerRenderSnapshot[spawners.Count];
            for (int i = 0; i < spawners.Count; i++)
            {
                snapshots[i] = spawners[i].BuildRenderSnapshot();
            }
            return new SpawnZoneRenderSnapshot(id, snapshots);
        }

        
    }
}
