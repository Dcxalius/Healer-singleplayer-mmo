using Microsoft.Xna.Framework;
using Project_1.Camera;
using Project_1.GameObjects.Unit;
using Project_1.Textures;
using Project_1.Textures.AnimatedTextures;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Project_1.GameObjects.Entities.Friendlies.GuildMembers;
using Project_1.GameObjects.Entities.Friendlies.Players;
using Project_1.Managers;

namespace Project_1.GameObjects.Entities.Friendlies.GuildMembers
{
    internal class GuildMember : Friendly, ILightEmitter
    {
        public float LightRadiusTiles => 5f;
        

        public AttackTree AttackLogic => attackTree;
        internal float MinimumAttackRange => CalculateMinimumAttackRange();

        readonly AttackTree attackTree;

        public struct GuildMemberData
        {
            public string Name => name;
            public string Level => level;
            public string Class => @class;

            string name;
            string level;
            string @class;

            public GuildMemberData(string aName, int aLevel, string aClass)
            {
                name = aName;
                level = aLevel.ToString();
                @class = aClass;
            }
        }

        public bool Leaving => leaving;
        bool leaving;

        public GuildMember(UnitData aData) : base(aData)
        {
            ThreadAffinity.AssertSimThread();
            RemoveNamePlate(); //TODO: Think of a better way to handle this
            leaving = false;
            attackTree = new AttackTree(this);
        }

        public override void Update()
        {
            ThreadAffinity.AssertSimThread();
            base.Update();


            CheckLeavingDistance();
        }

        void CheckLeavingDistance()
        {
            if (!leaving || HasDestination)
            {
                return;
            }

            RemoveNamePlate();
            ObjectManager.RemoveEntity(this);
        }

        public override void RefreshPlates()
        {
            ThreadAffinity.AssertSimThread();
            if (HasNamePlate) base.RefreshPlates();
        }

        public void AddedToParty()
        {
            ThreadAffinity.AssertSimThread();
            CreateNamePlate();
            FlagForRefresh();
            leaving = false;
        }

        public void RemovedFromParty()
        {
            ThreadAffinity.AssertSimThread();
            leaving = true ;
        }

        public void RecieveDirectWalkingOrder(WorldSpace aPos)
        {
            ThreadAffinity.AssertSimThread();
            target = null;
            Destination.OverwriteDestination(aPos);
        }


        public void AddWalkingOrder(WorldSpace aPos)
        {
            ThreadAffinity.AssertSimThread();
            Destination.AddDestination(aPos);
        }

        protected override bool CheckForRelation()
        {
            if (target.RelationToPlayer == Relation.RelationToPlayer.Self || target.RelationToPlayer == Relation.RelationToPlayer.Friendly)
            {
                return false;
            }
            if (target.RelationToPlayer != RelationToPlayer)
            {
                return true;
            }

            return false;

        }

        public override void ExpToParty(int aExpAmount)
        {
            ThreadAffinity.AssertSimThread();
            Party party = ObjectManager.Player.Party;
            if (!party.IsInParty(this))
            {
                GainExperience(aExpAmount);
                return;
            }

            party.DivideExpAmongParty(aExpAmount);

        }

        float CalculateMinimumAttackRange()
        {
            AttackData attacks = UnitData.AttackData;

            if (attacks.MainHandAttack != null && attacks.OffHandAttack != null)
            {
                return Math.Min(attacks.MainHandAttack.Range, attacks.OffHandAttack.Range);
            }

            if (attacks.MainHandAttack == null && attacks.OffHandAttack != null)
            {
                return attacks.OffHandAttack.Range;
            }

            if (attacks.MainHandAttack != null)
            {
                return attacks.MainHandAttack.Range;
            }

            return 0f;
        }
    }
}
