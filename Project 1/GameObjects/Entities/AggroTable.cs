using Project_1.Managers;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static Project_1.GameObjects.Spells.AoE.AreaOfEffectData;

namespace Project_1.GameObjects.Entities
{
    internal class AggroTable
    {
        public int Count => aggroEntities.Count;

        List<(Entity, float, TimeSpan)> aggroEntities;
        readonly TimeSpan maxAggroDurationStaleness = TimeSpan.FromSeconds(10);
        NonFriendly owner;

        public AggroTable(NonFriendly aOwner)
        {
            owner = aOwner;
            aggroEntities = new List<(Entity, float, TimeSpan)>();
        }

        int Contains(Entity aEntity)
        {
            for (int i = 0; i < aggroEntities.Count; i++)
            {
                (Entity, float, TimeSpan) entry = aggroEntities[i];
                if (entry.Item1 == aEntity)
                {
                    return i;
                }
            }
            return -1;
        }
        public void Update()
        {
            AquireTargetThroughAggro();
            CleaningAggroTable();
        }
        void AquireTargetThroughAggro()
        {
            float highestThreat = 0;

            if (owner.Target != null)
            {
                int index = Contains(owner.Target);
                if (index >= 0)
                {
                    highestThreat = aggroEntities[index].Item2;

                }
            }
            foreach (var item in aggroEntities)
            {
                if (item.Item2 > highestThreat * 1.05)
                {
                    owner.Target = item.Item1;

                }
            }
        }

        void CleaningAggroTable()
        {
            for (int i = aggroEntities.Count - 1; i >= 0; i--)
            {
                ClearStaleAggro(i);
            }
        }

        void ClearStaleAggro(int i)
        {
            if (aggroEntities[i].Item3 + maxAggroDurationStaleness < TimeManager.TotalFrameTimeAsTimeSpan)
            {
                ClearAggro(i);
            }
        }

        void ClearAggro(int i)
        {

            aggroEntities[i].Item1.RemovedFromAggroTable(owner);
            AquireNewTarget(i);
            aggroEntities.Remove(aggroEntities[i]);
        }

        void AquireNewTarget(int i)
        {
            if (owner.Target == aggroEntities[i].Item1)
            {
                Entity newTarget = null;
                float v = 0;
                for (int j = 0; j < aggroEntities.Count; j++)
                {
                    if (i == j) continue;

                    if (aggroEntities.Count == 0)
                    {
                        owner.Target = null;
                        return;
                    }
                    if (aggroEntities[j].Item2 > v)
                    {
                        newTarget = aggroEntities[j].Item1;
                        v = aggroEntities[j].Item2;
                    }
                }
                owner.Target = newTarget;

            }
        }

        public virtual void AddToAggroTable(Entity aEntityToAdd, float aThreatValue)
        {
            int i = Contains(aEntityToAdd);
            if (i >= 0)
            {
                (Entity, float, TimeSpan) entry = (aEntityToAdd, aggroEntities[i].Item2 + aThreatValue, TimeManager.TotalFrameTimeAsTimeSpan);
                aggroEntities[i] = entry;
                return;
            }


            aggroEntities.Add((aEntityToAdd, aThreatValue, TimeManager.TotalFrameTimeAsTimeSpan));

            aEntityToAdd.AddedToAggroTable(owner);
        }

        public void RemoveFromAggroTable(Entity aEntity)
        {
            Debug.Assert(aEntity != null);
            int index = Contains(aEntity);
            Debug.Assert(index >= 0);

            aggroEntities.RemoveAt(index);
        }
    }
}
