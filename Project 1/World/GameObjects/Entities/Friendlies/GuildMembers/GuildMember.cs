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
    internal class GuildMember : Friendly, ILightEmitter //TODO: Find a better name? Since while they are ai, they are more playerlike than other npcs
    {
        public AttackTree AttackLogic => attackTree;
        internal float MinimumAttackRange => CalculateMinimumAttackRange();

        readonly AttackTree attackTree;

        public struct GuildMemberData
        {
            //Q: What is this for?
            //Its called in friendly which feels wrong af
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
            //TODO: This is pretty rough, we should probably have a more robust system for this, but for now this will do.
            if (!leaving || HasDestination)
            {
                return;
            }

            //TODO: Make sure this doesn't bug out if the player is dead or guildmember is in combat, potentially prevent leaving if the guildmember is in combat or the player is dead?
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

        protected override bool CheckForRelation() //Q: Feels like there is a cleaner way to do this.
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
                //TODO: Handling seperate party that the player is not in.
                GainExperience(aExpAmount);
                return;
            }

            party.DivideExpAmongParty(aExpAmount);
        }

        float CalculateMinimumAttackRange() //TODO: This feels like this should be deeper in Enitity even perhaps? Why is it here?
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
