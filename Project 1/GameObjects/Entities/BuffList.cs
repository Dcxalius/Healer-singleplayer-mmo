using Project_1.GameObjects.Spells.Buff;
using Project_1.UI.HUD;
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
            for (int i = buffs.Count - 1; i >= 0; i--)
            {
                if (buffs[i].IsOver)
                {
                    buffs.RemoveAt(i);
                    continue;
                }
                buffs[i].Update(aOwner);
            }
        }

        public void AddBuff(Buff aBuff, Entity aOwner)
        {
            for (int i = 0; i < buffs.Count; i++)
            {
                if (buffs[i] == aBuff)
                {
                    buffs[i].Recast();
                    return;
                }
            }

            buffs.Add(aBuff);
            HUDManager.AddBuff(buffs.Last(), aOwner);
        }

        public List<Buff> GetAllBuffs()
        {
            return buffs;
        }
    }
}
