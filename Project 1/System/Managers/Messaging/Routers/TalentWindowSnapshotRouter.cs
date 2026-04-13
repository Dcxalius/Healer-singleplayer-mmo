using System;
using System.Linq;
using Project_1.GameObjects.Entities;
using Project_1.GameObjects.Entities.Friendlies;
using Project_1.GameObjects.Entities.Friendlies.Players;
using Project_1.Managers;
using Project_1.Messaging;
using Project_1.Messaging.Events;
using Project_1.Textures;
using Project_1.World.GameObjects.Unit.Talents;

namespace Project_1.GameObjects.Entities.Friendlies.GuildMembers
{
    internal static class TalentWindowSnapshotRouter
    {
        const int ExpectedTreesPerClass = 3;
        static bool initialized;

        public static void Init()
        {
            ThreadAffinity.AssertMainThread();
            if (initialized) return;
            initialized = true;

            MailboxManager.RegisterSimCommandType<TalentWindowSnapshotRequested>();
            MailboxManager.RegisterSimCommandType<TalentLearnRequested>();
            MailboxManager.Sim.Subscribe<TalentWindowSnapshotRequested>(HandleTalentWindowSnapshotRequested);
            MailboxManager.Sim.Subscribe<TalentLearnRequested>(HandleTalentLearnRequested);
        }

        static void HandleTalentWindowSnapshotRequested(TalentWindowSnapshotRequested e)
        {
            ThreadAffinity.AssertSimThread();

            Entity owner = ResolveOwner(e.MemberRenderId);
            if (owner == null) return;

            MailboxManager.PublishUiEvent(new TalentWindowSet(BuildSnapshot(owner)));
        }

        static void HandleTalentLearnRequested(TalentLearnRequested e)
        {
            ThreadAffinity.AssertSimThread();
            Player player = ObjectManager.Player;
            if (player == null) return;
            if (!player.TryLearnTalent(e.TalentId)) return;

            MailboxManager.PublishUiEvent(new TalentWindowSet(BuildSnapshot(player)));
        }

        static Entity ResolveOwner(int? memberRenderId)
        {
            if (!memberRenderId.HasValue)
            {
                return ObjectManager.Player;
            }

            if (ObjectManager.TryGetGuildMemberByRenderId(memberRenderId.Value, out var member))
            {
                return member;
            }

            return null;
        }

        static TalentWindowSnapshot BuildSnapshot(Entity owner)
        {
            TalentTree[] sourceTrees = owner.ClassData?.TalentTrees ?? Array.Empty<TalentTree>();
            TalentTreeUiSnapshot[] trees = new TalentTreeUiSnapshot[ExpectedTreesPerClass];
            int remainingTalentPoints = owner is Friendly friendly ? friendly.RemainingTalentPoints : 0;

            for (int i = 0; i < trees.Length; i++)
            {
                trees[i] = i < sourceTrees.Length && sourceTrees[i] != null
                    ? BuildTreeSnapshot(owner, sourceTrees[i])
                    : BuildEmptyTreeSnapshot(i);
            }

            return new TalentWindowSnapshot(owner.BuildUiSnapshot(), trees, remainingTalentPoints);
        }

        static TalentTreeUiSnapshot BuildTreeSnapshot(Entity owner, TalentTree tree)
        {
            TalentUiSnapshot[][] rows = tree.Talents?
                .Select(row => row?
                    .Select(talent => new TalentUiSnapshot(
                        talent.Id,
                        talent.Name,
                        talent.Description,
                        talent.GfxPath,
                        owner.GetTalentRank(talent.Id),
                        talent.MaxRank))
                    .ToArray() ?? Array.Empty<TalentUiSnapshot>())
                .ToArray()
                ?? Array.Empty<TalentUiSnapshot[]>();

            int spentPoints = tree.GetIds.Sum(owner.GetTalentRank);
            return new TalentTreeUiSnapshot(tree.Name, tree.Background, rows, spentPoints);
        }

        static TalentTreeUiSnapshot BuildEmptyTreeSnapshot(int index)
        {
            return new TalentTreeUiSnapshot($"Tree {index + 1}", GfxPath.NullPath, Array.Empty<TalentUiSnapshot[]>(), 0);
        }
    }
}
