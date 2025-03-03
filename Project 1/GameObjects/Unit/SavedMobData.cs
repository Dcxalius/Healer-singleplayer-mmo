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

        int spawnerID;
        int spawnZoneID;

        public SavedMobData(int aSpawnerID, int aSpawnZoneID, MobData aData, WorldSpace aSpawn) : base(aData, aSpawn)
        {
            this.spawnerID = aSpawnerID;
            this.spawnZoneID = aSpawnZoneID;
        }

        public SavedMobData(string name, int spawnerID, int spawnZoneID, string corpseGfxName, string className, Relation.RelationToPlayer? relation, int level, int experience, float currentHp, float currentResource, int?[] equipment, WorldSpace position, WorldSpace momentum, WorldSpace velocity, List<WorldSpace> destinations) : base(name, corpseGfxName, className, relation, level, experience, currentHp, currentResource, equipment, position, momentum, velocity, destinations)
        {
            this.spawnerID = spawnerID;
            this.spawnZoneID = spawnZoneID;
        }
    }
}
