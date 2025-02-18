using Project_1.Camera;
using Project_1.GameObjects.Entities.Players;
using Project_1.Managers;
using Project_1.Tiles;
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
        public bool HasDestination => destinations.Count > 0 || currentPath != null;
        public WorldSpace DirectionToWalk => directionToWalk;
        public float LengthTo => lengthTo;
        float lengthTo; 
        
        WorldSpace directionToWalk;
        
        List<WorldSpace> destinations; //TODO: Remove this and rely only on the path
        Entity owner;

        Path currentPath;
        public Destination(Entity aOwner)
        {
            destinations = new List<WorldSpace>();
            owner = aOwner;
        }

        public void Update() //TOOD: Rework this ASAP
        {
            //if (owner.Momentum.ToVector2().Length() < 0.1f)
            //{
            //    owner.Momentum = WorldSpace.Zero;
            //}
            if (owner is Player)
            {
                if (!(owner as Player).LockedMovement) return;
            }

            if (currentPath != null && currentPath.Count == 0) currentPath = null;
            if (destinations.Count == 0 && owner.Target == null && currentPath != null)
            {
                destinations.Add(currentPath.ComsumeNextPoint);
                //directionToWalk = WorldSpace.Zero;
                //lengthTo = 0;
                //return;
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

        public WorldSpace GetVelocity(float aAttackRange, float aSpeed, WorldSpace aSize)
        {
            if (owner.Target == null)
            {
                bool xIsBigger = Math.Abs(DirectionToWalk.X) >= Math.Abs(DirectionToWalk.Y);
                if(directionToWalk.X != 0)
                {

                }
                float lengthOfCross;
                if (xIsBigger) lengthOfCross = aSize.X * (aSize.X / (aSize.X * Math.Abs(DirectionToWalk.X)));
                else lengthOfCross = aSize.Y * (aSize.Y / (aSize.Y * Math.Abs(DirectionToWalk.Y)));

                if (LengthTo < lengthOfCross / 2)
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

        public void AddDestination(WorldSpace aDestination)
        {
            destinations.Clear();
            currentPath = TileManager.GetPath(owner.FeetPosition, aDestination, new WorldSpace(owner.FeetSize));
            //destinations.AddRange();
        }

        void UpdateDirection(WorldSpace aDestination)
        {
            WorldSpace dirV = aDestination - owner.FeetPosition;
            if (dirV.X == 0 && dirV.Y == 0)
            {
                //TODO: Bugfix, if the destination is the same as feetposition it will teleport the entity to Infinity
            }
            lengthTo = dirV.ToVector2().Length();
            dirV.Normalize();
            directionToWalk = dirV;
        }


    }
}
