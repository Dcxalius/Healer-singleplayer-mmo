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
using Project_1.Messaging;
using Project_1.Messaging.Events;
using Project_1.System.Models.BaseModels;
using Project_1.GameObjects.Entities.Friendlies.GuildMembers;
using Project_1.GameObjects.Entities.Friendlies.Players;
using Project_1.World.GameObjects.Unit.Stats.Secondary;

namespace Project_1.GameObjects.Entities.Friendlies.Players
{
    internal class Player : Friendly, ILightEmitter
    {
        const float PreviewTurnSpeedRadians = MathHelper.PiOver2;
        static readonly string[] presentationDirectionNames = //Q? what is this for?
        {
            "Behind",
            "BackRight",
            "Right",
            "FrontRight",
            "Front",
            "FrontLeft",
            "Left",
            "BackLeft"
        };

        public override Color MinimapColor => Color.White; //TODO: Should be settable

        //All the facing logic feels like it should be in the camera but could be wrong. Just feels like it makes more sense for the camera to manage wheter or not its followed object should rotate
        internal override bool FaceCameraInPreview => false;
        internal override bool FrameFacesCameraInPreview => true;
        internal override bool PresentationFacesCameraInPreview => true;
        //

        public PlayerData PlayerData => UnitData as PlayerData;

        public Inventory Inventory => PlayerData.Inventory;
        public SpellBook SpellBook => PlayerData.SpellBook;

        //TODO: Party should probably be PlayerParty and GuildMemberParty.
        public Party Party => party;
        Party party;

        //TODO: Same thing with guild eventually... probably...
        public Guild Guild => guild;
        Guild guild;

        //TODO: Move this to movement?
        public bool LockedMovement => lockedMovement;
        bool lockedMovement = false;

        //Q: Debug?
        float previewFacingYawRadians;
        bool previewFacingYawInitialized;
        float previewLastFreeMoveYawRadians;
        bool previewLastFreeMoveYawInitialized;

        internal float PreviewBodyFacingYawRadians
        {
            get
            {
                EnsurePreviewFacingYawInitialized();
                return previewFacingYawRadians;
            }
        }
        internal int PreviewPresentationDirectionIndex => ResolvePreviewPresentationDirectionIndexCore();
        internal string PreviewPresentationDirectionName => presentationDirectionNames[PreviewPresentationDirectionIndex];

        public int Gold => PlayerData.Gold;

        //TODO: Should probably be moved up or depri, I think it's only used by the saving system atm
        public bool InCombatOrPartyInCombat => party.IsInCombat || InCombat;

        public Player(string aName, string aClassName)  : this(new PlayerData(aName, aClassName))
        {

        }

        public Player(PlayerData aPlayerData) : base(aPlayerData)
        {
            ThreadAffinity.AssertSimThread();
            MailboxManager.PublishUiEvent(new InventoryAssigned(Inventory.BuildUiSnapshot()));
            party = new Party(this);
            guild = new Guild(this);
            SpellBook.Init(this);

            LoadSpellBar(PlayerData.SavedSpellsOnBar);

            CreationEvents();
        }

        void CreationEvents()
        {
            //TODO: Could this be collapsed to a single event? PlayerCreated or something like that?
            MailboxManager.PublishUiEvent(new SpellbookRefreshed(RenderId, SpellBook.Spells.Select(x => x.SpellKey).ToArray()));
            MailboxManager.PublishUiEvent(new CharacterWindowSet(BuildCharacterWindowSnapshot()));
            MailboxManager.PublishUiEvent(new PlayerPlateSet(BuildUiSnapshot()));
            MailboxManager.PublishUiEvent(new GoldChanged(Gold));
        }

        public override void Update()
        {
            ThreadAffinity.AssertSimThread();
            Party.Update(); //TODO: Determine the party flow, this feels wrong. Perhaps bundling at the manager levels to parties and update those and have the parties be responsible for their units updates? 
            base.Update();
        }

        void LoadSpellBar(string[] aSpellOnBar)
        {
            //TODO: This feels like it should be in spellbar?
            if (aSpellOnBar == null) return;
            Spell[] spells = SpellBook.Spells;
            string[] spellNamesToAddToBar = new string[aSpellOnBar.Length];
            for (int i = 0; i < aSpellOnBar.Length; i++)
            {
                if (string.IsNullOrWhiteSpace(aSpellOnBar[i])) continue;
                if (!SpellBook.TryGetSpell(aSpellOnBar[i], out Project_1.GameObjects.Spells.Spell spell)) continue;
                spellNamesToAddToBar[i] = spell.SpellKey;
            }
            MailboxManager.PublishUiEvent(new SpellbarLoaded(RenderId, spellNamesToAddToBar)); //TODO: What do we use the RenderID for? Feels like since the UIE responisble for the spells only exists for the player
        }

        public void GetPartyMembersFromGuild()
        {
            //TODO: Rename, possible depri? //Q: What does this even do? What is the flow?
            ThreadAffinity.AssertSimThread();
            string[] partyMembers = PlayerData.Party;
            for (int i = 0; i < partyMembers.Length; i++)
            {
                GuildMember guildMember = guild.GetGuildMemberByName(partyMembers[i]);
                ObjectManager.SpawnGuildMemberToParty(guildMember, guildMember.FeetPosition);
            }
            MailboxManager.PublishUiEvent(new GuildInviteStatusUpdated(partyMembers, Enumerable.Repeat(InviteStatus.Accepted, partyMembers.Length).ToArray()));
        }

        public void ApplyMoveInput(bool left, bool right, bool up, bool down) //TODO: Should the inputs be separated into Up/None/Down and Left/None/Right enums? Anything other than 4 bools tbh
        {
            //TODO: Break up
            ThreadAffinity.AssertSimThread();
            if (HasDestination && LockedMovement) return;

            if (DebugManager.Mode(DebugMode.ModelPreview))
            {
                ApplyPreviewRotationInput();
                Vector2 movementDirection = Vector2.Zero;
                if (Camera.Camera.CurrentPreviewCameraMode == PreviewCameraMode.OverTheShoulder)
                {
                    Vector2 forward = BuildPreviewForward();
                    Vector2 rightVector = new Vector2(-forward.Y, forward.X);
                    if (left) movementDirection -= rightVector;
                    if (right) movementDirection += rightVector;
                    if (up) movementDirection += forward;
                    if (down) movementDirection -= forward;
                }
                else
                {
                    Vector2 forward = WorldBlockRenderer.CameraGroundForward;
                    Vector2 rightVector = WorldBlockRenderer.CameraGroundRight;
                    if (left) movementDirection -= rightVector;
                    if (right) movementDirection += rightVector;
                    if (up) movementDirection += forward;
                    if (down) movementDirection -= forward;
                }
                TrackPreviewFreeMoveDirection(movementDirection);
                velocity += new WorldSpace(movementDirection);
            }
            else
            {
                if (left) velocity.X -= 1;
                if (right) velocity.X += 1;
                if (up) velocity.Y -= 1;
                if (down) velocity.Y += 1;
            }

            if (velocity == WorldSpace.Zero) return;
            velocity.Normalize();
            velocity *= Speed * (float)TimeManager.SecondsSinceLastFrame; //TODO: Ponder if it makes sense to make a SpeedProp that mults with deltatime
        }

        protected override bool TryResolvePreviewFacingYawRadians(out float aFacingYawRadians)
        {
            EnsurePreviewFacingYawInitialized();
            aFacingYawRadians = previewFacingYawRadians;
            return true;
        }

        protected override int ResolvePreviewPresentationDirectionIndex()
        {
            return ResolvePreviewPresentationDirectionIndexCore();
        }

        void ApplyPreviewRotationInput()
        {
            float rotationDelta = 0f;
            if (KeyBindStateCache.GetHold(KeyBindManager.KeyListner.RotatePlayerLeft))
            {
                rotationDelta += PreviewTurnSpeedRadians * (float)TimeManager.SecondsSinceLastFrame;
            }
            if (KeyBindStateCache.GetHold(KeyBindManager.KeyListner.RotatePlayerRight))
            {
                rotationDelta -= PreviewTurnSpeedRadians * (float)TimeManager.SecondsSinceLastFrame;
            }
            if (Math.Abs(rotationDelta) <= float.Epsilon)
            {
                return;
            }

            RotatePreviewFacing(rotationDelta);
        }

        internal void RotatePreviewFacing(float aRotationDeltaRadians)
        {
            EnsurePreviewFacingYawInitialized();
            if (Math.Abs(aRotationDeltaRadians) <= float.Epsilon)
            {
                return;
            }

            previewFacingYawRadians += aRotationDeltaRadians;
            while (previewFacingYawRadians <= -MathF.PI) previewFacingYawRadians += MathHelper.TwoPi;
            while (previewFacingYawRadians > MathF.PI) previewFacingYawRadians -= MathHelper.TwoPi;

            if (Camera.Camera.CurrentPreviewCameraMode == PreviewCameraMode.OverTheShoulder)
            {
                WorldBlockRenderer.RotateCameraYaw(aRotationDeltaRadians);
            }
        }

        void EnsurePreviewFacingYawInitialized()
        {
            if (previewFacingYawInitialized) return;

            Vector2 forward = WorldBlockRenderer.CameraGroundForward;
            if (forward.LengthSquared() <= float.Epsilon)
            {
                forward = -Vector2.UnitY;
            }
            else
            {
                forward.Normalize();
            }

            previewFacingYawRadians = MathF.Atan2(forward.X, forward.Y);
            previewFacingYawInitialized = true;
        }

        Vector2 BuildPreviewForward()
        {
            EnsurePreviewFacingYawInitialized();
            return new Vector2(MathF.Sin(previewFacingYawRadians), MathF.Cos(previewFacingYawRadians));
        }

        void TrackPreviewFreeMoveDirection(Vector2 aMovementDirection)
        {
            if (Camera.Camera.CurrentPreviewCameraMode != PreviewCameraMode.Free) return;
            if (aMovementDirection.LengthSquared() <= float.Epsilon) return;

            Vector2 normalized = Vector2.Normalize(aMovementDirection);
            previewLastFreeMoveYawRadians = MathF.Atan2(normalized.X, normalized.Y);
            previewLastFreeMoveYawInitialized = true;
        }

        int ResolvePreviewPresentationDirectionIndexCore()
        {
            if (Camera.Camera.CurrentPreviewCameraMode == PreviewCameraMode.OverTheShoulder)
            {
                return 0;
            }

            Vector2 worldDirection = ResolvePresentationWorldDirection();
            if (worldDirection.LengthSquared() <= float.Epsilon)
            {
                return 0;
            }

            Vector2 right = WorldBlockRenderer.CameraGroundRight;
            Vector2 forward = WorldBlockRenderer.CameraGroundForward;
            float x = Vector2.Dot(worldDirection, right);
            float y = Vector2.Dot(worldDirection, forward);
            float angle = MathF.Atan2(x, y);
            int sector = ((int)MathF.Round(angle / MathHelper.PiOver4) % 8 + 8) % 8;
            return sector;
        }

        Vector2 ResolvePresentationWorldDirection()
        {
            if (previewLastFreeMoveYawInitialized)
            {
                return new Vector2(MathF.Sin(previewLastFreeMoveYawRadians), MathF.Cos(previewLastFreeMoveYawRadians));
            }

            return BuildPreviewForward();
        }

        public void ChangeGold(int aAmount)
        {
            ThreadAffinity.AssertSimThread();
            PlayerData.Gold += aAmount;
            //TODO: LoseGold(-aAmount) or GainGold(aAmount) Should prob be a separate event than the gold 
            MailboxManager.PublishUiEvent(new GoldChanged(Gold));
        }

        CharacterWindowSnapshot BuildCharacterWindowSnapshot() //TODO: This feels like it should be friendly, since guildmembers gear is shown in the same system as the players
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
                equippedItems,
                BuildWeaponSkillSnapshots());
        }

        WeaponSkillUiSnapshot[] BuildWeaponSkillSnapshots()
        {
            //TODO: If this is use for more than just validation or creation something is wrong.
            //TODO: There should be a SkillUpEvent()
            ThreadAffinity.AssertSimThread();
            List<WeaponSkillUiSnapshot> snapshots = new List<WeaponSkillUiSnapshot>();
            int maxSkill = Math.Max(1, CurrentLevel * 5);

            foreach (Items.SubTypes.Weapon.WeaponType weaponType in Enum.GetValues<Items.SubTypes.Weapon.WeaponType>())
            {
                if (!ShouldShowWeaponSkill(weaponType)) continue;

                snapshots.Add(new WeaponSkillUiSnapshot(
                    weaponType,
                    FormatWeaponSkillName(weaponType),
                    UnitData.WeaponSkill?.GetSkill(weaponType) ?? 0,
                    maxSkill));
            }

            return snapshots.ToArray();
        }

        bool ShouldShowWeaponSkill(Items.SubTypes.Weapon.WeaponType weaponType)
        {
            //TODO: Make a new class called Skill, Skill => WeaponSkill, Skill => Profession, Skill => Defense (What other skills are there in wow? Riding but nothing in our design about riding yet)
            if (weaponType == Items.SubTypes.Weapon.WeaponType.None ||
                weaponType == Items.SubTypes.Weapon.WeaponType.Shield ||
                weaponType == Items.SubTypes.Weapon.WeaponType.Holdable)
            {
                return false;
            }

            return ClassData.WeaponUsuable(weaponType);
        }

        static string FormatWeaponSkillName(Items.SubTypes.Weapon.WeaponType weaponType)
        {
            //TODO: Move to WeaponSkill class
            return weaponType switch
            {
                Items.SubTypes.Weapon.WeaponType.TwoHandedSword => "Two-Handed Sword",
                Items.SubTypes.Weapon.WeaponType.TwoHandedAxe => "Two-Handed Axe",
                Items.SubTypes.Weapon.WeaponType.TwoHandedMace => "Two-Handed Mace",
                Items.SubTypes.Weapon.WeaponType.Fist => "Fist Weapon",
                _ => weaponType.ToString()
            };
        }

        StatReportSnapshot BuildSecondaryReport()
        {
            //TODO: The snapshot system should probably be reworked. Instead of sending each line they should already be bundled here. Strings shouldn't be used, rather just an object, and then the StatReport should get both the number and the string name from the object
            //TODO: Worth waiting until the customizable Statreports?
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

        int BuildTotalAttackPower() //TODO: Should be internal to UnitData. Could have been a hack if there was no clean way to get the AP from data though.
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
            //TODO: Same as other report
            ThreadAffinity.AssertSimThread();
            List<SpellSchoolBonusSnapshot> damageBonuses = new List<SpellSchoolBonusSnapshot>();
            List<SpellSchoolBonusSnapshot> critChanceBonuses = new List<SpellSchoolBonusSnapshot>();
            List<SpellSchoolBonusSnapshot> critDamageBonuses = new List<SpellSchoolBonusSnapshot>();
            List<SpellSchoolBonusSnapshot> hitChanceBonuses = new List<SpellSchoolBonusSnapshot>();

            int baseSpellDamage = SecondaryStats.Spell.SpellDamageForSchool(SpellSchool.Base);
            double baseSpellCritChance = SecondaryStats.Spell.CriticalChanceForSchool(SpellSchool.Base);
            double baseSpellCritDamage = SecondaryStats.Spell.CriticalDamageForSchool(SpellSchool.Base);
            double baseSpellHitChance = SecondaryStats.Spell.BonusHitChanceForSchool(SpellSchool.Base);

            foreach (SpellSchool school in Enum.GetValues(typeof(SpellSchool)))
            {
                if (school == SpellSchool.Base)
                {
                    continue;
                }

                int schoolDamageBonus = SecondaryStats.Spell.SpellDamageForSchool(school) - baseSpellDamage;
                double schoolCritBonus = SecondaryStats.Spell.CriticalChanceForSchool(school) - baseSpellCritChance;
                double schoolCritDamageBonus = SecondaryStats.Spell.CriticalDamageForSchool(school) - baseSpellCritDamage;
                double schoolHitBonus = SecondaryStats.Spell.BonusHitChanceForSchool(school) - baseSpellHitChance;

                if (schoolDamageBonus != 0)
                {
                    damageBonuses.Add(new SpellSchoolBonusSnapshot(school.ToString(), schoolDamageBonus));
                }

                if (Math.Abs(schoolCritBonus) > 0.000001d)
                {
                    critChanceBonuses.Add(new SpellSchoolBonusSnapshot(school.ToString(), schoolCritBonus));
                }

                if (Math.Abs(schoolCritDamageBonus) > 0.000001d)
                {
                    critDamageBonuses.Add(new SpellSchoolBonusSnapshot(school.ToString(), schoolCritDamageBonus));
                }

                if (Math.Abs(schoolHitBonus) > 0.000001d)
                {
                    hitChanceBonuses.Add(new SpellSchoolBonusSnapshot(school.ToString(), schoolHitBonus));
                }
            }

            return new SpellReportDetailsSnapshot(
                damageBonuses.ToArray(),
                critChanceBonuses.ToArray(),
                critDamageBonuses.ToArray(),
                hitChanceBonuses.ToArray());
        }

        protected override bool CheckForRelation() //TODO: Should be moved probably
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
            party.ExpToParty(aExpAmount); //Player is always in their own party so no need to check.
        }
    }
}
