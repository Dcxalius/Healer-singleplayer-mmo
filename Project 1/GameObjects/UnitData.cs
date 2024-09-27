using Microsoft.Xna.Framework;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Project_1.GameObjects
{
    struct UnitData
    {
        public enum RelationToPlayer
        {
            Self,
            Friendly,
            Neutral,
            Hostile
        }

        static Color[] RelationColors = new Color[] { Color.AliceBlue, Color.LightSeaGreen, Color.Yellow, Color.IndianRed };
        public Color RelationColor() { return RelationColors[(int)relationToPlayer]; }


        public string Name { get => name; }
        public float MaxHealth { get => maxHealth; set => maxHealth = value; }

        [JsonIgnore]
        public float CurrentHealth
        {
            get => currentHealth;
            set
            {
                if (value >= maxHealth)
                {
                    currentHealth = maxHealth;
                    return;
                }
                currentHealth = value;
            }
        }

        public RelationToPlayer Relation { get => relationToPlayer; set => relationToPlayer = value; }  

        public float Speed { get  => speed; set => speed = value; }

        string name;
        float maxHealth;
        float currentHealth;
        RelationToPlayer relationToPlayer;
        float speed;


        [JsonConstructor]
        public UnitData(string name, float maxHealth, RelationToPlayer? relation, float speed) 
        {
            
            this.name = name;
            this.maxHealth = maxHealth;
            Debug.Assert(relation.HasValue);
            
            this.relationToPlayer = relation.Value;
            currentHealth = maxHealth;
            this.speed = speed;
            Assert();
        }

        void Assert()
        {

            if (name == null || maxHealth <= 0 || speed <= 0)
            {
                throw new Exception("UnitData improperly set");
            }
        }
    }
}
