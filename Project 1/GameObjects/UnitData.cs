using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Project_1.GameObjects
{
    struct UnitData
    {
        string name;
        float maxHealth;
        float currentHealth;

        public UnitData(string aName, float aMaxHealth) 
        {
            name = aName;
            maxHealth = aMaxHealth;
            currentHealth = aMaxHealth;
        }
    }
}
