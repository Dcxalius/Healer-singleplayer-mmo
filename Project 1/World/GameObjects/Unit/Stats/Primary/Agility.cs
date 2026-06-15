using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Project_1.GameObjects.Unit.Classes;

namespace Project_1.World.GameObjects.Unit.Stats.Primary
{
    internal class Agility : Stat
    {
        /*Blue post
        Agility increases the chance of a critical hit with melee and ranged attacks. 
        The amount of the increase is dependent on both class and level. 
        For most level 60 character classes, approximately 20 points of Agility will increase your critical hit chance by approximately 1%. 
        Rogues require 29 Agility for an additional 1% critical hit chance, and Hunters require 53 Agility for an additional 1% critical hit chance, but both of these classes also gain attack power from agility, and the items available to them typically have much higher amounts of Agility.

        Agility increases the chance to dodge an attack. 
        The amount increased is dependant on both class and level. 
        For most level 60 character classes, approximately 20 points of AGI will increase your chance to dodge by approximately 1%. 
        Rogues only require 14.5 AGI for an additional 1% dodge chance. 
        Hunters require 26.5 AGI for an additional 1% dodge chance, but Hunters typically have a high amount of agility, as well as an Aspect spell that further increases their chance to dodge attacks.*/

        /// <summary>
        /// Returns Agility * 2
        /// </summary>
        public int Armor => Value * 2;

        /// <summary>
        /// Returns an entities bonus crictical strike chance based on their given class and current Agility.
        /// </summary>
        /// <param name="aClass"></param>
        /// <returns></returns>
        public double GetCriticalChanceBonus(ClassData aClass) => aClass.AttackCritChanceScaler * Value;

        /// <summary>
        /// Returns an entities bonus dodge chance based on their given class and current Agility.
        /// </summary>
        /// <param name="aClass"></param>
        /// <returns></returns>
        public double GetDodgeChanceBonus(ClassData aClass) => aClass.AgilityDodgeChanceScaler * Value;

        /// <summary>
        /// Returns a Melee Attack Power value which is Agility * 1 if a class is tagged as having MeleeAttackPowerBonus.Agility, if not it returns 0.
        /// </summary>
        /// <param name="aClass"></param>
        /// <returns></returns>
        public int GetMeleeAttackPower(ClassData aClass)
        {
            if (aClass.MeleeAttackBonus != ClassData.MeleeAttackPowerBonus.Agility) return 0;//TODO: Unsure atm how to best implement the fact that catform gains +1 per agi
            return Value;
        }

        public Agility(int aValue) : base(aValue) { }


        public static bool operator ==(Agility lhs, Agility rhs) => lhs.Equals(rhs);
        public static bool operator !=(Agility lhs, Agility rhs) => !lhs.Equals(rhs);

        public static Agility operator +(Agility a, Agility b) => a + b.Value;
        public static Agility operator +(Agility a, int b) => new Agility(a.Value + b);

        public static Agility operator -(Agility a, Agility b) => a - b.Value;
        public static Agility operator -(Agility a, int b) => new Agility(a.Value - b);

        public static Agility operator *(Agility a, Agility b) => a * b.Value;
        public static Agility operator *(Agility a, int b) => new Agility(a.Value * b);

        public static Agility operator /(Agility a, Agility b) => a / b.Value;
        public static Agility operator /(Agility a, int b) => new Agility(a.Value / b);

        public override int GetHashCode()
        {
            return Value.GetHashCode();
        }

        public override bool Equals(object obj)
        {
            if (obj == null) return this == null;
            if (obj.GetType() != typeof(Agility)) return false;
            Agility other = (Agility)obj;

            return Value == other.Value;
        }
    }
}
