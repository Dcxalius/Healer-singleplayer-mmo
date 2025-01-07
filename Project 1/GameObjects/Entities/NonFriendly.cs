using Microsoft.Xna.Framework;
using Project_1.Camera;
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

        AggroTable aggroTable;
        public NonFriendly(Texture aTexture, WorldSpace aStartingPos, Corpse aCorpse = null) : base(aTexture, aStartingPos, aCorpse)
        {
            aggroTable = new AggroTable(this);
        }

        protected override void Death()
        {
            base.Death();

            aggroTable.ClearTable();
        }

        public override void Update()
        {
            base.Update();

            aggroTable.Update();
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
    }
}
