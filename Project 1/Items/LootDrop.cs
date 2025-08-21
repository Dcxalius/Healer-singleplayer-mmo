using Newtonsoft.Json;
using Project_1.Camera;
using Project_1.GameObjects;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Project_1.Items
{
    internal class LootDrop //TODO: Better name?
    {
        public Item[] Drop => drop;
        Item[] drop;

        WorldObject dropper;

        [JsonIgnore]
        public bool Despawned;

        [JsonIgnore]
        public bool IsEmpty => drop.All(drop => drop == null);

        [JsonIgnore]
        public bool InDistance => dropper.FeetPosition.DistanceTo(ObjectManager.Player.FeetPosition) < dropper.FeetSize.Y / 2 + ObjectManager.Player.FeetSize.Y / 2;

        //public LootDrop()

        public LootDrop(Item[] drop, WorldObject @object)
        {
            this.drop = drop;
            dropper = @object;
        }
    }
}
