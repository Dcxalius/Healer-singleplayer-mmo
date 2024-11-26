using Project_1.GameObjects.Entities;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Project_1.GameObjects.Spells
{
    internal class SpellEffect
    {
        static int GetId { get => nextId++; }
        static int nextId;
        public int Id { get; private set; }
        int id;
        public string Name { get => name; }
        string name;

        public SpellEffect(string aName)
        {
            id = GetId;
            name = aName;

            Debug.Assert(name != null, "No name");
        }

        public virtual bool Trigger(Entity aCaster, Entity aTarget)
        {
            

            return false;
        }
    }
}
