using Project_1.GameObjects.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Project_1.GameObjects.Spells
{
    internal class SpellEffect
    {
        static int GetId { get => nextId++; }
        static int nextId;
        int id;
        string name;

        public SpellEffect(string aName)
        {
            id = GetId;
            name = aName;
        }
    }
}
