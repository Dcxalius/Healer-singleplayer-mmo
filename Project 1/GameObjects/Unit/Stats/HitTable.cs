using Project_1.GameObjects.Entities;
using Project_1.Managers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Security.Policy;
using System.Text;
using System.Threading.Tasks;
using Project_1.GameObjects.Entities.Friendlies.Players;

namespace Project_1.GameObjects.Unit.Stats
{
    internal class HitTable
    {
        public enum HitResult
        {
            Miss,
            Dodge,
            Parry,
            Glancing, //Player vs Mob only
            Block,
            Crit,
            Crushing, //Mob vs Player only
            Hit
        }

        public static HitResult GenerateTable(Unit.Attack aAttack, Entity aAttacker, Entity aTarget)
            => GenerateTable(aAttack, aAttacker, aTarget, true, true);

        public static HitResult GenerateTable(Unit.Attack aAttack, Entity aAttacker, Entity aTarget, bool aApplyDualWieldPenalty, bool aAllowGlancing)
        {
            //TODO: https://github.com/magey/classic-warrior/issues/5

            double[] table = new double[Enum.GetNames<HitResult>().Length];
            double dualWieldPenalty = aApplyDualWieldPenalty && aAttacker.IsDualWielding ? 0.19 : 0.0;
            int attackerSkillGap = aAttacker.WeaponSkill.GetSkill(aAttack.WeaponType) - aTarget.DefenseSkill;
            int defenderSkillGap = aTarget.DefenseSkill - aAttacker.WeaponSkill.GetSkill(aAttack.WeaponType);
            int attackerSkillGapWithDefenseCapped = aAttacker.WeaponSkill.GetSkill(aAttack.WeaponType) - Math.Min(aTarget.DefenseSkill, aAttacker.CurrentLevel * 5);
            int defenderSkillGapWithAttackCapped = aTarget.DefenseSkill - Math.Min(aAttacker.WeaponSkill.GetSkill(aAttack.WeaponType), aAttacker.CurrentLevel * 5);
            double diff10Penalty = aAttacker.WeaponSkill.GetSkill(aAttack.WeaponType) - aTarget.DefenseSkill >= 10 ? 0.003 : 0.0;

            //0.02 in base when +10 is probably wrong, has sources saying it's 0.01, 0.02 and 0.04
            table[(int)HitResult.Miss] = 0.05 + (attackerSkillGap >= 10 ? 0.02 : 0) + dualWieldPenalty - (attackerSkillGap - (attackerSkillGap >= 10 ? 10 : 0)) * (0.001 + diff10Penalty) - aAttacker.SecondaryStats.Attack.BonusHitChance;
            if (aTarget.HasControl)
            {
                table[(int)HitResult.Dodge] = aTarget.SecondaryStats.Defense.DodgeChance + defenderSkillGap * 0.04;

                if (aTarget.ClassData.CanParry)
                    table[(int)HitResult.Parry] = aTarget.SecondaryStats.Defense.ParryChance + defenderSkillGap * 0.04;
                else
                    table[(int)HitResult.Parry] = 0.0;

                if (aTarget.Equipment.HasShield) //TODO: Check if shield can block damagetype being dealt
                    table[(int)HitResult.Block] = aTarget.SecondaryStats.Defense.BlockChance + defenderSkillGap * 0.04;
                else
                    table[(int)HitResult.Block] = 0.0;
            }
            else
            {
                table[(int)HitResult.Dodge] = 0.0;
                table[(int)HitResult.Parry] = 0.0;
                table[(int)HitResult.Block] = 0.0;
            }


            if (aAllowGlancing && aAttacker.UnitType == UnitType.Player && aTarget.UnitType >= UnitType.Normal)
                table[(int)HitResult.Glancing] = 0.1 + (defenderSkillGapWithAttackCapped > 0 ? defenderSkillGapWithAttackCapped : 0 * 0.02);
            else
                table[(int)HitResult.Glancing] = 0.0;

            table[(int)HitResult.Crit] = aAttacker.SecondaryStats.Attack.CriticalChance + attackerSkillGap * 0.04;
            table[(int)HitResult.Crushing] = (aAttacker.UnitType >= UnitType.Normal && aTarget.UnitType == UnitType.Player && attackerSkillGapWithDefenseCapped >= 20) ? attackerSkillGapWithDefenseCapped * 0.02 - 0.15 : 0.0;

            for (int i = 0; i < table.Length; i++)
            {
                table[i] = Math.Clamp(table[i], 0.0, 1.0);
            }

            //DebugManager.Print(FormatHitTable(table));

            double roll = RandomManager.RollDouble(0.01, 1);
            double cumulative = 0.0;
            for (int i = 0; i < table.Length - 1; i++)
            {
                cumulative += table[i];
                if (roll <= cumulative)
                {
                    DebugManager.Print("Roll: " + roll + ", Expected: " + (HitResult)i);

                    return (HitResult)i;
                }
            }
            DebugManager.Print("Roll: " + roll + ", Expected: " + HitResult.Hit);

            return HitResult.Hit;
        }

        static string FormatHitTable(double[] table)
        {
            StringBuilder builder = new StringBuilder();
            builder.AppendLine("Hit table:");
            string[] names = Enum.GetNames<HitResult>();
            double cumulative = 0.0;

            for (int i = 0; i < names.Length - 1; i++)
            {
                double chance = table[i];
                double start = cumulative * 100.0;
                cumulative += chance;
                double end = cumulative * 100.0;
                builder.AppendLine($"{names[i],-9} {chance * 100.0,6:0.00}%  [{start,6:0.00}-{end,6:0.00}]");
            }

            double hitChance = Math.Max(0.0, 1.0 - cumulative);
            double hitStart = cumulative * 100.0;
            double hitEnd = (cumulative + hitChance) * 100.0;
            builder.AppendLine($"{HitResult.Hit,-9} {hitChance * 100.0,6:0.00}%  [{hitStart,6:0.00}-{hitEnd,6:0.00}]");

            return builder.ToString().TrimEnd();
        }

        //Miss	27.00%	0.01 - 27.00
        //Dodge   6.50 % 27.01 - 33.50
        //Parry   14.00 % 33.51 - 47.50
        //Glancing Blow   24.00 % 47.51 - 71.50
        //Block   6.50 % 71.51 - 78.00
        //Critical hit    22.00 % 78.01 - 100.00
        //Crushing Blow   0 %  —
        //Ordinary hit    0 %  —
    }
}
