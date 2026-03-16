using Project_1.Camera;
using Project_1.Managers;
using Project_1.Tiles;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Project_1.GameObjects.Entities.Friendlies.Players;

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

        public bool HasDestination => destination.HasValue || paths.Count > 0 || pendingAddRequests > 0 || pathRequestInFlight;
        public WorldSpace DirectionToWalk => directionToWalk;
        public float LengthTo => lengthTo;
        float lengthTo; 
        
        WorldSpace directionToWalk;
        
        Entity owner;
        private void CheckIfClear()
        {
            if (paths.Count == 0) return;

            if (paths[0].Count == 0) paths.RemoveAt(0);
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
        bool pathRequestInFlight;
        int pendingAddRequests;
        Entity lastTarget;
        WorldSpace lastTargetPosition;
        WorldSpace? pendingTarget;
        int overwriteToken;
        int requestToken;
        bool hasPendingOverwrite;
        Path pendingOverwritePath;
        public Destination(List<WorldSpace> aDestinationList)
        {
            paths = new List<Path>();
            if (aDestinationList == null) return;
            if (aDestinationList.Count != 0) paths.Add(new Path(aDestinationList));
        }

        public void SetOwner(Entity aOwner) => owner = aOwner;

        public void Update()
        {
            ThreadAffinity.AssertSimThread();
            
            if (owner is Player)
            {
                if (!(owner as Player).LockedMovement) return;
            }

            if (hasPendingOverwrite)
            {
                paths.Clear();
                paths.Add(pendingOverwritePath);
                destination = null;
                pendingOverwritePath = null;
                hasPendingOverwrite = false;
            }
            if (!HasDestination) destination = null;
            if (owner.Target == null && CurrentPath != null && destination == null)
            {
                if (CurrentPath.Count == 0)
                {
                    CheckIfClear();
                    if (CurrentPath == null)
                    {
                        directionToWalk = WorldSpace.Zero;
                        lengthTo = 0;
                        return;
                    }
                }
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
                WorldSpace targetPos = owner.Target.FeetPosition;
                if (lastTarget != owner.Target)
                {
                    lastTarget = owner.Target;
                    lastTargetPosition = targetPos;
                    OverwriteDestination(targetPos);
                }
                else if (targetPos != lastTargetPosition)
                {
                    lastTargetPosition = targetPos;
                    OverwriteDestination(targetPos);
                }

                if (destination == null && CurrentPath != null)
                {
                    if (CurrentPath.Count == 0)
                    {
                        CheckIfClear();
                        if (CurrentPath == null)
                        {
                            directionToWalk = WorldSpace.Zero;
                            lengthTo = 0;
                            return;
                        }
                    }
                    destination = CurrentPath.ComsumeNextPoint;
                }
                if (destination == null)
                {
                    directionToWalk = WorldSpace.Zero;
                    lengthTo = 0;
                    return;
                }
            }
            else
            {
                lastTarget = null;
            }

            UpdateDirection(destination.Value);
        }

        public WorldSpace GetVelocity(float aAttackRange, float aSpeed, WorldSpace aSize)
        {
            ThreadAffinity.AssertSimThread();
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
                float distanceToTarget = (owner.Target.FeetPosition - owner.FeetPosition).ToVector2().Length();
                if (distanceToTarget < aAttackRange - owner.Target.Size.X / 2 - owner.Size.X / 2)
                {
                    return WorldSpace.Zero;
                }
            }

            return DirectionToWalk * aSpeed * (float)TimeManager.SecondsSinceLastFrame;
        }


        public void OverwriteDestination(WorldSpace aDestination)
        {
            ThreadAffinity.AssertSimThread();
            if (pathRequestInFlight && pendingTarget.HasValue && pendingTarget.Value == aDestination)
            {
                return;
            }

            pathRequestInFlight = true;
            pendingTarget = aDestination;
            overwriteToken++;
            int localToken = ++requestToken;

            TileManager.RequestPath(owner.FeetPosition, aDestination, new WorldSpace(owner.FeetSize), path =>
            {
                if (localToken != requestToken) return;
                if (overwriteToken == 0) return;

                pathRequestInFlight = false;
                pendingTarget = null;

                if (path != null)
                {
                    pendingOverwritePath = path;
                    hasPendingOverwrite = true;
                }
                else if (DebugManager.Mode(DebugMode.TeleportStuckThings))
                {
                    owner.Teleport(aDestination);
                }
            });
        }

        public void AddDestination(WorldSpace aDestination)
        {
            ThreadAffinity.AssertSimThread();
            Path lastPath = null;
            for (int i = paths.Count - 1; i >= 0; i--)
            {
                if (paths[i].Count > 0)
                {
                    lastPath = paths[i];
                    break;
                }
            }
            WorldSpace start = lastPath != null ? lastPath.CheckLastSpace : owner.FeetPosition;
            int localOverwriteToken = overwriteToken;
            pendingAddRequests++;
            TileManager.RequestPath(start, aDestination, new WorldSpace(owner.FeetSize), pathToAdd =>
            {
                if (pendingAddRequests > 0) pendingAddRequests--;
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
