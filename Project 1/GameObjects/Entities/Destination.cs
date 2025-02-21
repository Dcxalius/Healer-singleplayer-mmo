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
        public bool HasDestination => CurrentPath != null;
        public WorldSpace DirectionToWalk => directionToWalk;
        public float LengthTo => lengthTo;
        float lengthTo; 
        
        WorldSpace directionToWalk;
        
        Entity owner;
        private void CheckIfClear()
        {
            if (!HasDestination) return;

            if (CurrentPath.Count == 0) paths.RemoveAt(0);
        }

        Path CurrentPath
        {
            get
            {
                if (paths.Count == 0) return null;
                return paths[0];
            }
        }

        List<Path> paths;
        WorldSpace? destination;
        public Destination(Entity aOwner)
        {
            owner = aOwner;
        }

        public void Update()
        {
            
            if (owner is Player)
            {
                if (!(owner as Player).LockedMovement) return;
            }

            if (!HasDestination) destination = null;
            if (owner.Target == null && CurrentPath != null && destination == null)
            {
                destination = CurrentPath.ComsumeNextPoint;
            }

            if (owner.Target == null && destination == null)
            {
                directionToWalk = WorldSpace.Zero;
                lengthTo = 0;
                return;
            }

            if (owner.Target != null)
            {
                OverwriteDestination(owner.Target.FeetPosition);

            }

            UpdateDirection(destination.Value);
        }

        public WorldSpace GetVelocity(float aAttackRange, float aSpeed, WorldSpace aSize)
        {
            if (owner.Target == null)
            {
                bool xIsBigger = Math.Abs(DirectionToWalk.X) >= Math.Abs(DirectionToWalk.Y);
                
                float lengthOfBiggestCrossSection;

                if (xIsBigger) lengthOfBiggestCrossSection = aSize.X * (aSize.X / (aSize.X * Math.Abs(DirectionToWalk.X)));
                else lengthOfBiggestCrossSection = aSize.Y * (aSize.Y / (aSize.Y * Math.Abs(DirectionToWalk.Y)));

                if (LengthTo < lengthOfBiggestCrossSection / 2)
                {
                    CheckIfClear();
                    destination = null;
                    return WorldSpace.Zero;
                }

            }
            else
            {
                if (LengthTo < aAttackRange - owner.Target.Size.X / 2 - owner.Size.X / 2)
                {
                    CheckIfClear();
                    destination = null;
                    return WorldSpace.Zero;
                }
            }

            return DirectionToWalk * aSpeed * (float)TimeManager.SecondsSinceLastFrame;
        }


        public void OverwriteDestination(WorldSpace aDestination)
        {
            paths.Clear();
            paths.Add(TileManager.GetPath(owner.FeetPosition, aDestination, new WorldSpace(owner.FeetSize)));
        }

        public void AddDestination(WorldSpace aDestination)
        {
            if (paths.Count > 0)
            {
                paths.Add(TileManager.GetPath(paths[paths.Count - 1].CheckLastSpace, aDestination, new WorldSpace(owner.FeetSize)));

                return;
            }
            paths.Add(TileManager.GetPath(owner.FeetPosition, aDestination, new WorldSpace(owner.FeetSize)));
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
