using Microsoft.Xna.Framework;
using Project_1.Camera;
using Project_1.GameObjects.Spawners.Pathing;
using Project_1.GameObjects.Unit;
using Project_1.Items;
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
        public enum BehaviourWhenAttacked
        {
            Flee,
            Retaliate,
            RetaliateButFleeWhenLow
        }

        public override bool InCombat => aggroTable.Count > 0;

        MobPathing pathing;

        AggroTable aggroTable;
        public NonFriendly(MobPathing aPathing, SavedMobData aUnitData) : base(aUnitData)
        {
            aggroTable = new AggroTable(this);
            pathing = aPathing;
            
        }

        public override void Update()
        {
            base.Update();

            aggroTable.Update();

            if (!Destination.HasDestination)
            {
                WorldSpace? nextSpace = pathing.GetNextSpace;
                if (!nextSpace.HasValue) return;
                Destination.AddDestination(nextSpace.Value);
            }

        }
        protected override void Death()
        {

            int[] averageLevel = aggroTable.GetLevelOfAggroTable();

            int exp = UnitData.Level.ExpReward((int)Math.Round(averageLevel.Average()));
            aggroTable.Tagger.ExpToParty(exp);
            aggroTable.ClearTable();
            base.Death();
        }

        public override void TakeDamage(Entity aAttacker, float aDamageTaken)
        {
            base.TakeDamage(aAttacker, aDamageTaken);
            aggroTable.AddToAggroTable(aAttacker, aDamageTaken);

        }

        public virtual void AddToAggroTable(Entity aEntityToAdd, float aThreatValue)
        {
            aggroTable.AddToAggroTable(aEntityToAdd, aThreatValue);
        }

        public void RemoveFromAggroTable(Entity aEntity)
        {
            aggroTable.RemoveFromAggroTable(aEntity);
        }

        protected override bool CheckForRelation()
        {
            if (target.RelationToPlayer != RelationToPlayer) return true;
            

            return false;
            
        }

        public override void ExpToParty(int aExpAmount)
        {
            throw new NotImplementedException();
        }
    }
}
