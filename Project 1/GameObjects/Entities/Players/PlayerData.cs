using Microsoft.Xna.Framework;
using Newtonsoft.Json;
using Project_1.Camera;
using Project_1.GameObjects.Spells;
using Project_1.GameObjects.Unit;
using Project_1.Items;
using Project_1.UI.HUD;
using Project_1.UI.UIElements.SpellBook;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Project_1.GameObjects.Entities.Players
{
    internal class PlayerData : UnitData
    {
        public string[] Party => party;
        string[] party;

        public Inventory Inventory => inventory;
        Inventory inventory;

        [JsonProperty("LearntSpells")]
        string[] LearntSpellNames => spellBook.LearntSpells;

        [JsonIgnore]
        public SpellBook SpellBook => spellBook;
        SpellBook spellBook;

        [JsonProperty("SpellOnBar")]
        string[] spellsOnBar => HUDManager.SaveSpellBar;

        [JsonIgnore]
        public string[] SavedSpellsOnBar => spellOnBar;
        string[] spellOnBar;

        [JsonConstructor]
        public PlayerData(string name, string corpseGfxName, string className, Relation.RelationToPlayer? relation, string[] party, 
            int level, int experience, float currentHp, float currentResource, int?[] equipment, Inventory inventory, string[] learntSpells, string[] spellOnBar, 
            WorldSpace position, WorldSpace momentum, WorldSpace velocity, List<WorldSpace> destinations)
            : base(name, corpseGfxName, className, relation, level, experience, currentHp, currentResource, equipment, position, momentum, velocity, destinations)
        {
            spellBook = new SpellBook(learntSpells);
            this.spellOnBar = spellOnBar;

            this.inventory = inventory;

            this.party = party;
        }

        public PlayerData(string aName, string aClassName) : this(aName, null, aClassName, Relation.RelationToPlayer.Self, new string[] { },
            1, 0, float.MaxValue, float.MaxValue, null, null, null, null,
            new WorldSpace(500, 500) /*TODO: Remove hardcoded*/ , new WorldSpace(0, 0), new WorldSpace(0, 0), new List<WorldSpace>() { })
        {
            inventory = new Inventory();
            spellBook = new SpellBook();

        }
    }
}
