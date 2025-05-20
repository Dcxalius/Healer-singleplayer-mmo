using Project_1.GameObjects.Unit;
using Project_1.Input;
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

        public Npc(UnitData aUnitData) : base(aUnitData)
        {

        }

        protected override void ClickedOn(ClickEvent aClickEvent)
        {
            base.ClickedOn(aClickEvent);

            if (ObjectManager.Player.DistanceTo(FeetPosition) < speakRange) return;

            //TODO: Open gossip window
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
