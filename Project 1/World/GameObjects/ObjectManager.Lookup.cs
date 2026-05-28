using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Project_1.Camera;
using Project_1.GameObjects.Entities;
using Project_1.GameObjects.Entities.Friendlies;
using Project_1.GameObjects.Entities.Friendlies.GuildMembers;
using Project_1.Managers;

namespace Project_1.GameObjects
{
    internal static partial class ObjectManager
    {
        public static GuildMember[] GetGuildMembers()
        {
            ThreadAffinity.AssertSimThread();
            if (guild == null || guild.Count == 0) return Array.Empty<GuildMember>();
            return guild.ToArray();
        }

        public static Entity[] GetEntitiesSnapshot()
        {
            ThreadAffinity.AssertSimThread();
            if (entities == null || entities.Count == 0) return Array.Empty<Entity>();
            return entities.ToArray();
        }

        public static Entity[] GetAllEntitiesSnapshot()
        {
            ThreadAffinity.AssertSimThread();
            List<Entity> all = BuildAllScratch();
            if (all == null || all.Count == 0) return Array.Empty<Entity>();
            return all.ToArray();
        }

        public static bool TryGetEntityByRenderId(int renderId, out Entity entity)
        {
            //TODO: Check if there is a cleaner way to fingure this out
            ThreadAffinity.AssertSimThread();
            entity = null;
            if (renderId <= 0) return false;

            if (player != null && player.RenderId == renderId)
            {
                entity = player;
                return true;
            }

            for (int i = 0; i < entities.Count; i++)
            {
                if (entities[i].RenderId != renderId) continue;
                entity = entities[i];
                return true;
            }

            for (int i = 0; i < guild.Count; i++)
            {
                if (guild[i].RenderId != renderId) continue;
                entity = guild[i];
                return true;
            }

            for (int i = 0; i < npcs.Count; i++)
            {
                if (npcs[i].RenderId != renderId) continue;
                entity = npcs[i];
                return true;
            }
            //TODO: If we are searching for something that doesn't exists we should probably be logging that

            return false;
        }

        public static bool TryGetGuildMemberByRenderId(int renderId, out GuildMember member)
        {
            ThreadAffinity.AssertSimThread();
            member = null;
            if (renderId <= 0) return false;

            for (int i = 0; i < guild.Count; i++)
            {
                if (guild[i].RenderId != renderId) continue;
                member = guild[i];
                return true;
            }
            //TODO: If we are searching for something that doesn't exists we should probably be logging that

            return false;
        }

        public static bool TryGetFriendlyByRenderId(int renderId, out Friendly friendly)
        {
            ThreadAffinity.AssertSimThread();
            friendly = null;
            if (renderId <= 0) return false;

            if (player != null && player.RenderId == renderId)
            {
                friendly = player;
                return true;
            }

            for (int i = 0; i < guild.Count; i++)
            {
                if (guild[i].RenderId != renderId) continue;
                friendly = guild[i];
                return true;
            }

            for (int i = 0; i < npcs.Count; i++)
            {
                if (npcs[i].RenderId != renderId) continue;
                friendly = npcs[i];
                return true;
            }

            //TODO: If we are searching for something that doesn't exists we should probably be logging that
            return false;
        }

        public static bool TryGetFriendlyByName(string name, out Friendly friendly)
        {
            //TODO: Should be depricated. Double get. Name could be an unsafe getter due to duplicates. If by name is desired, then a way to detect and handle duplicates are needed. At least a Name => id and using the above GetFriendlyByRenderId should be used
            ThreadAffinity.AssertSimThread();
            friendly = null;
            if (string.IsNullOrWhiteSpace(name)) return false;

            if (player != null && string.Equals(player.Name, name, StringComparison.OrdinalIgnoreCase))
            {
                friendly = player;
                return true;
            }

            for (int i = 0; i < guild.Count; i++)
            {
                if (!string.Equals(guild[i].Name, name, StringComparison.OrdinalIgnoreCase)) continue;
                friendly = guild[i];
                return true;
            }

            for (int i = 0; i < npcs.Count; i++)
            {
                if (!string.Equals(npcs[i].Name, name, StringComparison.OrdinalIgnoreCase)) continue;
                friendly = npcs[i];
                return true;
            }

            return false;
        }

        static GuildMember GetClosestGuildMember() => entities.MinBy(x => x.DistanceTo(player.FeetPosition)) as GuildMember; //TODO: This should be changed to only look through the party members on screen. This should eventually be developed to allow for use of tab targeting through this list
        static GuildMember[] GuildMembersInWorld => guild.Where(x => entities.Contains(x)).ToArray();

        public static bool TryGetEntityAt(WorldSpace worldPos, out Entity entity)
        {
            //TODO: Change to the modern 3d system.
            //TODO: Should search through a build list of rectangles and return rather than live objects
            ThreadAffinity.AssertSimThread();
            entity = null;
            if (player != null && player.WorldRectangle.Contains(worldPos.ToPoint()))
            {
                entity = player;
                return true;
            }

            for (int i = 0; i < entities.Count; i++)
            {
                if (!entities[i].WorldRectangle.Contains(worldPos.ToPoint())) continue;
                entity = entities[i];
                return true;
            }

            for (int i = 0; i < npcs.Count; i++)
            {
                if (!npcs[i].WorldRectangle.Contains(worldPos.ToPoint())) continue;
                entity = npcs[i];
                return true;
            }

            return false;
        }
    }
}
