using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Project_1.Camera;
using Project_1.Managers;
using Project_1.Messaging.Events;
using Project_1.Textures;
using Project_1.UI.UIElements.Boxes;

namespace Project_1.UI.HUD.Windows.Logic
{
    internal class NodeViewer : Box
    {
        int? currentTargetRenderId;
        LogicNodeUiSnapshot[] nodes = Array.Empty<LogicNodeUiSnapshot>();
        readonly UITexture nodeTexture;
        readonly UITexture linkTexture;
        readonly Text label;
        readonly Color textColor = Color.Black;
        const int linkThickness = 2;

        public NodeViewer(RelativeScreenPosition aPos, RelativeScreenPosition aSize) : base(new UITexture("WhiteBackground", Color.Lime), aPos, aSize)
        {
            nodeTexture = new UITexture("WhiteBackground", Color.LightGray);
            linkTexture = new UITexture("WhiteBackground", Color.Black);
            label = new Text("Gloryse");
        }

        public void SetCurrentTarget(int guildMemberRenderId)
        {
            currentTargetRenderId = guildMemberRenderId;
            nodes = Array.Empty<LogicNodeUiSnapshot>();
        }

        public void SetSnapshot(int memberRenderId, LogicNodeUiSnapshot[] snapshotNodes)
        {
            if (!currentTargetRenderId.HasValue || currentTargetRenderId.Value != memberRenderId) return;
            nodes = snapshotNodes ?? Array.Empty<LogicNodeUiSnapshot>();
        }

        public override void Draw(SpriteBatch aBatch)
        {
            base.Draw(aBatch);

            if (!currentTargetRenderId.HasValue || nodes.Length == 0) return;

            Dictionary<int, Point> positions = BuildLayout(nodes, out Dictionary<int, List<LogicNodeUiSnapshot>> levels);
            if (positions.Count == 0) return;

            GraphicsManager.CaptureScissor(this, AbsolutePos);
            DrawLinks(aBatch, positions, nodes);
            DrawNodes(aBatch, positions, levels, nodes);
            GraphicsManager.ReleaseScissor(this);
        }

        Dictionary<int, Point> BuildLayout(LogicNodeUiSnapshot[] snapshots, out Dictionary<int, List<LogicNodeUiSnapshot>> levels)
        {
            levels = new Dictionary<int, List<LogicNodeUiSnapshot>>();
            Dictionary<int, Point> positions = new Dictionary<int, Point>();
            for (int i = 0; i < snapshots.Length; i++)
            {
                LogicNodeUiSnapshot node = snapshots[i];
                if (!levels.TryGetValue(node.Depth, out List<LogicNodeUiSnapshot> list))
                {
                    list = new List<LogicNodeUiSnapshot>();
                    levels[node.Depth] = list;
                }
                list.Add(node);
            }

            if (levels.Count == 0) return positions;

            Rectangle bounds = AbsolutePos;
            int maxDepth = levels.Keys.Max();
            float xStep = bounds.Width / Math.Max(1f, maxDepth + 1.5f);

            foreach (var level in levels.ToArray())
            {
                List<LogicNodeUiSnapshot> ordered = level.Value.OrderBy(n => n.NodeId).ToList();
                levels[level.Key] = ordered;
                float yStep = bounds.Height / Math.Max(1f, ordered.Count + 1);
                for (int i = 0; i < ordered.Count; i++)
                {
                    float x = bounds.Left + xStep * (level.Key + 1);
                    float y = bounds.Top + yStep * (i + 1);
                    positions[ordered[i].NodeId] = new Point((int)x, (int)y);
                }
            }

            return positions;
        }

        void DrawLinks(SpriteBatch batch, Dictionary<int, Point> positions, LogicNodeUiSnapshot[] snapshots)
        {
            for (int i = 0; i < snapshots.Length; i++)
            {
                LogicNodeUiSnapshot node = snapshots[i];
                if (node.ParentNodeId < 0) continue;
                if (!positions.TryGetValue(node.ParentNodeId, out Point from)) continue;
                if (!positions.TryGetValue(node.NodeId, out Point to)) continue;
                DrawLink(batch, from, to);
            }
        }

        void DrawLink(SpriteBatch batch, Point from, Point to)
        {
            Rectangle horizontal = new Rectangle(
                Math.Min(from.X, to.X),
                from.Y - linkThickness / 2,
                Math.Max(1, Math.Abs(from.X - to.X)),
                linkThickness);

            Rectangle vertical = new Rectangle(
                to.X - linkThickness / 2,
                Math.Min(from.Y, to.Y),
                linkThickness,
                Math.Max(1, Math.Abs(from.Y - to.Y)));

            linkTexture.Draw(batch, horizontal, Color.Black);
            linkTexture.Draw(batch, vertical, Color.Black);
        }

        void DrawNodes(SpriteBatch batch, Dictionary<int, Point> positions, Dictionary<int, List<LogicNodeUiSnapshot>> levels, LogicNodeUiSnapshot[] snapshots)
        {
            Rectangle bounds = AbsolutePos;
            int maxDepth = levels.Count == 0 ? 1 : levels.Keys.Max() + 2;
            int maxPerLevel = levels.Count == 0 ? 1 : levels.Values.Max(l => l.Count);

            int nodeWidth = Math.Max(60, (int)(bounds.Width / Math.Max(3, maxDepth) * 0.65f));
            int nodeHeight = Math.Max(28, (int)(bounds.Height / Math.Max(maxPerLevel + 1, 3) * 0.6f));

            for (int i = 0; i < snapshots.Length; i++)
            {
                LogicNodeUiSnapshot node = snapshots[i];
                if (!positions.TryGetValue(node.NodeId, out Point centre)) continue;
                Rectangle rect = new Rectangle(centre.X - nodeWidth / 2, centre.Y - nodeHeight / 2, nodeWidth, nodeHeight);
                nodeTexture.Draw(batch, rect, GetNodeColor(node.ResultName));

                label.Color = textColor;
                label.Value = node.ResultName;
                label.CentredDraw(batch, new AbsoluteScreenPosition(rect.Center));
            }
        }

        static Color GetNodeColor(string resultName)
        {
            if (string.IsNullOrWhiteSpace(resultName) || resultName == "Branch") return Color.DarkSlateGray;
            if (resultName.Contains("Heal")) return Color.OrangeRed;
            if (resultName.Contains("Spell")) return Color.MediumPurple;
            if (resultName.Contains("Move")) return Color.SteelBlue;
            if (resultName.Contains("Attack")) return Color.LightGreen;
            if (resultName.Contains("Target")) return Color.SkyBlue;
            if (resultName.Contains("Idle")) return Color.LightGray;

            return Color.DarkSlateBlue;
        }
    }
}
