using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Project_1.Managers;
using Project_1.Textures;
using Project_1.Input;
using Project_1.Tiles;
using Project_1.Textures.AnimatedTextures;
using Project_1.Items;
using Project_1.GameObjects.Spells;
using Project_1.Camera;
using System.Diagnostics;
using Project_1.UI.HUD.Managers;
using Project_1.GameObjects.Unit;
using Project_1.GameObjects.Unit.Stats;
using Project_1.Messaging;
using Project_1.Messaging.Events;
using Project_1.GameObjects.Entities.Friendlies.GuildMembers;
using Project_1.GameObjects.Entities.Friendlies.Players;

namespace Project_1.GameObjects.Entities.Friendlies.Players
{
    internal class Player : Friendly, ILightEmitter
    {
        public float LightRadiusTiles => 6f;
        public override Color MinimapColor => Color.White;
        public PlayerData PlayerData => UnitData as PlayerData;
        public Inventory Inventory => PlayerData.Inventory;

        public SpellBook SpellBook => PlayerData.SpellBook;

        public Party Party => party;
        Party party;

        public Guild Guild => guild;
        Guild guild;
        public bool LockedMovement => lockedMovement;
        bool lockedMovement = false;

        public int Gold => PlayerData.Gold;


        public bool InCombatOrPartyInCombat => party.IsInCombat || InCombat;

        public Player(string aName, string aClassName) /*Change class to be a class*/ : this(new PlayerData(aName, aClassName))
        {

        }

        public Player(PlayerData aPlayerData) : base(aPlayerData)
        {
            ThreadAffinity.AssertSimThread();
            Mailboxes.PublishUiEvent(new InventoryAssigned(Inventory.BuildUiSnapshot()));
            party = new Party(this);
            guild = new Guild(this);
            SpellBook.Init(this);

            LoadSpellBar(PlayerData.SavedSpellsOnBar);

            Mailboxes.PublishUiEvent(new SpellbookRefreshed(RenderId, SpellBook.Spells.Select(x => x.Name).ToArray()));
            Mailboxes.PublishUiEvent(new CharacterWindowSet(BuildCharacterWindowSnapshot()));
            Mailboxes.PublishUiEvent(new PlayerPlateSet(BuildUiSnapshot()));
            Mailboxes.PublishUiEvent(new GoldChanged(Gold));
        }

        public override void Update()
        {
            ThreadAffinity.AssertSimThread();
            Party.Update();
            base.Update();
        }

        void LoadSpellBar(string[] aSpellOnBar)
        {
            if (aSpellOnBar == null) return;
            Project_1.GameObjects.Spells.Spell[] spells = SpellBook.Spells;
            int?[] indexOfSpellsToAdd = new int?[aSpellOnBar.Length];
            for (int i = 0; i < aSpellOnBar.Length; i++)
            {
                if (aSpellOnBar[i] == null) continue;

                int indexOfSpell = Array.FindIndex(spells, x => x.Name == aSpellOnBar[i]);
                Debug.Assert(indexOfSpell >= 0);

                indexOfSpellsToAdd[i] = indexOfSpell;
            }
            string[] spellNamesToAddToBar = new string[indexOfSpellsToAdd.Length];
            for (int i = 0; i < indexOfSpellsToAdd.Length; i++)
            {
                if (!indexOfSpellsToAdd[i].HasValue) continue;
                spellNamesToAddToBar[i] = spells[indexOfSpellsToAdd[i].Value].Name;
            }
            Mailboxes.PublishUiEvent(new SpellbarLoaded(RenderId, spellNamesToAddToBar));
        }

        public void GetPartyMembersFromGuild()
        {
            ThreadAffinity.AssertSimThread();
            string[] partyMembers = PlayerData.Party;
            for (int i = 0; i < partyMembers.Length; i++)
            {
                GuildMember guildMember = guild.GetGuildMemberByName(partyMembers[i]);
                ObjectManager.SpawnGuildMemberToParty(guildMember, guildMember.FeetPosition);
            }
            Mailboxes.PublishUiEvent(new GuildInviteStatusUpdated(partyMembers, Enumerable.Repeat(InviteStatus.Accepted, partyMembers.Length).ToArray()));
        }

        public void ApplyMoveInput(bool left, bool right, bool up, bool down)
        {
            ThreadAffinity.AssertSimThread();
            if (HasDestination && LockedMovement) return;
            if (left) velocity.X -= 1;
            if (right) velocity.X += 1;
            if (up) velocity.Y -= 1;
            if (down) velocity.Y += 1;
            if (velocity == WorldSpace.Zero) return;
            velocity.Normalize();
            velocity *= (float)(UnitData.MovementData.Speed * TimeManager.SecondsSinceLastFrame);
        }

        public void ChangeGold(int aAmount)
        {
            ThreadAffinity.AssertSimThread();
            PlayerData.Gold += aAmount;
            Mailboxes.PublishUiEvent(new GoldChanged(Gold));
        }

        CharacterWindowSnapshot BuildCharacterWindowSnapshot()
        {
            ThreadAffinity.AssertSimThread();
            ItemUiSnapshot[] equippedItems = new ItemUiSnapshot[(int)Unit.Equipment.Slot.Count];
            for (int i = 0; i < equippedItems.Length; i++)
            {
                Items.Item equipped = Equipment.EquipedInSlot((Unit.Equipment.Slot)i);
                if (equipped == null) continue;
                equippedItems[i] = ItemUiSnapshot.FromItem(equipped);
            }

            return new CharacterWindowSnapshot(
                BuildUiSnapshot(),
                StatReportSnapshot.FromPairReport(PrimaryStatReport, default, StatLineCategoryResolver.ResolveCharacter),
                BuildSecondaryReport(),
                CurrentLevel,
                Level.Experience,
                equippedItems);
        }

        StatReportSnapshot BuildSecondaryReport()
        {
            ThreadAffinity.AssertSimThread();
            PairReport report = new PairReport();
            SpellReportDetailsSnapshot spellDetails = BuildSpellReportDetails();
            report.AddLine("Attack Power", BuildTotalAttackPower());
            report.AddLine("Crit Chance", SecondaryStats.Attack.CriticalChance);
            report.AddLine("Crit Damage", SecondaryStats.Attack.CriticalDamage);
            report.AddLine("Hit Chance", SecondaryStats.Attack.BonusHitChance);
            report.AddLine("Spell Damage", SecondaryStats.Spell.SpellDamageForSchool(SpellSchool.Base));
            report.AddLine("Spell Crit Chance", SecondaryStats.Spell.CriticalChanceForSchool(SpellSchool.Base));
            report.AddLine("Spell Crit Damage", SecondaryStats.Spell.CriticalDamageForSchool(SpellSchool.Base));
            report.AddLine("Spell Hit Chance", SecondaryStats.Spell.BonusHitChanceForSchool(SpellSchool.Base));
            report.AddLine("Dodge Chance", SecondaryStats.Defense.DodgeChance);
            report.AddLine("Parry Chance", SecondaryStats.Defense.ParryChance);
            return StatReportSnapshot.FromPairReport(report, spellDetails, StatLineCategoryResolver.ResolveCharacter);
        }

        int BuildTotalAttackPower()
        {
            ThreadAffinity.AssertSimThread();
            int strength = 0;
            int agility = 0;
            var lines = PrimaryStatReport?.Lines;
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

            int fromStrength = ClassData.MeleeAttackBonus == Project_1.GameObjects.Unit.Classes.ClassData.MeleeAttackPowerBonus.Strength ? strength * 2 : strength;
            int fromAgility = ClassData.MeleeAttackBonus == Project_1.GameObjects.Unit.Classes.ClassData.MeleeAttackPowerBonus.Agility ? agility : 0;
            int fromEquipment = Equipment.GetSecondaryStat<int>("AttackPower");
            return fromStrength + fromAgility + fromEquipment;
        }

        SpellReportDetailsSnapshot BuildSpellReportDetails()
        {
            ThreadAffinity.AssertSimThread();
            List<SpellSchoolBonusSnapshot> damageBonuses = new List<SpellSchoolBonusSnapshot>();
            List<SpellSchoolBonusSnapshot> critChanceBonuses = new List<SpellSchoolBonusSnapshot>();
            List<SpellSchoolBonusSnapshot> hitChanceBonuses = new List<SpellSchoolBonusSnapshot>();

            int baseSpellDamage = SecondaryStats.Spell.SpellDamageForSchool(SpellSchool.Base);
            double baseSpellCritChance = SecondaryStats.Spell.CriticalChanceForSchool(SpellSchool.Base);
            double baseSpellHitChance = SecondaryStats.Spell.BonusHitChanceForSchool(SpellSchool.Base);

            foreach (SpellSchool school in Enum.GetValues(typeof(SpellSchool)))
            {
                if (school == SpellSchool.Base)
                {
                    continue;
                }

                int schoolDamageBonus = SecondaryStats.Spell.SpellDamageForSchool(school) - baseSpellDamage;
                double schoolCritBonus = SecondaryStats.Spell.CriticalChanceForSchool(school) - baseSpellCritChance;
                double schoolHitBonus = SecondaryStats.Spell.BonusHitChanceForSchool(school) - baseSpellHitChance;

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

        protected override bool CheckForRelation()
        {
            if (target.RelationToPlayer == Relation.RelationToPlayer.Self || target.RelationToPlayer == Relation.RelationToPlayer.Friendly)
            {
                return false;
            }
            if (target.RelationToPlayer != RelationToPlayer)
            {
                return true;
            }

            return false;
        }

        public override void ExpToParty(int aExpAmount)
        {
            ThreadAffinity.AssertSimThread();
            party.ExpToParty(aExpAmount);
        }
    }
}
