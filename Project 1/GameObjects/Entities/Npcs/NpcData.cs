using Newtonsoft.Json;
using Project_1.Camera;
using Project_1.GameObjects.Unit;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Project_1.GameObjects.Entities.Npcs
{
    internal class NpcData : UnitData
    {

        [JsonIgnore]
        public Items.SubTypes.Equipment[] Sellables => sellables;

        Items.SubTypes.Equipment[] sellables;

        

        [JsonConstructor]
        public NpcData(string name, string corpseGfxName, string className, Relation.RelationToPlayer? relation, int level, int experience, float currentHp, float currentResource, int?[] equipment, WorldSpace position, WorldSpace momentum, WorldSpace velocity, List<WorldSpace> destinations) : base(name, corpseGfxName, className, relation, level, experience, currentHp, currentResource, equipment, position, momentum, velocity, destinations)
        {
        }
    }
}
