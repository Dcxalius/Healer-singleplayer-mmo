using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Project_1.GameObjects.Unit
{
    internal struct AttackData
    {
        public enum AttackStyle
        {
            None,
            OneHander,
            TwoHander,
            DualWielding
        }

        public int AttackPower
        {
            set
            {
                if (mainHandAttack != null) mainHandAttack.AttackPower = value;
                if (offHandAttack != null) offHandAttack.AttackPower = value;
            }
        }

        public AttackStyle Style => attackStyle;
        AttackStyle attackStyle;



        public Attack MainHandAttack => mainHandAttack;
        Attack mainHandAttack;

        public Attack OffHandAttack => offHandAttack;
        Attack offHandAttack;

        public AttackData(AttackStyle aStyle, Attack aMainHandAttack, Attack aOffHandAttack)
        {
            attackStyle = aStyle;
            mainHandAttack = aMainHandAttack;
            offHandAttack = aOffHandAttack;
        }
    }
}
