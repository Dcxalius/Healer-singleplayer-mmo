using Microsoft.Xna.Framework;
using Newtonsoft.Json;
using Project_1.Items;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace Project_1.GameObjects.Entities
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

        public float Speed { get => speed; }
        public float MaxSpeed { get => maxSpeed; }

        public float SecondsPerAttack { get => secondsPerAttack; }
        public float AttackDamage { get => attackDamage; }
        public float AttackRange { get => attackRange; }

        string name;
        float maxHealth;
        float currentHealth;
        RelationToPlayer relationToPlayer;
        float speed;
        float maxSpeed;
        float secondsPerAttack;
        float attackDamage;
        float attackRange;

        public LootTable LootTable { get => lootTable; }
        LootTable lootTable;

        [JsonConstructor]
        public UnitData(string name, float maxHealth, RelationToPlayer? relation, float speed, float maxSpeed, float secondsPerAttack, float attackDamage, float attackRange)
        {

            this.name = name;
            this.maxHealth = maxHealth;
            currentHealth = maxHealth;
            Debug.Assert(relation.HasValue);
            relationToPlayer = relation.Value;

            this.speed = speed;
            this.maxSpeed = maxSpeed;
            this.secondsPerAttack = secondsPerAttack;
            this.attackDamage = attackDamage;
            this.attackRange = attackRange;

            Loot[] loots = new Loot[5];
            loots[0] = new Loot(ItemFactory.GetItemData("Small Bag"), 1, (1, 1));
            for (int i = 1; i < loots.Length; i++)
            {
                loots[i] = new Loot(ItemFactory.GetItemData("Poop"), 10, (1, 5));
            }
            lootTable = new LootTable(loots, 1, 5);
            Assert();
        }

        void Assert()
        {

            if (name == null || maxHealth <= 0 || speed <= 0 || secondsPerAttack <= 0 || attackDamage <= 0 || attackRange <= 0)
            {
                throw new Exception("UnitData improperly set");
            }
        }
    }
}
