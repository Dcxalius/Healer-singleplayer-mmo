﻿using Microsoft.Xna.Framework;
using Project_1.Managers;
using Project_1.Textures;
using SharpDX.MediaFoundation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Project_1.GameObjects.Entities
{
    internal class NonFriendly : Entity
    {

        List<Entity> aggroEntities = new List<Entity>();
        Dictionary<Entity, float> aggroValues = new Dictionary<Entity, float>();
        Dictionary<Entity, double> aggroDurations = new Dictionary<Entity, double>();
        double maxAggroDurationStaleness = TimeSpan.FromSeconds(5).TotalMilliseconds;
        public NonFriendly(Texture aTexture, Vector2 aStartingPos) : base(aTexture, aStartingPos)
        {

        }

        public override void Update()
        {
            base.Update();

            Aggro();

        }

        void ClearStaleAggro(int i)
        {
            if (aggroDurations[aggroEntities[i]] + maxAggroDurationStaleness < TimeManager.CurrentFrameTime)
            {
                aggroValues.Remove(aggroEntities[i]);
                aggroDurations.Remove(aggroEntities[i]);
                if (aggroEntities[i] == target)
                {
                    AquireNewTarget(i);
                }
                aggroEntities[i].RemovedFromAggroTable(this);
                aggroEntities.Remove(aggroEntities[i]);
            }
        }

        bool ClearDeadAggro(int i)
        {
            if (aggroEntities[i].Alive)
            {
                return false;
            }

            aggroValues.Remove(aggroEntities[i]);
            aggroDurations.Remove(aggroEntities[i]);
            aggroEntities[i].RemovedFromAggroTable(this);
            AquireNewTarget(i);
            aggroEntities.Remove(aggroEntities[i]);
            return true;
        }

        void AquireNewTarget(int i)
        {
            if (target == aggroEntities[i])
            {
                Entity newTarget = null;
                float v = 0;
                for (int j = 0; j < aggroEntities.Count; j++)
                {
                    if (aggroValues.Count == 0)
                    {
                        target = null;
                        return;
                    }
                    if (aggroValues[aggroEntities[i]] > v)
                    {
                        newTarget = aggroEntities[i];
                        v = aggroValues[aggroEntities[i]];
                    }
                }
                target = newTarget;

            }
        }

        public override void TakeDamage(Entity aAttacker, float aDamageTaken)
        {
            AddToAggroTable(aAttacker, aDamageTaken);
            base.TakeDamage(aAttacker, aDamageTaken);

        }

        protected virtual void AddToAggroTable(Entity aAttacker, float aDamageTaken)
        {
            if (aggroEntities.Contains(aAttacker))
            {
                aggroValues[aAttacker] += aDamageTaken;
                aggroDurations[aAttacker] = TimeManager.CurrentFrameTime;
                return;
            }


            aggroEntities.Add(aAttacker);
            aggroValues.Add(aAttacker, aDamageTaken);
            aggroDurations[aAttacker] = TimeManager.CurrentFrameTime;

            aAttacker.AddedToAggroTable(this);
        }

        protected virtual void Aggro()
        {
            AquireTargetThroughAggro();
            CleaningAggroTable();
        }

        void AquireTargetThroughAggro()
        {
            float highestThreat = 0;

            if (target != null && aggroValues.ContainsKey(target))
            {

                highestThreat = aggroValues[target];
            }
            foreach (var item in aggroValues)
            {
                if (item.Value > highestThreat * 1.05)
                {
                    target = item.Key;

                }
            }
        }

        void CleaningAggroTable()
        {
            for (int i = aggroEntities.Count - 1; i >= 0; i--)
            {
                if (ClearDeadAggro(i))
                {
                    continue;
                }

                ClearStaleAggro(i);
            }
        }
    }
}