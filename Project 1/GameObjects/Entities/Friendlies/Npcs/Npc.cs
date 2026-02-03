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

namespace Project_1.GameObjects.Entities.Friendlies.Npcs
{
    internal class Npc : Friendly //TODO: Should this be friendly?
    {
        const float speakRange = 100f;
        GossipData gossip;
        static Npc activeConversation;

        public bool InConversationRange(WorldSpace aFeetPos) => aFeetPos.DistanceTo(FeetPosition) < speakRange;

        public Npc(UnitData aUnitData) : base(aUnitData)
        {
            gossip = ObjectFactory.GetGossip(Name);
        }

        public bool TryBeginConversation()
        {
            var player = ObjectManager.Player;
            if (player == null) return false;
            if (!InConversationRange(player.FeetPosition)) return false;

            BeginConversation(this);
            Mailboxes.Ui.Publish(new GossipOpened(new GossipUiSnapshot(gossip.Options, gossip.LinkTree, gossip.StartIndex)));
            return true;
        }

        public override void Update()
        {
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
            if (activeConversation == npc) return;
            EndConversation();
            activeConversation = npc;
        }

        static void EndConversation()
        {
            if (activeConversation == null) return;
            activeConversation = null;
            Mailboxes.Ui.Publish(new GossipClosed());
            Mailboxes.Ui.Publish(new ShopClosed());
        }
    }
}
