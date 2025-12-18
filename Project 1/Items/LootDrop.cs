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

        static int nextId;
        public int Id { get; }
        public WorldSpace DropperFeet => dropperFeet;
        WorldSpace dropperFeet;
        public float DropperHalfHeight => dropperHalfHeight;
        float dropperHalfHeight;

        [JsonIgnore]
        public bool Despawned;

        [JsonIgnore]
        public bool IsEmpty => drop.All(drop => drop == null);

        [JsonIgnore]
        public bool InDistance
        {
            get
            {
                float allowed = dropperHalfHeight + (float)ObjectManager.Player.FeetSize.Y / 2f;
                return dropperFeet.DistanceTo(ObjectManager.Player.FeetPosition) < allowed;
            }
        }

        //public LootDrop()

        public LootDrop(Item[] drop, WorldObject @object)
        {
            this.drop = drop;
            Id = System.Threading.Interlocked.Increment(ref nextId);
            dropperFeet = @object.FeetPosition;
            dropperHalfHeight = (float)@object.FeetSize.Y / 2f;
        }

        public void SetDrop(Item[] newDrop)
        {
            drop = newDrop;
        }
    }
}
