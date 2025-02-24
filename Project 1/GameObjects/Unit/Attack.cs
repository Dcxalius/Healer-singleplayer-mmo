using Project_1.Managers;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Project_1.GameObjects.Unit
{
    internal class Attack
    {
        public enum Type
        {
            Melee,
            Range
        }

        public float GetAttackDamage => (float)RandomManager.RollDouble(Damage.Item1, Damage.Item2);


        public float AttackPower { set => attackPower = value; }
        float attackPower;

        public float SecondsPerAttack => secondsPerAttack;
        float secondsPerAttack;

        public float dps => ((Damage.Item1 + Damage.Item2) / 2) / secondsPerAttack;
        public (float, float) Damage => (attackDamageMin + (attackPower / 14) * secondsPerAttack, attackDamageMax + (attackPower / 14) * secondsPerAttack);
        float attackDamageMin;
        float attackDamageMax;

        public float Range
        {
            get
            {
                if (type == Type.Melee)
                {
                    return meleeAR;
                }

                return rangeAR;
            }
        }

        const float meleeAR = 50;
        const float rangeAR = 500;
        Type type;


        public Attack(float aDmgMin, float aDmgMax, float aSecondsPerAttack)
        {
            attackDamageMin = aDmgMin;
            attackDamageMax = aDmgMax;
            secondsPerAttack = aSecondsPerAttack;

            //Debug.Assert(secondsPerAttack > 0 && attackDamageMin > 0 && attackDamageMax > 0);
        }
    }
}
