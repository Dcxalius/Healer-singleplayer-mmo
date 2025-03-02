﻿using Newtonsoft.Json;
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

        [JsonIgnore]
        public Attack Attack => (itemData as WeaponData).Attack;


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
