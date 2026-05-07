using Project_1.GameObjects.Spells;
using Project_1.GameObjects.Spells.Buff;
using Project_1.GameObjects.Unit.Stats;
using Project_1.Managers;
using Project_1.UI.HUD.Managers;
using Project_1.Messaging;
using Project_1.Messaging.Events;
using Project_1.World.GameObjects.Spells.SpellEffects;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Project_1.GameObjects.Entities
{
    internal class BuffList
    {
        List<Buff> buffs;

        public BuffList() 
        {
            buffs = new List<Buff>();
        }

        public void Update(Entity aOwner)
        {
            ThreadAffinity.AssertSimThread();
            for (int i = buffs.Count - 1; i >= 0; i--)
            {
                buffs[i].Update(aOwner);
                if (buffs[i].IsOver)
                {
                    bool hadStatModifiers = buffs[i].HasStatModifiers;
                    buffs[i].OnRemoved(aOwner);
                    buffs.RemoveAt(i);
                    RefreshOwnerStatsIfNeeded(aOwner, hadStatModifiers);
                }
            }
        }

        public void AddBuff(Buff aBuff, Entity aOwner)
        {
            ThreadAffinity.AssertSimThread();
            bool hadStatModifiers = HasStatModifiers;
            for (int i = 0; i < buffs.Count; i++)
            {
                if (buffs[i] != aBuff) continue;
                Buff currentBuff = buffs[i];
                if (aBuff.MultipleSourceStackable && !buffs[i].SameCaster(aBuff)) break;
                if (currentBuff.Rank < aBuff.Rank)
                {
                    currentBuff.OnRemoved(aOwner);
                    buffs.Remove(currentBuff);
                    break;
                }
                buffs[i].Recast(aBuff);
                MailboxManager.PublishUiEvent(new BuffAdded(aOwner.RenderId, buffs[i].BuffUiSnapshot));
                RefreshOwnerStatsIfNeeded(aOwner, hadStatModifiers || buffs[i].HasStatModifiers);
                return;
            }

            buffs.Add(aBuff);
            aBuff.OnApplied(aOwner);
            MailboxManager.PublishUiEvent(new BuffAdded(aOwner.RenderId, buffs.Last().BuffUiSnapshot));
            RefreshOwnerStatsIfNeeded(aOwner, hadStatModifiers || aBuff.HasStatModifiers);
        }

        // Chains through all AbsorbBuffs, depleting them in order until damage is exhausted.
        // Removes any buff that reaches zero absorb. Returns remaining damage after all absorbs.
        public double ApplyAbsorb(double incomingDamage, DamageType damageType, Entity aOwner, out double totalAbsorbed)
        {
            ThreadAffinity.AssertSimThread();
            totalAbsorbed = 0;
            double remaining = incomingDamage;
            for (int i = buffs.Count - 1; i >= 0 && remaining > 0; i--)
            {
                double absorbed = buffs[i].AbsorbDamage(remaining, damageType);
                if (absorbed <= 0) continue;
                totalAbsorbed += absorbed;
                remaining -= absorbed;
                if (buffs[i].IsDepleted)
                {
                    bool hadStatModifiers = buffs[i].HasStatModifiers;
                    buffs[i].OnRemoved(aOwner);
                    buffs.RemoveAt(i);
                    RefreshOwnerStatsIfNeeded(aOwner, hadStatModifiers);
                }
            }
            return remaining;
        }

        public void RemoveFirstBuffOfType<T>(Entity aOwner) where T : Buff
        {
            ThreadAffinity.AssertSimThread();
            for (int i = 0; i < buffs.Count; i++)
            {
                if (buffs[i] is not T) continue;
                bool hadStatModifiers = buffs[i].HasStatModifiers;
                buffs[i].OnRemoved(aOwner);
                buffs.RemoveAt(i);
                RefreshOwnerStatsIfNeeded(aOwner, hadStatModifiers);
                return;
            }
        }

        public List<Buff> GetAllBuffs()
        {
            ThreadAffinity.AssertSimThread();
            return buffs;
        }

        public double GetStatusFlat(string aStat)
        {
            ThreadAffinity.AssertSimThread();
            return SumStatusModifiers(aStat, true);
        }

        public double GetStatusPercent(string aStat)
        {
            ThreadAffinity.AssertSimThread();
            return SumStatusModifiers(aStat, false);
        }

        public bool HasControl()
        {
            ThreadAffinity.AssertSimThread();
            for (int i = 0; i < buffs.Count; i++)
            {
                if (!buffs[i].HasControl)
                {
                    return false;
                }
            }

            return true;
        }

        bool HasStatModifiers => buffs.Any(x => x.HasStatModifiers);

        void RefreshOwnerStatsIfNeeded(Entity aOwner, bool aMaybeChangedStats)
        {
            if (!aMaybeChangedStats)
            {
                return;
            }

            aOwner.RefreshStatsFromStatusChange();
        }

        double SumStatusModifiers(string aStat, bool aFlat)
        {
            Dictionary<string, double> strongestByCategory = new Dictionary<string, double>(StringComparer.Ordinal);
            for (int i = 0; i < buffs.Count; i++)
            {
                StatusModifier[] modifiers = buffs[i].StatusModifiers;
                for (int j = 0; j < modifiers.Length; j++)
                {
                    if (modifiers[j].Flat != aFlat) continue;
                    if (!string.Equals(modifiers[j].Stat, aStat, StringComparison.Ordinal)) continue;

                    double value = modifiers[j].Amount * Math.Max(1, buffs[i].Count);
                    string modifierCategory = string.IsNullOrWhiteSpace(modifiers[j].Category)
                        ? buffs[i].StackingCategory
                        : modifiers[j].Category;
                    string key = modifierCategory + "|" + modifiers[j].Stat + "|" + aFlat;
                    if (!strongestByCategory.TryGetValue(key, out double current) || Math.Abs(value) > Math.Abs(current))
                    {
                        strongestByCategory[key] = value;
                    }
                }
            }

            return strongestByCategory.Values.Sum();
        }
    }
}
