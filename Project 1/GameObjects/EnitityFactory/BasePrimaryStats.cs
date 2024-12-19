using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Project_1.GameObjects.EnitityFactory
{
    internal class BasePrimaryStats
    {
        public int Strength { get => strength; }
        int strength;
        public int Agility { get => agility; }
        int agility;
        public int Intellect { get => intellect; }
        int intellect;
        public int Spirit { get => spirit; }
        int spirit;

        public int Stamina { get => stamina; }

        int stamina;

        public BasePrimaryStats(int aStrength, int aAgility, int aIntellect, int aSpirit, int aStamina)
        {
            strength = aStrength;
            agility = aAgility;
            intellect = aIntellect;
            spirit = aSpirit;
            stamina = aStamina;

            Debug.Assert(strength > 0 && agility > 0 && intellect > 0 && spirit > 0 && Stamina > 0, "Primary stat missing.");
        }

    }
}
