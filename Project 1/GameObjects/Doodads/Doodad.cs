using Project_1.Camera;
using Project_1.GameObjects.Entities.GroundEffect;
using Project_1.Textures;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Project_1.GameObjects.Entities.Friendlies.Players;

namespace Project_1.GameObjects.Doodads
{
    internal class Doodad : WorldObject
    {
        float maximumClickDistance = 500;

        public Doodad(Texture aTexture, WorldSpace aStartingPos) : base(aTexture, aStartingPos)
        {
            groundEffects.Add(new Shadow());
        }


        public override float MaxSpeed => 0;

        public bool CanInteract(WorldSpace worldPos)
        {
            if (FeetPosition.DistanceTo(ObjectManager.Player.FeetPosition) >= maximumClickDistance) return false;
            return HitTest(worldPos);
        }
    }
}
