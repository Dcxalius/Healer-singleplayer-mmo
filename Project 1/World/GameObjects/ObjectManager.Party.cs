using System.Diagnostics;
using System.Linq;
using Project_1.Camera;
using Project_1.GameObjects.Entities.Friendlies.GuildMembers;
using Project_1.Tiles;
using Project_1.Managers;
using System;

namespace Project_1.GameObjects
{
    internal static partial class ObjectManager
    {
        public static GuildMember FriendlyTargetCycle()
        {
            ThreadAffinity.AssertSimThread();

            if (player.Target == null) return GetClosestGuildMember();
            if (player.Target.GetType() != typeof(GuildMember)) return GetClosestGuildMember();
            return entities.Where(x => x.DistanceTo(player.FeetPosition) > player.Target.DistanceTo(player.FeetPosition)).MinBy(x => x.DistanceTo(player.FeetPosition)) as GuildMember;
        }

        public static bool SpawnGuildMemberToParty(GuildMember aMember, WorldSpace? aPosition)
        {
            ThreadAffinity.AssertSimThread();
            Debug.Assert(guild.Contains(aMember));
            Debug.Assert(!player.Party.IsInParty(aMember));

            WorldSpace position = aPosition ?? FindTileAroundPlayer();
            aMember.Teleport(position);
            entities.Add(guild.Find(member => member == aMember));
            return player.Party.AddToParty(entities[entities.Count - 1] as GuildMember);
        }

        public static bool RemoveGuildMemberFromParty(GuildMember aMember)
        {
            ThreadAffinity.AssertSimThread();
            Debug.Assert(guild.Contains(aMember));
            Debug.Assert(player.Party.IsInParty(aMember));

            WorldSpace position = FindTileAroundPlayer();
            aMember.RecieveDirectWalkingOrder(position);
            return player.Party.RemoveFromParty(aMember);
        }

        static WorldSpace FindTileAroundPlayer()
        {
            Tile[] tiles = Array.Empty<Tile>();
            float start = DistanceOfCircleAroundPlayer;
            const float step = 50;
            while (tiles.Length == 0)
            {
                tiles = TileManager.GetTilesAroundPosition(Player.FeetPosition, start);
                start -= step;
                Debug.Assert(start > 0);
            }

            Tile tile = tiles[RandomManager.RollInt(tiles.Length)];
            return tile.Position;
        }
    }
}
