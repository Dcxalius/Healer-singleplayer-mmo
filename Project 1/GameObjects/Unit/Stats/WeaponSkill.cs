using Microsoft.Xna.Framework.Media;
using Project_1.GameObjects.Entities;
using Project_1.Items.SubTypes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Project_1.GameObjects.Unit.Stats
{
    internal class WeaponSkill
    {
        Entity entity;

        public int[] Skills
        {
            get
            {
                int[] returnable = new int[Enum.GetValues<Weapon.WeaponType>().Count()];
                foreach (var skill in skills)
                {
                    returnable[(int)skill.Key] = skill.Value;
                }
                return returnable;
            }
        }
        Dictionary<Weapon.WeaponType, int> skills;
        Dictionary<Weapon.WeaponType, int> bonuses;
        

        public WeaponSkill(Entity aEntity, int[] aWeaponSkill)
        {
            skills = new Dictionary<Weapon.WeaponType, int>();
            bonuses = new Dictionary<Weapon.WeaponType, int>();
            for (int i = 0; i < aWeaponSkill.Count(); i++)
            {
                if (aWeaponSkill[i] == 0) continue;
                skills.Add((Weapon.WeaponType)i, aWeaponSkill[i]);
            }
            entity = aEntity;
        }

        public WeaponSkill(Entity aEntity, Classes.ClassData aClass)
        {
            skills = new Dictionary<Weapon.WeaponType, int>();
            for (int i = 0; i < aClass.w.Length; i++)
            {
                if (aClass.w[i]) continue;
                skills.Add((Weapon.WeaponType)i, 1);
            }
            entity = aEntity;
        }

        public void LevelUpSkill(Weapon.WeaponType aType)
        {
            if (!skills.ContainsKey(aType)) throw new Exception("Player does not have skill for weapon type " + aType);
            if (skills[aType] < entity.CurrentLevel * 5) return;
            skills[aType]++;
        }

        public int GetSkill(Weapon.WeaponType aType)
        {
            if (!skills.TryGetValue(aType, out int returnable)) returnable = 0;
            bonuses.TryGetValue(aType, out int bonus);
            return returnable + bonus;
        }

        public void UpdateBonus(Equipment aEquipment /* Racials */)
        {
            //TOOD
            //Search aEquipment for bonus weaponskill
            //Search racials for same
        }
    }
}
