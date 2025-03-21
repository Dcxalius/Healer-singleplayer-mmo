using Newtonsoft.Json;
using Project_1.GameObjects.Unit;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Project_1.Items.SubTypes
{
    internal class Weapon : Equipment
    {
        public enum HandRequirement
        {
            OneHand,
            TwoHand,
            MainHand,
            OffHand,
            Ranged
        }

        [Flags]
        public enum WeaponType
        {
            None,
            Dagger = 1,
            Sword = 2,
            TwoHandedSword = 4,
            Axe = 8,
            TwoHandedAxe = 16,
            Mace = 32,
            TwoHandedMace = 64,
            Fist = 128,
            Staff = 256,
            Bow = 512,
            Gun = 1024,
            Thrown = 2048,
            Wand  = 4096,
            Shield = 8192,
            Holdable = 16384 //TODO: I dunnu what to call this xdd
        }

        [JsonIgnore]
        public Attack Attack => (itemData as WeaponData).Attack;

        public WeaponData WeaponData => itemData as WeaponData;

        [JsonIgnore]
        public HandRequirement handRequirement;

        public Weapon(LootData aLoot) : base(aLoot)
        {
        }

        public Weapon(WeaponData aData) : base(aData)
        {
        }
    }
}
