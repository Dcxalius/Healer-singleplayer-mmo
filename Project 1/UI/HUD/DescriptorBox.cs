using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Project_1.Camera;
using Project_1.Input;
using Project_1.Managers;
using Project_1.Messaging.Events;
using Project_1.Textures;
using Project_1.UI.UIElements;
using Project_1.UI.UIElements.Boxes;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO.Packaging;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Project_1.UI.HUD
{
    internal class DescriptorBox : Box
    {
        readonly struct DescriptorLine
        {
            public DescriptorLine(Label aLabel, string aText)
            {
                Label = aLabel;
                Text = aText;
                Size = Textures.Text.CalculateOffset(aText, "Comfortaa-msdf");
            }

            public Label Label { get; }
            public string Text { get; }
            public Vector2 Size { get; }
        }

        Label itemName;
        Label itemDescription;
        Label itemStats;
        Label itemBonusStats;
        Label itemSellPrice;

        SquareImage goldImage;

        float xMax;
        readonly RelativeScreenPosition spacingFromItem = RelativeScreenPosition.GetSquareFromX(0.0005f);
        RelativeScreenPosition spacingInWorld;


        public DescriptorBox(UIElement aParent) : base(aParent, new UITexture("GrayBackground", Color.White), RelativeScreenPosition.Zero, RelativeScreenPosition.One)
        {
            xMax = 0.15f;

            itemName = new Label(this, RelativeScreenPosition.Zero, RelativeScreenPosition.Zero, Label.TextAllignment.TopLeft);
            itemDescription = new Label(this, RelativeScreenPosition.Zero, RelativeScreenPosition.Zero, Label.TextAllignment.TopLeft);
            itemStats = new Label(this, RelativeScreenPosition.Zero, RelativeScreenPosition.Zero, Label.TextAllignment.TopLeft);
            itemBonusStats = new Label(this, RelativeScreenPosition.Zero, RelativeScreenPosition.Zero, Label.TextAllignment.TopLeft, Color.LimeGreen);
            itemSellPrice = new Label(this, RelativeScreenPosition.Zero, RelativeScreenPosition.Zero, Label.TextAllignment.CentreRight);


            goldImage = new SquareImage(this, new UITexture("Gold", Color.White), new RelativeScreenPosition(0, 0), new RelativeScreenPosition(0f, 0.1f));

            Visible = false;
            AlwaysFullyOnScreen = true;
        }

        public void SetToSnapshot(in ItemDescriptorSnapshot snapshot, RelativeScreenPosition aPos)
        {
            Visible = true;
            spacingInWorld = RelativeScreenPosition.GetSquareFromX(0.005f);

            List<DescriptorLine> lines = new List<DescriptorLine>();
            AddLine(lines, itemName, snapshot.Name);
            AddLine(lines, itemDescription, snapshot.Description);
            if (snapshot.HasStatReport) AddLine(lines, itemStats, snapshot.StatReport);
            else itemStats.Text = null;
            if (snapshot.HasBonusStatReport) AddLine(lines, itemBonusStats, snapshot.BonusStatReport);
            else itemBonusStats.Text = null;

            LayoutDescriptor(lines, snapshot.HasSellPrice, snapshot.SellPrice.ToString(CultureInfo.InvariantCulture), aPos);
        }

        public void SetToSnapshot(in SpellDescriptorSnapshot snapshot, RelativeScreenPosition aPos)
        {
            Visible = true;
            spacingInWorld = RelativeScreenPosition.GetSquareFromX(0.005f);

            List<DescriptorLine> lines = new List<DescriptorLine>();
            AddLine(lines, itemName, snapshot.Name);
            AddLine(lines, itemDescription, snapshot.Description);
            if (snapshot.HasStatReport) AddLine(lines, itemStats, snapshot.StatReport);
            else itemStats.Text = null;

            itemBonusStats.Text = null;
            LayoutDescriptor(lines, false, null, aPos);
        }

        public void Clear()
        {
            ResetDescriptor();
        }

        void AddLine(List<DescriptorLine> lines, Label label, string text)
        {
            if (string.IsNullOrWhiteSpace(text))
            {
                label.Text = null;
                return;
            }

            int maxTextWidth = Math.Max(1, (int)(Camera.Camera.ScreenRectangle.Width * xMax));
            string wrapped = WrapText(text, maxTextWidth);
            label.Text = wrapped;
            lines.Add(new DescriptorLine(label, wrapped));
        }

        void LayoutDescriptor(List<DescriptorLine> lines, bool hasSellPrice, string sellPriceText, RelativeScreenPosition aPos)
        {
            int spacingPx = Math.Max(1, spacingInWorld.ToAbsoluteScreenPos().X);
            int maxWidth = 1;
            int height = spacingPx;

            for (int i = 0; i < lines.Count; i++)
            {
                maxWidth = Math.Max(maxWidth, (int)MathF.Ceiling(lines[i].Size.X));
                height += (int)MathF.Ceiling(lines[i].Size.Y) + spacingPx;
            }

            itemSellPrice.Text = null;
            goldImage.Visible = false;
            Vector2 sellPriceSize = Vector2.Zero;
            int goldIconSize = 0;
            if (hasSellPrice)
            {
                itemSellPrice.Text = sellPriceText;
                goldImage.Visible = true;
                sellPriceSize = itemSellPrice.UnderlyingTextOffset;
                goldIconSize = Math.Max(1, (int)MathF.Ceiling(sellPriceSize.Y));
                maxWidth = Math.Max(maxWidth, (int)MathF.Ceiling(sellPriceSize.X) + spacingPx + goldIconSize);
                height += goldIconSize + spacingPx;
            }

            int width = maxWidth + spacingPx * 2;
            height = Math.Max(1, height);
            Resize(new AbsoluteScreenPosition(width, height));
            Move(aPos - spacingFromItem - RelativeSize);

            RelativeScreenPosition spacingInBox = new AbsoluteScreenPosition(spacingPx).ToRelativeScreenPosition(Size);
            RelativeScreenPosition pos = spacingInBox;

            for (int i = 0; i < lines.Count; i++)
            {
                DescriptorLine line = lines[i];
                line.Label.Resize(new AbsoluteScreenPosition(maxWidth, (int)MathF.Ceiling(line.Size.Y)).ToRelativeScreenPosition(Size));
                line.Label.Move(pos);
                pos += line.Label.RelativeSize.OnlyY + spacingInBox.OnlyY;
            }

            if (!hasSellPrice) return;

            goldImage.Resize(new AbsoluteScreenPosition(goldIconSize).ToRelativeScreenPosition(Size).OnlyY);
            itemSellPrice.Resize(new AbsoluteScreenPosition(maxWidth - goldIconSize - spacingPx, goldIconSize).ToRelativeScreenPosition(Size));
            itemSellPrice.Move(pos);
            goldImage.Move(new AbsoluteScreenPosition(width - spacingPx - goldIconSize, pos.ToAbsoluteScreenPos(Size).Y).ToRelativeScreenPosition(Size));
        }

        static string WrapText(string text, int maxWidth)
        {
            if (string.IsNullOrEmpty(text)) return text;

            string[] sourceLines = text.Replace("\r\n", "\n").Split('\n');
            StringBuilder builder = new StringBuilder();

            for (int i = 0; i < sourceLines.Length; i++)
            {
                if (i > 0) builder.Append('\n');
                builder.Append(WrapSingleLine(sourceLines[i], maxWidth));
            }

            return builder.ToString();
        }

        static string WrapSingleLine(string line, int maxWidth)
        {
            if (string.IsNullOrWhiteSpace(line)) return string.Empty;

            string[] words = line.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
            StringBuilder builder = new StringBuilder();
            string current = string.Empty;

            for (int i = 0; i < words.Length; i++)
            {
                string word = words[i];
                if (FitsLine(word, maxWidth))
                {
                    string candidate = string.IsNullOrEmpty(current) ? word : current + " " + word;
                    if (FitsLine(candidate, maxWidth))
                    {
                        current = candidate;
                        continue;
                    }

                    AppendWrappedLine(builder, ref current);
                    current = word;
                    continue;
                }

                AppendWrappedLine(builder, ref current);
                AppendLongWord(builder, word, maxWidth);
            }

            AppendWrappedLine(builder, ref current);
            return builder.ToString();
        }

        static void AppendWrappedLine(StringBuilder builder, ref string current)
        {
            if (string.IsNullOrEmpty(current)) return;
            if (builder.Length > 0) builder.Append('\n');
            builder.Append(current);
            current = string.Empty;
        }

        static void AppendLongWord(StringBuilder builder, string word, int maxWidth)
        {
            int start = 0;
            while (start < word.Length)
            {
                int count = 1;
                while (start + count < word.Length && FitsLine(word.Substring(start, count + 1), maxWidth))
                {
                    count++;
                }

                if (builder.Length > 0) builder.Append('\n');
                builder.Append(word.Substring(start, count));
                start += count;
            }
        }

        static bool FitsLine(string text, int maxWidth)
        {
            return Textures.Text.CalculateOffset(text, "Comfortaa-msdf").X <= maxWidth;
        }

        

        void ResetDescriptor()
        {
            Visible = false;
            itemName.Text = null;
            itemDescription.Text = null;
            itemStats.Text = null;
            itemBonusStats.Text = null;
            itemSellPrice.Text = null;
            Resize(RelativeScreenPosition.Zero);
            return;
        }
    }
}
