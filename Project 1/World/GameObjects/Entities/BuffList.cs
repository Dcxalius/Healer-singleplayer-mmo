using Project_1.GameObjects.Spells.Buff;
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
                if (buffs[i] == aBuff)
                {
                    buffs[i].Recast();
                    MailboxManager.PublishUiEvent(new BuffAdded(aOwner.RenderId, BuildSnapshot(buffs[i])));
                    return;
                }
            }

            buffs.Add(aBuff);
            aBuff.OnApplied(aOwner);
            MailboxManager.PublishUiEvent(new BuffAdded(aOwner.RenderId, BuildSnapshot(buffs.Last())));
        }

        static BuffUiSnapshot BuildSnapshot(Buff buff)
        {
            return new BuffUiSnapshot(buff.EffectId, buff.GfxPath, buff.DurationRemaining);
        }

        public List<Buff> GetAllBuffs()
        {
            ThreadAffinity.AssertSimThread();
            return buffs;
        }
    }
}
