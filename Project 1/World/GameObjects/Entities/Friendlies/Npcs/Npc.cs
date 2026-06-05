using Project_1.Camera;
using Project_1.GameObjects.Unit;
using Project_1.Messaging;
using Project_1.Messaging.Events;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Project_1.GameObjects.Entities.Friendlies.Players;
using Project_1.GameObjects.Entities.Friendlies.Npcs;
using Project_1.Managers;

namespace Project_1.GameObjects.Entities.Friendlies.Npcs
{
    internal class Npc : Friendly //TODO: Should this be friendly?
    {
        //TODO: Ponder what should be in here and what it should inherit from. This and mob should be pretty close, the only difference being that Npc should have interactions if neutral/friendly+, like gossip, shops, spelltraining, and quests
        //TODO: Make framework for guildmembers interacting with npcs

        const float speakRange = 100f;
        GossipData gossip;
        static Npc activeConversation;

        public bool InConversationRange(WorldSpace aFeetPos) => aFeetPos.DistanceTo(FeetPosition) < speakRange;

        public Npc(UnitData aUnitData) : base(aUnitData)
        {
            ThreadAffinity.AssertSimThread();
            gossip = ObjectFactory.GetGossip(Name);
        }

        public bool TryBeginConversation()
        {
            ThreadAffinity.AssertSimThread();
            var player = ObjectManager.Player;
            if (player == null) return false;
            if (!InConversationRange(player.FeetPosition)) return false;

            BeginConversation(this);
            MailboxManager.PublishUiEvent(new GossipOpened(new GossipUiSnapshot(gossip.Options, gossip.LinkTree, gossip.StartIndex, Name)));
            return true;
        }

        public override void Update()
        {
            ThreadAffinity.AssertSimThread();
            base.Update();
            if (activeConversation != this) return;
            var player = ObjectManager.Player;
            if (player == null) return;
            if (InConversationRange(player.FeetPosition)) return;
            EndConversation();
        }

        public override void ExpToParty(int aExpAmount)
        {
            throw new NotImplementedException();
        }

        protected override bool CheckForRelation()
        {
            throw new NotImplementedException();
        }

        static void BeginConversation(Npc npc)
        {
            ThreadAffinity.AssertSimThread();
            if (activeConversation == npc) return;
            EndConversation();
            activeConversation = npc; //TODO: Shouldn't there be events for starting conversations?
        }

        static void EndConversation()
        {
            ThreadAffinity.AssertSimThread();
            if (activeConversation == null) return;
            activeConversation = null;
            //TODO: Remove overarching event spam. Which is the closing conversation, there might be cases in the future where the player can have two shops open at once or something similar. At least shop + gossip or shop + spelltraining should be possible.
            MailboxManager.PublishUiEvent(new GossipClosed());
            MailboxManager.PublishUiEvent(new ShopClosed());
            MailboxManager.PublishUiEvent(new SpellTrainingClosed());
        }
    }
}
