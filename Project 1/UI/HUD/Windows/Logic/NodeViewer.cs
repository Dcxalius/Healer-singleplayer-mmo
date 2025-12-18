using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Project_1.Camera;
using Project_1.GameObjects.Entities.GuildMembers;
using Project_1.Managers;
using Project_1.Textures;
using Project_1.UI.UIElements.Boxes;
using System.Collections.Generic;
using System.Linq;

namespace Project_1.UI.HUD.Windows.Logic
{
    internal class NodeViewer : Box
    {
        GuildMember currentTarget;
        readonly UITexture nodeTexture;
        readonly UITexture linkTexture;
        readonly Text label;
        readonly Color textColor = Color.Black;
        const int linkThickness = 2;

        public NodeViewer() : base(new UITexture("WhiteBackground", Color.Lime), new RelativeScreenPosition(0.01f, 0.01f), new RelativeScreenPosition(0.48f, 0.48f))
        {
            nodeTexture = new UITexture("WhiteBackground", Color.LightGray);
            linkTexture = new UITexture("WhiteBackground", Color.Black);
            label = new Text("Gloryse");
        }

        public void SetCurrentTarget(GuildMember aGuildMember)
        {
            currentTarget = aGuildMember;
        }

        public override void Draw(SpriteBatch aBatch)
        {
            base.Draw(aBatch);

            LogicNode root = currentTarget?.AttackLogic?.RootNode;
            if (root == null) return;

            var positions = BuildLayout(root, out var levels);
            if (positions.Count == 0) return;

            GraphicsManager.CaptureScissor(this, AbsolutePos);
            DrawLinks(aBatch, positions);
            DrawNodes(aBatch, positions, levels);
            GraphicsManager.ReleaseScissor(this);
        }

        Dictionary<LogicNode, Point> BuildLayout(LogicNode root, out Dictionary<int, List<LogicNode>> levels)
        {
            levels = new Dictionary<int, List<LogicNode>>();
            var positions = new Dictionary<LogicNode, Point>();
            var queue = new Queue<(LogicNode node, int depth)>();
            var visited = new HashSet<LogicNode>();
            queue.Enqueue((root, 0));

            while (queue.Count > 0)
            {
                var (node, depth) = queue.Dequeue();
                if (!levels.TryGetValue(depth, out var list))
                {
                    list = new List<LogicNode>();
                    levels[depth] = list;
                }
                if (!list.Contains(node))
                {
                    list.Add(node);
                }
                if (!visited.Add(node)) continue;

                foreach (var child in node.Children)
                {
                    queue.Enqueue((child, depth + 1));
                }
            }

            Rectangle bounds = AbsolutePos;
            int maxDepth = levels.Keys.Count == 0 ? 0 : levels.Keys.Max();
            float xStep = bounds.Width / Math.Max(1f, maxDepth + 1.5f);

            foreach (var level in levels)
            {
                float yStep = bounds.Height / Math.Max(1f, level.Value.Count + 1);
                for (int i = 0; i < level.Value.Count; i++)
                {
                    float x = bounds.Left + xStep * (level.Key + 1);
                    float y = bounds.Top + yStep * (i + 1);
                    positions[level.Value[i]] = new Point((int)x, (int)y);
                }
            }

            return positions;
        }

        void DrawLinks(SpriteBatch batch, Dictionary<LogicNode, Point> layout)
        {
            foreach (var node in layout.Keys)
            {
                foreach (var child in node.Children)
                {
                    if (!layout.TryGetValue(child, out var childPos)) continue;
                    DrawLink(batch, layout[node], childPos);
                }
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

        void DrawNodes(SpriteBatch batch, Dictionary<LogicNode, Point> layout, Dictionary<int, List<LogicNode>> levels)
        {
            Rectangle bounds = AbsolutePos;
            int maxDepth = levels.Keys.Count == 0 ? 1 : levels.Keys.Max() + 2;
            int maxPerLevel = levels.Values.Count == 0 ? 1 : levels.Values.Max(l => l.Count);

            int nodeWidth = Math.Max(60, (int)(bounds.Width / Math.Max(3, maxDepth) * 0.65f));
            int nodeHeight = Math.Max(28, (int)(bounds.Height / Math.Max(maxPerLevel + 1, 3) * 0.6f));

            foreach (var kvp in layout)
            {
                Point centre = kvp.Value;
                Rectangle rect = new Rectangle(centre.X - nodeWidth / 2, centre.Y - nodeHeight / 2, nodeWidth, nodeHeight);
                nodeTexture.Draw(batch, rect, GetNodeColor(kvp.Key));

                label.Color = textColor;
                label.Value = GetLabel(kvp.Key);
                label.CentredDraw(batch, new AbsoluteScreenPosition(rect.Center));
            }
        }

        static string GetLabel(LogicNode node)
        {
            var raw = node.RawResult;
            if (raw == null || raw is Continue) return "Branch";
            string name = raw.GetType().Name;
            const string suffix = "Result";
            if (name.EndsWith(suffix))
            {
                name = name.Substring(0, name.Length - suffix.Length);
            }
            return name;
        }

        static Color GetNodeColor(LogicNode node)
        {
            var result = node.RawResult;
            if (result == null || result is Continue) return Color.DarkSlateGray;

            string name = result.GetType().Name;
            if (name.Contains("Heal")) return Color.OrangeRed;
            if (name.Contains("Spell")) return Color.MediumPurple;
            if (name.Contains("Move")) return Color.SteelBlue;
            if (name.Contains("Attack")) return Color.LightGreen;
            if (name.Contains("Target")) return Color.SkyBlue;
            if (name.Contains("Idle")) return Color.LightGray;

            return Color.DarkSlateBlue;
        }
    }
}
