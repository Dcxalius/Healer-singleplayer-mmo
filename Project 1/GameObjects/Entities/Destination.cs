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
    internal partial class Destination
    {
        public List<WorldSpace> DestinationsAsWP
        {
            get
            {
                List<WorldSpace> returnable = new List<WorldSpace>();
                if (destination.HasValue)
                {
                    returnable.Add(destination.Value);
                }
                for (int i = 0; i < paths.Count; i++)
                {
                    returnable.AddRange(paths[i].PointsOnPath);
                }
                return returnable;
            }
        }

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
                if (paths.Count == 0) return new Path(new List<WorldSpace>() { owner.FeetPosition });
                return paths[0];
            }
        }

        List<Path> paths;
        WorldSpace? destination;
        bool pathRequestInFlight;
        WorldSpace? pendingTarget;
        int overwriteToken;
        int requestToken;
        public Destination(List<WorldSpace> aDestinationList)
        {
            paths = new List<Path>();
            if (aDestinationList == null) return;
            if (aDestinationList.Count != 0) paths.Add(new Path(aDestinationList));
        }

        public void SetOwner(Entity aOwner) => owner = aOwner;

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

            if (owner.Target == null &&  destination == null)
            {
                directionToWalk = WorldSpace.Zero;
                lengthTo = 0;
                return;
            }

            if (owner.Target != null)
            {
                OverwriteDestination(owner.Target.FeetPosition);
                destination = CurrentPath.ComsumeNextPoint;
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
            if (pathRequestInFlight && pendingTarget.HasValue && pendingTarget.Value == aDestination)
            {
                return;
            }

            pathRequestInFlight = true;
            pendingTarget = aDestination;
            overwriteToken++;
            int localToken = ++requestToken;

            paths.Clear();
            destination = null;

            TileManager.RequestPath(owner.FeetPosition, aDestination, new WorldSpace(owner.FeetSize), path =>
            {
                if (localToken != requestToken) return;
                if (overwriteToken == 0) return;

                pathRequestInFlight = false;
                pendingTarget = null;

                if (path != null)
                {
                    paths.Clear();
                    paths.Add(path);
                    destination = null;
                }
                else if (DebugManager.Mode(DebugMode.TeleportStuckThings))
                {
                    owner.Teleport(aDestination);
                }
            });
        }

        public void AddDestination(WorldSpace aDestination)
        {
            Path lastPath = paths.Count > 0 ? paths[paths.Count - 1] : null;
            WorldSpace start = lastPath != null ? lastPath.CheckLastSpace : owner.FeetPosition;
            int localOverwriteToken = overwriteToken;
            TileManager.RequestPath(start, aDestination, new WorldSpace(owner.FeetSize), pathToAdd =>
            {
                if (localOverwriteToken != overwriteToken) return;
                if (pathToAdd != null)
                {
                    paths.Add(pathToAdd);
                }
            });
        }

        void UpdateDirection(WorldSpace aDestination)
        {
            WorldSpace dirV = aDestination - owner.FeetPosition;
            if (!(dirV.X == 0 && dirV.Y == 0))
            {
                lengthTo = dirV.ToVector2().Length();
                dirV.Normalize();
            }
            
            directionToWalk = dirV;
        }


    }
}
