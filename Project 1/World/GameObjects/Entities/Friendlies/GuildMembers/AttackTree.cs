using Project_1.GameObjects.Spells;
using Project_1.GameObjects.Unit;
using Project_1.Managers;
using System;
using System.Collections.Generic;
using System.Linq;
using Project_1.GameObjects.Entities.Friendlies.GuildMembers;
using Project_1.GameObjects.Entities.Friendlies.Players;

namespace Project_1.GameObjects.Entities.Friendlies.GuildMembers
{
    internal class AttackTree : LogicTree
    {
        readonly GuildMember owner;
        readonly List<Spell> prioritySpells;

        const double lowHealthThreshold = 0.35;

        internal IReadOnlyList<Spell> PrioritySpells => prioritySpells;

        static LogicNode BuildRoot(GuildMember owner, List<Spell> spells)
        {
            var findTarget = new LogicNode(new FindTargetResult());
            var panic = new LogicNode(new HealOrFleeResult());
            var castSpell = new LogicNode(new CastSpellResult(spells));
            var moveToTarget = new LogicNode(new MoveToTargetResult());
            var attack = new LogicNode(new AutoAttackResult());
            var idle = new LogicNode(new IdleResult());

            Func<bool>[] conditions =
            {
                () => NeedsNewTarget(owner),
                () => IsLowHealth(owner),
                () => ShouldCast(owner, spells),
                () => ShouldChase(owner),
                () => owner.Target != null,
                () => true
            };

            return new LogicNode(new[] { findTarget, panic, castSpell, moveToTarget, attack, idle }, conditions);
        }

        public AttackTree(GuildMember aOwner) : this(aOwner, BuildSpells(aOwner))
        {
        }

        AttackTree(GuildMember aOwner, List<Spell> spells) : base(BuildRoot(aOwner, spells))
        {
            owner = aOwner;
            prioritySpells = spells;
        }

        static List<Spell> BuildSpells(GuildMember owner)
        {
            var friendlyData = owner.ClassData;
            if (friendlyData == null || friendlyData.LevelOneSpells == null) return new List<Spell>();
            return friendlyData.LevelOneSpells.Select(name => new Spell(owner, name, 1)).ToList();
        }

        static bool NeedsNewTarget(GuildMember owner)
        {
            if (owner.Target == null) return true;
            if (!owner.Target.Alive) return true;
            if (owner.Target.RelationToPlayer == Relation.RelationToPlayer.Self ||
                owner.Target.RelationToPlayer == Relation.RelationToPlayer.Friendly)
            {
                return true;
            }
            return false;
        }

        static bool IsLowHealth(GuildMember owner)
        {
            if (owner.MaxHealth <= 0) return false;
            return owner.CurrentHealth / owner.MaxHealth <= lowHealthThreshold;
        }

        static bool ShouldCast(GuildMember owner, List<Spell> spells)
        {
            if (owner.Target == null) return false;
            if (!owner.OffGlobalCooldown) return false;
            if (spells == null || spells.Count == 0) return false;
            return spells.Any(spell => spell.OffCooldown && owner.Resource.isCastable(spell.ResourceCost));
        }

        static bool ShouldChase(GuildMember owner)
        {
            if (owner.Target == null) return false;
            return !IsInAttackRange(owner);
        }

        static bool IsInAttackRange(GuildMember owner)
        {
            if (owner.Target == null) return false;
            float distance = owner.Target.DistanceTo(owner.FeetPosition);
            float attackRange = owner.MinimumAttackRange - owner.Size.X / 2f - owner.Target.Size.X / 2f;
            return distance <= attackRange;
        }

        sealed class MoveToTargetResult : ILogicResult
        {
            public void ExecuteResult(GuildMember guildMember)
            {
                if (guildMember.Target == null) return;
                guildMember.Destination.OverwriteDestination(guildMember.Target.FeetPosition);
            }
        }

        sealed class AutoAttackResult : ILogicResult
        {
            public void ExecuteResult(GuildMember guildMember)
            {
                // Auto-attack handled by Entity.Update once target is in range.
            }
        }

        sealed class HealOrFleeResult : ILogicResult
        {
            public void ExecuteResult(GuildMember guildMember)
            {
                guildMember.RemoveTarget();
                var player = ObjectManager.Player;
                var fallback = player != null ? player.FeetPosition : guildMember.FeetPosition;
                guildMember.RecieveDirectWalkingOrder(fallback);
            }
        }

        sealed class CastSpellResult : ILogicResult
        {
            readonly List<Spell> spells;

            public CastSpellResult(List<Spell> spells)
            {
                this.spells = spells;
            }

            public void ExecuteResult(GuildMember guildMember)
            {
                if (guildMember.Target == null) return;
                if (spells == null) return;

                foreach (var spell in spells)
                {
                    if (spell == null) continue;
                    if (!spell.OffCooldown) continue;
                    if (!guildMember.Resource.isCastable(spell.ResourceCost)) continue;
                    float distance = guildMember.Target.DistanceTo(guildMember.FeetPosition);
                    if (distance > spell.CastDistance) continue;

                    guildMember.StartCast(spell);
                    return;
                }
            }
        }

        sealed class FindTargetResult : ILogicResult
        {
            public void ExecuteResult(GuildMember guildMember)
            {
                Entity playerTarget = ObjectManager.Player?.Target;
                Entity newTarget = SelectEnemy(playerTarget, guildMember);
                if (newTarget == null)
                {
                    Entity[] snapshot = ObjectManager.GetEntitiesSnapshot();
                    newTarget = snapshot
                        .Where(e => IsEnemy(e, guildMember))
                        .OrderBy(e => e.DistanceTo(guildMember.FeetPosition))
                        .FirstOrDefault();
                }

                if (newTarget != null)
                {
                    guildMember.SetTarget(newTarget);
                }
                else
                {
                    guildMember.RemoveTarget();
                }
            }

            static Entity SelectEnemy(Entity candidate, GuildMember owner)
            {
                if (candidate == null) return null;
                return IsEnemy(candidate, owner) ? candidate : null;
            }

            static bool IsEnemy(Entity entity, GuildMember owner)
            {
                if (entity == null) return false;
                if (!entity.Alive) return false;
                if (entity == owner) return false;
                return entity.RelationToPlayer == Relation.RelationToPlayer.Hostile;
            }
        }

        sealed class IdleResult : ILogicResult
        {
            public void ExecuteResult(GuildMember guildMember)
            {
                guildMember.RemoveTarget();
            }
        }
    }
}
