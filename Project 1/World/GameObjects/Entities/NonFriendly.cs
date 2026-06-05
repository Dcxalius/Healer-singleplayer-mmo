using Microsoft.Xna.Framework;
using Project_1.Camera;
using Project_1.GameObjects.Spawners.Pathing;
using Project_1.GameObjects.Unit;
using Project_1.GameObjects.Unit.Stats;
using Project_1.Items;
using Project_1.Managers;
using Project_1.Textures;
using Project_1.UI.UIElements;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Project_1.GameObjects.Entities
{
    internal class NonFriendly : Entity
    {
        public override Color MinimapColor => Color.Red; //TODO: Settable, but should it be filterable? Maybe even implement the tracking system, allowing to set the different trackings to different colors?
        //TODO: Determine how much filtering that tracking system should allow? Color per type of filter?
        public enum BehaviourWhenAttacked //TODO: Implement AI behaviour trees, mobs can be relative simple but NPCs should be more advanced
        {
            Flee,
            Retaliate,
            RetaliateButFleeWhenLow
        }

        public override bool InCombat => aggroTable.Count > 0;

        MobPathing pathing;
        AggroTable aggroTable;

        public SavedMobData SavedMobData => UnitData as SavedMobData;
        //TODO: Since the breakup is probably going to be NF => Mob, NF => Npc, this should prob be in mob

        public NonFriendly(MobPathing aPathing, SavedMobData aUnitData) : base(aUnitData)
        {
            ThreadAffinity.AssertSimThread();
            aggroTable = new AggroTable(this);
            pathing = aPathing;
            
        }

        public override void Update()
        {
            ThreadAffinity.AssertSimThread();
            base.Update();

            aggroTable.Update();


            GetNewPath(); //TODO: Check if this should actually be done after update
        }

        void GetNewPath()
        {
            if (!Destination.HasDestination)
            {
                WorldSpace? nextSpace = pathing.GetNextSpace;
                if (!nextSpace.HasValue) return;
                Destination.AddDestination(nextSpace.Value);
            }
        }

        protected override void Death()
        {
            ThreadAffinity.AssertSimThread();
            int[] averageLevel = aggroTable.GetLevelOfAggroTable(); //TODO: Change this to not be dependant on aggroTable?
            //TODO: Internal damage by party tracker for exp calcs?

            int exp = UnitData.Level.ExpReward((int)Math.Round(averageLevel.Average()));
            aggroTable.Tagger.ExpToParty(exp);
            aggroTable.ClearTable();
            base.Death();
        }

        protected override void ProcessDamage(Entity aCause, string aCauseName, float aDamageTaken, float aThreatMod, DamageType aDamageType, Color aBorderColor, string aPrefix, string aSuffix) //TODO: Determine if this should always be called, even if attack dealt no damage to handle threat. Alternatively, handle base threat elsewhere and only do damage threat here
        {
            ThreadAffinity.AssertSimThread();
            aggroTable.AddToAggroTable(aCause, aDamageTaken * aThreatMod);
            base.ProcessDamage(aCause, aCauseName, aDamageTaken, aThreatMod, aDamageType, aBorderColor, aPrefix, aSuffix);
        }

        public virtual void AddToAggroTable(Entity aEntityToAdd, float aThreatValue) //TODO: Move
        {
            ThreadAffinity.AssertSimThread();
            aggroTable.AddToAggroTable(aEntityToAdd, aThreatValue);
        }

        public void RemoveFromAggroTable(Entity aEntity) //TODO: Move
        {
            ThreadAffinity.AssertSimThread();
            aggroTable.RemoveFromAggroTable(aEntity);
        }

        protected override bool CheckForRelation()
        {
            if (target.RelationToPlayer != RelationToPlayer) return true; //TODO: Deeper check here. Npcs should check factions, Mobs should check type of mob

            return false;
        }

        public override void ExpToParty(int aExpAmount)
        {
            throw new NotImplementedException();
        }
    }
}
