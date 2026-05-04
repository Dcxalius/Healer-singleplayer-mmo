using Project_1.GameObjects.Entities;
using Project_1.GameObjects.Entities.Friendlies;
using Project_1.GameObjects.Unit.Classes;
using Project_1.GameObjects.Unit.Resources;
using Project_1.Messaging;
using Project_1.Messaging.Events;
using Project_1.UI.HUD.Managers;
using Project_1.Managers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static Project_1.World.GameObjects.Unit.Stats.Primary.BasePrimaryStats;
using Project_1.World.GameObjects.Unit.Stats.Primary;
using Project_1.World.GameObjects.Unit.Stats.Secondary;

namespace Project_1.GameObjects.Unit.Stats
{
    internal class BaseStats
    {
        static void AssertSimThread() => ThreadAffinity.AssertSimThread();
        Entity owner;
        public Health Health => health;
        Health health;

        public Resource Resource => resource;
        Resource resource;

        public AttackData FistAttack => fistAttack;
        AttackData fistAttack;

        public Armor TotalArmor => baseArmor + totalPrimaryStats.Agility.Armor;
        Armor baseArmor;

        ClassData classData;

        public TotalPrimaryStats TotalPrimaryStats => totalPrimaryStats;
        TotalPrimaryStats totalPrimaryStats;
        BasePrimaryStats basePrimaryStats;

        public PairReport StatReport
        {
            get
            {
                PairReport report = TotalPrimaryStats.NewReport;
                report.AddLine("Armor", TotalArmor);
                return report;
            }
        }

        public int GetAttackPower(ClassData aClassData)
        {
            AssertSimThread();
            int baseAttackPower = totalPrimaryStats.Agility.GetMeleeAttackPower(aClassData) + totalPrimaryStats.Strength.GetMeleeAttackPower(aClassData);
            int equipmentAttackPower = owner?.Equipment.GetSecondaryStat<int>("AttackPower") ?? 0;
            return baseAttackPower + equipmentAttackPower;
        }


        public BaseStats(ClassData aClassData, int aLevel, EquipmentStats aEquipmentStats, float aCurrentHealth = float.MaxValue, float aCurrentResource = float.MaxValue)
        {
            basePrimaryStats = new BasePrimaryStats(aClassData.BaseStats, aClassData.PerLevelStats, aLevel);
            baseArmor = new Armor(aEquipmentStats.Armor);
            health = new Health(aClassData, basePrimaryStats, aLevel, aCurrentHealth);
            classData = aClassData;
            totalPrimaryStats = new TotalPrimaryStats(basePrimaryStats, aEquipmentStats);
            switch (aClassData.Resource)
            {
                case Resource.ResourceType.Mana:
                    //int aBaseManaFromClass;
                    float manaPer5 = 5; //TODO: Extranct these values from class
                    int maxResource = /*baseFromClass + */ 1;

                    resource = new Mana(maxResource, TotalPrimaryStats, aCurrentResource, manaPer5, aLevel);
                    break;
                case Resource.ResourceType.Energy:
                    resource = new Energy(aCurrentResource);
                    break;
                case Resource.ResourceType.Rage:
                    resource = new Rage(aCurrentResource);
                    break;
                case Resource.ResourceType.None:
                    resource = new None();
                    break;
                default:
                    throw new NotImplementedException();
            }

            fistAttack = new AttackData(AttackData.AttackStyle.OneHander, new Unit.Attack(aClassData.FistMinAttackDamage, aClassData.FistMaxAttackDamage, aClassData.FistAttackSpeed, Items.SubTypes.Weapon.WeaponType.None), null);
            fistAttack.AttackPower = GetAttackPower(aClassData);

        }

        public void SetOwner(Entity aEntity)
        {
            AssertSimThread();
            owner = aEntity;
            resource.SetOwner(aEntity);
            owner.Equipment.SetMeleeAttackPower = GetAttackPower(classData);
            fistAttack.AttackPower = GetAttackPower(classData);
            if (owner is not Friendly friendlyOwner) return;
            MailboxManager.PublishUiEvent(new StatsRefreshed(
                owner.RenderId,
                owner.RelationToPlayer.ToRelationToPlayerKind(),
                StatReportSnapshot.FromPairReport(StatReport, default, StatLineCategoryResolver.ResolveCharacter),
                BuildSecondaryReport(friendlyOwner)));
        }

        public bool CheckIfResourceRegened()
        {
            AssertSimThread();
            if (resource.GetType() != typeof(Mana)) return false;
            return (resource as Mana).CheckIfTicked();
        }


        public void LevelUp()
        {
            AssertSimThread();
            basePrimaryStats.LevelUp(classData.PerLevelStats);
            totalPrimaryStats.UpdateBaseStats(basePrimaryStats);
            health.LevelUp(classData.PerLevelHp, basePrimaryStats.Stamina);
            resource.LevelUp();

            RefreshStats();
        }

        public void RefreshStats()
        {
            AssertSimThread();
            health.Refresh(TotalPrimaryStats);
            resource.Refresh(TotalPrimaryStats);
            owner.RefreshSecondaryStats();
            owner.Equipment.SetMeleeAttackPower = GetAttackPower(classData);
            fistAttack.AttackPower = GetAttackPower(classData);


            if (owner is not Friendly friendlyOwner) return;
            MailboxManager.PublishUiEvent(new StatsRefreshed(
                owner.RenderId,
                owner.RelationToPlayer.ToRelationToPlayerKind(),
                StatReportSnapshot.FromPairReport(StatReport, default, StatLineCategoryResolver.ResolveCharacter),
                BuildSecondaryReport(friendlyOwner)));
        }

        public void RefreshEquipmentStats(EquipmentStats aEquipmentStats)
        {
            AssertSimThread();
            totalPrimaryStats.UpdateEquipmentStats(aEquipmentStats);
            baseArmor = aEquipmentStats.Armor;
            RefreshStats();
        }

        static StatReportSnapshot BuildSecondaryReport(Friendly owner)
        {
            PairReport report = new PairReport();
            if (owner == null) return StatReportSnapshot.Empty;
            SpellReportDetailsSnapshot spellDetails = BuildSpellReportDetails(owner.SecondaryStats.Spell);

            report.AddLine("Attack Power", BuildTotalAttackPower(owner));
            report.AddLine("Crit Chance", owner.SecondaryStats.Attack.CriticalChance);
            report.AddLine("Crit Damage", owner.SecondaryStats.Attack.CriticalDamage);
            report.AddLine("Hit Chance", owner.SecondaryStats.Attack.BonusHitChance);
            report.AddLine("Spell Damage", owner.SecondaryStats.Spell.SpellDamageForSchool(SpellSchool.Base));
            report.AddLine("Spell Crit Chance", owner.SecondaryStats.Spell.CriticalChanceForSchool(SpellSchool.Base));
            report.AddLine("Spell Crit Damage", owner.SecondaryStats.Spell.CriticalDamageForSchool(SpellSchool.Base));
            report.AddLine("Spell Hit Chance", owner.SecondaryStats.Spell.BonusHitChanceForSchool(SpellSchool.Base));
            report.AddLine("Dodge Chance", owner.SecondaryStats.Defense.DodgeChance);
            report.AddLine("Parry Chance", owner.SecondaryStats.Defense.ParryChance);
            return StatReportSnapshot.FromPairReport(report, spellDetails, StatLineCategoryResolver.ResolveCharacter);
        }

        static int BuildTotalAttackPower(Friendly aOwner)
        {
            if (aOwner == null)
            {
                return 0;
            }

            int strength = 0;
            int agility = 0;
            var lines = aOwner.PrimaryStatReport?.Lines;
            if (lines != null)
            {
                for (int i = 0; i < lines.Count; i++)
                {
                    if (lines[i].Name == "Strength")
                    {
                        strength = (int)Math.Round(lines[i].Value, MidpointRounding.AwayFromZero);
                    }
                    else if (lines[i].Name == "Agility")
                    {
                        agility = (int)Math.Round(lines[i].Value, MidpointRounding.AwayFromZero);
                    }
                }
            }

            int fromStrength = aOwner.ClassData.MeleeAttackBonus == ClassData.MeleeAttackPowerBonus.Strength ? strength * 2 : strength;
            int fromAgility = aOwner.ClassData.MeleeAttackBonus == ClassData.MeleeAttackPowerBonus.Agility ? agility : 0;
            int fromEquipment = aOwner.Equipment.GetSecondaryStat<int>("AttackPower");
            return fromStrength + fromAgility + fromEquipment;
        }

        static SpellReportDetailsSnapshot BuildSpellReportDetails(Spell aSpell)
        {
            if (aSpell == null)
            {
                return SpellReportDetailsSnapshot.Empty;
            }

            List<SpellSchoolBonusSnapshot> damageBonuses = new List<SpellSchoolBonusSnapshot>();
            List<SpellSchoolBonusSnapshot> critChanceBonuses = new List<SpellSchoolBonusSnapshot>();
            List<SpellSchoolBonusSnapshot> hitChanceBonuses = new List<SpellSchoolBonusSnapshot>();

            int baseSpellDamage = aSpell.SpellDamageForSchool(SpellSchool.Base);
            double baseSpellCritChance = aSpell.CriticalChanceForSchool(SpellSchool.Base);
            double baseSpellHitChance = aSpell.BonusHitChanceForSchool(SpellSchool.Base);

            foreach (SpellSchool school in Enum.GetValues(typeof(SpellSchool)))
            {
                if (school == SpellSchool.Base)
                {
                    continue;
                }

                int schoolDamageBonus = aSpell.SpellDamageForSchool(school) - baseSpellDamage;
                double schoolCritBonus = aSpell.CriticalChanceForSchool(school) - baseSpellCritChance;
                double schoolHitBonus = aSpell.BonusHitChanceForSchool(school) - baseSpellHitChance;

                if (schoolDamageBonus != 0)
                {
                    damageBonuses.Add(new SpellSchoolBonusSnapshot(school.ToString(), schoolDamageBonus));
                }

                if (Math.Abs(schoolCritBonus) > 0.000001d)
                {
                    critChanceBonuses.Add(new SpellSchoolBonusSnapshot(school.ToString(), schoolCritBonus));
                }

                if (Math.Abs(schoolHitBonus) > 0.000001d)
                {
                    hitChanceBonuses.Add(new SpellSchoolBonusSnapshot(school.ToString(), schoolHitBonus));
                }
            }

            return new SpellReportDetailsSnapshot(
                damageBonuses.ToArray(),
                critChanceBonuses.ToArray(),
                hitChanceBonuses.ToArray());
        }
    }
}
