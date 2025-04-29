using Project_1.Camera;
using Project_1.Input;
using Project_1.Textures;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Project_1.GameObjects.Doodads
{
    internal class Doodad : WorldObject
    {
        float maximumClickDistance = 5;

        public Doodad(Texture aTexture, WorldSpace aStartingPos) : base(aTexture, aStartingPos)
        {
        }

        public override float MaxSpeed => 0;

        public override bool Click(ClickEvent aClickEvent)
        {
            if (FeetPosition.DistanceTo(ObjectManager.Player.FeetPosition) >= maximumClickDistance) return false;

            return base.Click(aClickEvent);
        }
    }
}
