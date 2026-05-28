using System;
using System.Collections.Generic;
using Project_1.Managers;
using Project_1.Messaging;
using Project_1.Messaging.Events;
using Project_1.UI.HUD.Windows.Logic;

namespace Project_1.GameObjects.Entities.Friendlies.GuildMembers
{
    internal static class LogicWindowSnapshotRouter
    {
        static bool initialized;

        public static void Init()
        {
            //TODO: Move these closer to their respective systems
            ThreadAffinity.AssertMainThread();
            if (initialized) return;
            initialized = true;

            MailboxManager.RegisterSimCommandType<LogicWindowSnapshotRequested>();
            MailboxManager.Sim.Subscribe<LogicWindowSnapshotRequested>(HandleLogicWindowSnapshotRequested);
        }

        static void HandleLogicWindowSnapshotRequested(LogicWindowSnapshotRequested e)
        {
            ThreadAffinity.AssertSimThread();
            if (!ObjectManager.TryGetGuildMemberByRenderId(e.MemberRenderId, out GuildMember member))
            {
                MailboxManager.PublishUiEvent(new LogicWindowSnapshotSet(e.MemberRenderId, Array.Empty<LogicNodeUiSnapshot>()));
                return;
            }

            LogicNode root = member.AttackLogic?.RootNode;
            MailboxManager.PublishUiEvent(new LogicWindowSnapshotSet(e.MemberRenderId, BuildLogicNodeSnapshot(root)));
        }

        static LogicNodeUiSnapshot[] BuildLogicNodeSnapshot(LogicNode root)
        {
            if (root == null) return Array.Empty<LogicNodeUiSnapshot>();

            Dictionary<LogicNode, int> ids = new Dictionary<LogicNode, int>();
            List<LogicNodeUiSnapshot> snapshots = new List<LogicNodeUiSnapshot>();
            Queue<(LogicNode node, int parentId, int depth)> queue = new Queue<(LogicNode node, int parentId, int depth)>();
            queue.Enqueue((root, -1, 0));

            while (queue.Count > 0)
            {
                (LogicNode node, int parentId, int depth) = queue.Dequeue();
                if (node == null) continue;

                if (!ids.TryGetValue(node, out int nodeId))
                {
                    nodeId = ids.Count;
                    ids[node] = nodeId;
                    string resultName = GetLogicNodeName(node);
                    snapshots.Add(new LogicNodeUiSnapshot(nodeId, parentId, depth, resultName));

                    IReadOnlyList<LogicNode> children = node.Children;
                    for (int i = 0; i < children.Count; i++)
                    {
                        queue.Enqueue((children[i], nodeId, depth + 1));
                    }
                    continue;
                }

                if (parentId >= 0)
                {
                    snapshots.Add(new LogicNodeUiSnapshot(nodeId, parentId, depth, GetLogicNodeName(node)));
                }
            }

            return snapshots.ToArray();
        }

        static string GetLogicNodeName(LogicNode node)
        {
            ILogicResult raw = node.RawResult;
            if (raw == null || raw is Continue) return "Branch";
            string name = raw.GetType().Name;
            const string suffix = "Result";
            if (name.EndsWith(suffix))
            {
                name = name.Substring(0, name.Length - suffix.Length);
            }
            return name;
        }
    }
}
