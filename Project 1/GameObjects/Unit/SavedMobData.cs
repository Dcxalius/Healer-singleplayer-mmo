using Newtonsoft.Json;
using Project_1.Camera;
using Project_1.GameObjects.Spawners;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Project_1.GameObjects.Unit
{
    internal class SavedMobData : UnitData //TODO: Think about name
    {
        public int SpawnerID => spawnerID;
        int spawnerID;

        public int SpawnZoneID => spawnZoneID;
        int spawnZoneID;

        public SavedMobData(int aSpawnerID, int aSpawnZoneID, MobData aData, WorldSpace aSpawn) : base(aData, aSpawn)
        {
            this.spawnerID = aSpawnerID;
            this.spawnZoneID = aSpawnZoneID;
        }

        

        [JsonConstructor]
        public SavedMobData(string name, int spawnerID, int spawnZoneID, int level, float currentHp, float currentResource, int?[] equipment, WorldSpace position, WorldSpace momentum, WorldSpace velocity, List<WorldSpace> destinations) : base(name, ObjectFactory.GetMobData(name).CorpseGfxPath.Name, ObjectFactory.GetMobData(name).ClassData.Name, ObjectFactory.GetMobData(name).RelationData.ToPlayer, level, 0, currentHp, currentResource, equipment, position, momentum, velocity, destinations)
        {
            this.spawnerID = spawnerID;
            this.spawnZoneID = spawnZoneID;
        }
    }
}
