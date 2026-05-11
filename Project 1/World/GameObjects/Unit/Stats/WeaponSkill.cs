using Microsoft.Xna.Framework.Media;
using Project_1.GameObjects.Entities;
using Project_1.Items.SubTypes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Project_1.GameObjects.Entities.Friendlies.Players;
using Project_1.GameObjects.Unit.Classes;
using Project_1.Managers;

namespace Project_1.GameObjects.Unit.Stats
{
    internal class WeaponSkill
    {
        static void AssertSimThread() => ThreadAffinity.AssertSimThread();
        Entity entity;

        public int[] Skills
        {
            get
            {
                int maxIndex = Enum.GetValues<Weapon.WeaponType>().Max(x => (int)x);
                int[] returnable = new int[maxIndex + 1];
                foreach (var skill in skills)
                {
                    returnable[(int)skill.Key] = skill.Value;
                }
                return returnable;
            }
        }
        Dictionary<Weapon.WeaponType, int> skills;
        Dictionary<Weapon.WeaponType, int> bonuses;
        
        [JsonConstructor]
        public WeaponSkill(int[] skills)
        {
            this.skills = new Dictionary<Weapon.WeaponType, int>();
            bonuses = new Dictionary<Weapon.WeaponType, int>();
            if (skills == null) return;
            for (int i = 0; i < skills.Count(); i++)
            {
                if (skills[i] == 0) continue;
                this.skills.Add((Weapon.WeaponType)i, skills[i]);
            }
        }

        public WeaponSkill(Entity aEntity, int[] aWeaponSkill) : this(aWeaponSkill)
        {
            entity = aEntity;
        }

        public WeaponSkill(Entity aEntity, ClassData aClass)
        {
            skills = new Dictionary<Weapon.WeaponType, int>();
            bonuses = new Dictionary<Weapon.WeaponType, int>();

            EnsureClassSkills(aClass);
            entity = aEntity;
        }

        public void SetOwner(Entity aEntity)
        {
            AssertSimThread();
            entity = aEntity;
        }

        public void EnsureClassSkills(ClassData aClass)
        {
            AssertSimThread();
            foreach (Weapon.WeaponType weaponType in Enum.GetValues<Weapon.WeaponType>())
            {
                if (weaponType == Weapon.WeaponType.None) continue;
                if (!aClass.WeaponUsuable(weaponType)) continue;
                if (skills.ContainsKey(weaponType)) continue;
                skills.Add(weaponType, 1);
            }
        }

        public void LevelUpSkill(Weapon.WeaponType aType)
        {
            AssertSimThread();
            if (!skills.ContainsKey(aType)) throw new Exception("Player does not have skill for weapon type " + aType);
            if (skills[aType] >= entity.CurrentLevel * 5) return;
            skills[aType]++;
        }

        public int GetSkill(Weapon.WeaponType aType)
        {
            AssertSimThread();
            if (!skills.TryGetValue(aType, out int returnable)) returnable = 0;
            bonuses.TryGetValue(aType, out int bonus);
            return returnable + bonus;
        }

        public void UpdateBonus(Equipment aEquipment /* Racials */)
        {
            AssertSimThread();
            //TOOD
            //Search aEquipment for bonus weaponskill
            //Search racials for same
        }
    }
}
