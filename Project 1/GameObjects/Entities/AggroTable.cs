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

        List<AggroEntity> aggroEntities; //TODO: Ponder if this should be a dict or a heap
        readonly TimeSpan maxAggroDurationStaleness = TimeSpan.FromSeconds(10);

        public Entity Tagger => aggroEntities.MinBy(aggroEntity => aggroEntity.TimeSinceLastHit).Entity;

        NonFriendly owner;

        public AggroTable(NonFriendly aOwner)
        {
            owner = aOwner;
            aggroEntities = new List<AggroEntity>();
        }

        public void ClearTable()
        {
            for (int i = 0; i < aggroEntities.Count; i++)
            {
                aggroEntities[i].Entity.RemovedFromAggroTable(owner);
            }

            aggroEntities.Clear();
        }

        int Contains(Entity aEntity)
        {
            for (int i = 0; i < aggroEntities.Count; i++)
            {
                AggroEntity entry = aggroEntities[i];
                if (entry.Entity == aEntity) return i;
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
                if (index >= 0) highestThreat = aggroEntities[index].Threat;
            }
            foreach (var item in aggroEntities)
            {
                if (item.Threat > highestThreat * 1.05) owner.SetTarget(item.Entity);
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
            if (aggroEntities[i].TimeSinceLastHit + maxAggroDurationStaleness < TimeManager.TotalFrameTimeAsTimeSpan)
            {
                ClearAggro(i);
            }
        }

        void ClearAggro(int i)
        {

            aggroEntities[i].Entity.RemovedFromAggroTable(owner);
            AquireNewTargetIfClearedWasHighest(i);
            aggroEntities.Remove(aggroEntities[i]);
        }

        void AquireNewTargetIfClearedWasHighest(int i)
        {
            if (owner.Target == aggroEntities[i].Entity)
            {
                Entity newTarget = null;
                float highestNewAggro = 0;
                for (int j = 0; j < aggroEntities.Count; j++)
                {
                    if (i == j) continue;

                    if (aggroEntities.Count == 0)
                    {
                        owner.RemoveTarget();
                        return;
                    }
                    if (aggroEntities[j].Threat > highestNewAggro)
                    {
                        newTarget = aggroEntities[j].Entity;
                        highestNewAggro = aggroEntities[j].Threat;
                    }
                }
                owner.SetTarget(newTarget);

            }
        }

        public virtual void AddToAggroTable(Entity aEntityToAdd, float aThreatValue)
        {
            int i = Contains(aEntityToAdd);
            if (i >= 0)
            {
                //(Entity, float, TimeSpan, TimeSpan) entry = (aEntityToAdd, aggroEntities[i].Item2 + aThreatValue, TimeManager.TotalFrameTimeAsTimeSpan, aggroEntities[i].Item4);
                //aggroEntities[i] = entry;

                AggroEntity entry = aggroEntities[i];

                entry.Threat = aggroEntities[i].Threat + aThreatValue;
                entry.TimeFirstHitMe = TimeManager.TotalFrameTimeAsTimeSpan;

                aggroEntities[i] = entry;
                return;
            }

            AggroEntity aggroEntity = new AggroEntity(aEntityToAdd, aThreatValue, TimeManager.TotalFrameTimeAsTimeSpan);
            aggroEntities.Add(aggroEntity);

            aEntityToAdd.AddedToAggroTable(owner);
        }

        public void RemoveFromAggroTable(Entity aEntity)
        {
            Debug.Assert(aEntity != null);
            int index = Contains(aEntity);
            Debug.Assert(index >= 0);

            aggroEntities.RemoveAt(index);
        }

        public int[] CalculateAverageLevel()
        {
            int[] returnable = new int[aggroEntities.Count];
            for (int i = 0; i < aggroEntities.Count; i++)
            {
                returnable[i] = aggroEntities[i].Entity.CurrentLevel;
            }
            return returnable;
        }
    }
}
