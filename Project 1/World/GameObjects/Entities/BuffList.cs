using Project_1.GameObjects.Spells;
using Project_1.GameObjects.Spells.Buff;
using Project_1.GameObjects.Unit.Stats;
using Project_1.Managers;
using Project_1.UI.HUD.Managers;
using Project_1.Messaging;
using Project_1.Messaging.Events;
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
                    buffs[i].OnRemoved(aOwner);
                    buffs.RemoveAt(i);
                }
            }
        }

        public void AddBuff(Buff aBuff, Entity aOwner)
        {
            ThreadAffinity.AssertSimThread();
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
                return;
            }

            buffs.Add(aBuff);
            aBuff.OnApplied(aOwner);
            MailboxManager.PublishUiEvent(new BuffAdded(aOwner.RenderId, buffs.Last().BuffUiSnapshot));
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
                if (buffs[i] is not AbsorbBuff absorb) continue;
                double absorbed = absorb.AbsorbDamage(remaining, damageType);
                totalAbsorbed += absorbed;
                remaining -= absorbed;
                if (absorb.IsDepleted)
                {
                    absorb.OnRemoved(aOwner);
                    buffs.RemoveAt(i);
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
                buffs[i].OnRemoved(aOwner);
                buffs.RemoveAt(i);
                return;
            }
        }

        public List<Buff> GetAllBuffs()
        {
            ThreadAffinity.AssertSimThread();
            return buffs;
        }
    }
}
