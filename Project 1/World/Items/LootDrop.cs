using Newtonsoft.Json;
using Project_1.Camera;
using Project_1.GameObjects;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Project_1.GameObjects.Entities.Friendlies.Players;

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
            if (@object == null) throw new ArgumentNullException(nameof(@object));
            this.drop = drop ?? Array.Empty<Item>();
            Id = System.Threading.Interlocked.Increment(ref nextId);
            dropperFeet = @object.FeetPosition;
            dropperHalfHeight = (float)@object.FeetSize.Y / 2f;
        }

        [JsonConstructor]
        public LootDrop(Item[] drop, WorldSpace dropperFeet, float dropperHalfHeight, int id)
        {
            this.drop = drop ?? Array.Empty<Item>();
            Id = id > 0 ? id : System.Threading.Interlocked.Increment(ref nextId);
            if (Id > nextId) nextId = Id;
            this.dropperFeet = dropperFeet;
            this.dropperHalfHeight = dropperHalfHeight;
        }

        public void SetDrop(Item[] newDrop)
        {
            drop = newDrop;
        }
    }
}
