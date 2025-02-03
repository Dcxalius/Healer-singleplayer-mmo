using Microsoft.Xna.Framework;
using Project_1.GameObjects.Unit;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Project_1.GameObjects.Entities.Players
{
    internal class PlayerData : UnitData
    {
        public string[] Party => party;
        string[] party;
        public PlayerData(string name, string corpseGfxName, string className, Relation.RelationToPlayer? relation, string[] party, 
            int level, int experience, float currentHp, float currentResource, int?[] equipment, 
            Vector2 position, Vector2 momentum, Vector2 velocity)
            : base(name, corpseGfxName, className, relation, level, experience, currentHp, currentResource, equipment, position, momentum, velocity)
        {
            this.party = party;
        }
    }
}
