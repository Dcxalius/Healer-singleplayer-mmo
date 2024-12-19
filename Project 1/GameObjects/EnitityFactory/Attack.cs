using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Project_1.GameObjects.EnitityFactory
{
    internal class Attack
    {

        public float SecondsPerAttack => secondsPerAttack;
        float secondsPerAttack;

        public float Damage => attackDamage;
        float attackDamage;

        public float Range => attackRange;
        float attackRange;

        public Attack(float aAttackRange, float aAttackDamage, float aSecondsPerAttack)
        {
            this.attackRange = aAttackRange;
            attackDamage = aAttackDamage;
            attackRange = aAttackRange;

            Debug.Assert(secondsPerAttack >= 0 && attackDamage >= 0 && attackRange >= 0);
        }
    }
}
