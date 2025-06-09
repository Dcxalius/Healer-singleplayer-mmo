using Project_1.Camera;
using Project_1.GameObjects.Unit;
using Project_1.Input;
using Project_1.UI.HUD.Managers;
using Project_1.UI.HUD.Windows.Gossip;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Project_1.GameObjects.Entities.Npcs
{
    internal class Npc : Friendly
    {
        const float speakRange = 100f;
        GossipData gossip;

        public bool InConversationRange(WorldSpace aFeetPos) => aFeetPos.DistanceTo(FeetPosition) < speakRange;

        public Npc(UnitData aUnitData) : base(aUnitData)
        {
            gossip = ObjectFactory.GetGossip(Name);
        }

        protected override void ClickedOn(ClickEvent aClickEvent)
        {
            base.ClickedOn(aClickEvent);

            if (!InConversationRange(ObjectManager.Player.FeetPosition)) return;

            HUDManager.windowHandler.OpenGossipWindow(gossip.Start, this);
        }

        public override void ExpToParty(int aExpAmount)
        {
            throw new NotImplementedException();
        }

        protected override bool CheckForRelation()
        {
            throw new NotImplementedException();
        }
    }
}
