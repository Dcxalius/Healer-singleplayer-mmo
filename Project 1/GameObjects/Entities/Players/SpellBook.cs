using Project_1.GameObjects.Spells;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Project_1.GameObjects.Entities.Players
{
    internal class SpellBook
    {
        List<Spell> knownSpells;
        Player player;


        public SpellBook(Player aPlayer)
        {
            knownSpells = new List<Spell>();
            player = aPlayer;
            knownSpells.Add(new Spell("", player));
        }
    }
}
