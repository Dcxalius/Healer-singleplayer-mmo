﻿using Project_1.GameObjects.Spells;
using Project_1.UI.HUD;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Project_1.GameObjects.Entities.Players
{
    internal class SpellBook
    {
        public Spell[] Spells { get => knownSpells.ToArray(); }
        List<Spell> knownSpells;
        Player player;


        public SpellBook(Player aPlayer)
        {
            knownSpells = new List<Spell>();
            player = aPlayer;
            AddSpell(new Spell("Heal", player));
            AddSpell(new Spell("Renew", player));
        }

        public void AddSpell(Spell aSpell)
        {
            knownSpells.Add(aSpell);
            //HUDManager.AddSpellToSpellBook(aSpell);
        }

        public bool HasSpell(Spell aSpell)
        {
            return knownSpells.Contains(aSpell);
        }
    }
}
