using Project_1.Camera;
using Project_1.GameObjects.Entities.Players;
using Project_1.Managers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static Project_1.GameObjects.Spells.AoE.AreaOfEffectData;

namespace Project_1.GameObjects.Entities
{
    internal class Destination
    {
        public bool HasDestination => destinations.Count > 0;
        public WorldSpace DirectionToWalk => directionToWalk;
        public float LengthTo => lengthTo;
        float lengthTo; 
        
        WorldSpace directionToWalk;
        
        List<WorldSpace> destinations;
        Entity owner;
        
        public Destination(Entity aOwner)
        {
            destinations = new List<WorldSpace>();
            owner = aOwner;
        }

        public void Update()
        {
            //if (owner.Momentum.ToVector2().Length() < 0.1f)
            //{
            //    owner.Momentum = WorldSpace.Zero;
            //}
            if (owner is Player)
            {
                if (!(owner as Player).LockedMovement) return;
            }


            if (destinations.Count == 0 && owner.Target == null)
            {
                directionToWalk = WorldSpace.Zero;
                lengthTo = 0;
                return;
            }

            if (owner.Target != null)
            {
                OverwriteDestination(owner.Target.FeetPosition);

            }

            UpdateDirection(destinations[0]);
        }

        public WorldSpace GetVelocity(float aAttackRange, float aSpeed)
        {
            if (owner.Target == null)
            {
                if (LengthTo < owner.Momentum.ToVector2().Length() * 10f) //TODO: Find a good factor
                {
                    if (HasDestination)
                    {
                        RemoveFirst();
                    }

                    return WorldSpace.Zero;
                }

            }
            else
            {
                if (LengthTo < aAttackRange - owner.Target.Size.X / 2 - owner.Size.X / 2)
                {
                    if (HasDestination)
                    {
                        RemoveFirst();
                    }
                    //Momentum = owner.Momentum / 1.6f;

                    return WorldSpace.Zero;
                }
            }

            return DirectionToWalk * aSpeed * (float)TimeManager.SecondsSinceLastFrame;
        }

        public void RemoveFirst() => destinations.RemoveAt(0);

        public void OverwriteDestination(WorldSpace aDestination)
        {
            destinations.Clear();
            destinations.Add(aDestination);
        }

        public void AddDestination(WorldSpace aDestination) => destinations.Add(aDestination);

        void UpdateDirection(WorldSpace aDestination)
        {
            WorldSpace dirV = aDestination - owner.FeetPosition;
            lengthTo = dirV.ToVector2().Length();
            dirV.Normalize();
            directionToWalk = dirV;
        }


    }
}
