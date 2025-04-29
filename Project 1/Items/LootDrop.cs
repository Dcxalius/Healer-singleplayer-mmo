using Project_1.Camera;
using Project_1.GameObjects;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Project_1.Items
{
    internal class LootDrop
    {
        public Item[] Drop => drop;
        Item[] drop;

        WorldObject dropper;

        public bool Despawned;

        public bool IsEmpty => drop.All(drop => drop == null);

        public bool InDistance => dropper.FeetPosition.DistanceTo(ObjectManager.Player.FeetPosition) < dropper.FeetSize.Y / 2 + ObjectManager.Player.FeetSize.Y / 2;

        

        public LootDrop(Item[] aDrop, WorldObject aObject)
        {
            drop = aDrop;
            dropper = aObject;
        }
    }
}
