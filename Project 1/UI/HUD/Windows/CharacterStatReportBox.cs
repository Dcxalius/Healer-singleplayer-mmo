using Microsoft.Xna.Framework;
using Project_1.Camera;
using Project_1.GameObjects;
using Project_1.GameObjects.Unit.Classes;
using Project_1.Messaging;
using Project_1.Messaging.Events;
using Project_1.Textures;
using Project_1.UI.UIElements;
using Project_1.UI.UIElements.Boxes;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;

namespace Project_1.UI.HUD.Windows
{
    internal sealed class CharacterStatReportBox : PageBox
    {
        sealed class StatLineElement : UIElement
        {
            readonly Label numberLabel;
            readonly Label textLabel;
            ItemDescriptorSnapshot hoverSnapshot;
            bool hasHoverSnapshot;

            public StatLineElement(RelativeScreenPosition aPos, RelativeScreenPosition aSize, UIElement aParent = null) : base(aParent, null, aPos, aSize)
            {
                numberLabel = new Label(this, new RelativeScreenPosition(0f, 0f), new RelativeScreenPosition(0.45f, 1f), Label.TextAllignment.CentreRight, Color.Black);
                textLabel = new Label(this, new RelativeScreenPosition(0.5f, 0f), new RelativeScreenPosition(0.5f, 1f), Label.TextAllignment.CentreLeft, Color.Black);
                AddChild(numberLabel);
                AddChild(textLabel);

                Visible = false;
                CapturesClick = false;
                CapturesRelease = false;
                CapturesScroll = false;
            }

            public void Set(string aNumber, string aText, ItemDescriptorSnapshot aHoverSnapshot, bool aHasHoverSnapshot)
            {
                numberLabel.Text = aNumber;
                textLabel.Text = aText;
                hoverSnapshot = aHoverSnapshot;
                if (!aHasHoverSnapshot && hasHoverSnapshot && isHovered)
                {
                    MailboxManager.PublishUiEvent(new DescriptorBoxClear());
                }

                hasHoverSnapshot = aHasHoverSnapshot;
            }

            public void Clear()
            {
                if (hasHoverSnapshot && isHovered)
                {
                    MailboxManager.PublishUiEvent(new DescriptorBoxClear());
                }

                numberLabel.Text = null;
                textLabel.Text = null;
                hoverSnapshot = default;
                hasHoverSnapshot = false;
            }

            protected override void OnHover()
            {
                base.OnHover();
                if (!Visible || !hasHoverSnapshot) return;
                MailboxManager.PublishUiEvent(new DescriptorBoxSet(hoverSnapshot));
            }

            protected override void OnDeHover()
            {
                base.OnDeHover();
                if (!hasHoverSnapshot) return;
                MailboxManager.PublishUiEvent(new DescriptorBoxClear());
            }
        }

        readonly struct StatPage
        {
            public StatPage(string aTitle, List<StatLineSnapshot> aLines)
            {
                Title = aTitle;
                Lines = aLines ?? new List<StatLineSnapshot>();
            }

            public string Title { get; }
            public List<StatLineSnapshot> Lines { get; }
        }

        const int StatRowsPerPage = 5;
        const float StatPageTopPadding = 0.16f;
        static readonly Point StatPageSize = new Point(1, StatRowsPerPage);

        readonly List<StatPage> statPages = new List<StatPage>(4);
        readonly StatLineElement[] statLines;

        SpellReportDetailsSnapshot secondarySpellDetails = SpellReportDetailsSnapshot.Empty;
        string ownerClassName = string.Empty;
        RelationToPlayerKind ownerRelation = RelationToPlayerKind.Self;

        public CharacterStatReportBox(RelativeScreenPosition aPos, RelativeScreenPosition aSize, UIElement aParent = null)
            : base(new UITexture("WhiteBackground", Color.Transparent), aPos, aSize, StatPageSize, aParent)
        {
            statLines = new StatLineElement[StatRowsPerPage];
            for (int i = 0; i < statLines.Length; i++)
            {
                float rowHeight = (1f - StatPageTopPadding) / StatRowsPerPage;
                float y = StatPageTopPadding + rowHeight * i;
                statLines[i] = new StatLineElement(new RelativeScreenPosition(0.05f, y), new RelativeScreenPosition(0.9f, rowHeight), this);
            }

            SetPageElements(statLines, BindStatLine, ClearStatLine);
            SetPageTitleProvider(GetCurrentPageTitle);
            Visible = false;
        }

        public void SetOwnerContext(in EntityUiSnapshot aOwnerSnapshot)
        {
            SetOwnerContext(aOwnerSnapshot.ClassName, aOwnerSnapshot.RelationToPlayer);
        }

        public void SetOwnerContext(string aClassName, RelationToPlayerKind aRelation)
        {
            ownerClassName = aClassName ?? string.Empty;
            ownerRelation = aRelation;
        }

        public void SetReport(StatReportSnapshot aPrimary, StatReportSnapshot aSecondary)
        {
            secondarySpellDetails = aSecondary.SpellDetails;
            BuildStatPages(aPrimary, aSecondary);
            RefreshStatPage();
        }

        void RefreshStatPage()
        {
            int currentPage = CurrentPage;
            int pageCount = Math.Max(1, statPages.Count);
            Reset(pageCount * ItemsPerPage);
            SetPage(Math.Min(currentPage, pageCount - 1));
        }

        void BindStatLine(UIElement aElement, int aIndex)
        {
            StatLineElement line = aElement as StatLineElement;
            if (line == null) return;

            int page = aIndex / ItemsPerPage;
            if (page < 0 || page >= statPages.Count)
            {
                line.Clear();
                return;
            }

            int lineOnPage = aIndex % ItemsPerPage;
            List<StatLineSnapshot> lines = statPages[page].Lines;
            if (lineOnPage >= lines.Count)
            {
                line.Clear();
                return;
            }

            StatLineSnapshot pair = lines[lineOnPage];
            bool hasHoverSnapshot = TryBuildStatHoverSnapshot(pair, out ItemDescriptorSnapshot hoverSnapshot);
            line.Set(FormatValue(pair.Value), pair.Name, hoverSnapshot, hasHoverSnapshot);
        }

        string GetCurrentPageTitle(int aPageIndex)
        {
            if (aPageIndex < 0 || aPageIndex >= statPages.Count)
            {
                return $"Page {aPageIndex + 1}";
            }

            return statPages[aPageIndex].Title;
        }

        static void ClearStatLine(UIElement aElement)
        {
            StatLineElement line = aElement as StatLineElement;
            if (line == null) return;
            line.Clear();
        }

        static string FormatValue(double aValue)
        {
            double rounded = Math.Round(aValue);
            if (Math.Abs(aValue - rounded) < 0.01d)
            {
                return rounded.ToString(CultureInfo.InvariantCulture);
            }

            return aValue.ToString("0.##", CultureInfo.InvariantCulture);
        }

        static string FormatPercent(double aRateValue)
        {
            return (aRateValue * 100d).ToString("0.##", CultureInfo.InvariantCulture) + "%";
        }

        static string FormatSignedPercent(double aRateValue)
        {
            string sign = aRateValue > 0 ? "+" : string.Empty;
            return sign + FormatPercent(aRateValue);
        }

        static string FormatSignedValue(double aValue)
        {
            string sign = aValue > 0 ? "+" : string.Empty;
            return sign + FormatValue(aValue);
        }

        void BuildStatPages(StatReportSnapshot aPrimary, StatReportSnapshot aSecondary)
        {
            statPages.Clear();

            List<StatLineSnapshot> primaryLines = new List<StatLineSnapshot>();
            List<StatLineSnapshot> attackLines = new List<StatLineSnapshot>();
            List<StatLineSnapshot> spellLines = new List<StatLineSnapshot>();
            List<StatLineSnapshot> defenseLines = new List<StatLineSnapshot>();

            AddReportLinesByCategory(aPrimary, primaryLines, attackLines, spellLines, defenseLines);
            AddReportLinesByCategory(aSecondary, primaryLines, attackLines, spellLines, defenseLines);

            AddCategoryPages("Primary Stats", primaryLines);
            AddCategoryPages("Attack", attackLines);
            AddCategoryPages("Spell", spellLines);
            AddCategoryPages("Defense", defenseLines);
        }

        void AddReportLinesByCategory(
            StatReportSnapshot aReport,
            List<StatLineSnapshot> aPrimaryLines,
            List<StatLineSnapshot> aAttackLines,
            List<StatLineSnapshot> aSpellLines,
            List<StatLineSnapshot> aDefenseLines)
        {
            if (aReport.Lines == null)
            {
                return;
            }

            for (int i = 0; i < aReport.Count; i++)
            {
                StatLineSnapshot line = aReport.Lines[i];
                StatLineCategory category = line.Category == StatLineCategory.Unspecified
                    ? StatLineCategoryResolver.ResolveCharacter(line.Name)
                    : line.Category;

                switch (category)
                {
                    case StatLineCategory.Primary:
                        aPrimaryLines.Add(line);
                        break;
                    case StatLineCategory.Attack:
                        aAttackLines.Add(line);
                        break;
                    case StatLineCategory.Spell:
                        aSpellLines.Add(line);
                        break;
                    case StatLineCategory.Defense:
                        aDefenseLines.Add(line);
                        break;
                }
            }
        }

        void AddCategoryPages(string aTitle, List<StatLineSnapshot> aLines)
        {
            if (aLines.Count == 0)
            {
                statPages.Add(new StatPage(aTitle, new List<StatLineSnapshot>()));
                return;
            }

            int index = 0;
            while (index < aLines.Count)
            {
                int count = Math.Min(StatRowsPerPage, aLines.Count - index);
                List<StatLineSnapshot> pageLines = new List<StatLineSnapshot>(count);
                for (int i = 0; i < count; i++)
                {
                    pageLines.Add(aLines[index + i]);
                }

                statPages.Add(new StatPage(aTitle, pageLines));
                index += count;
            }
        }

        bool TryBuildStatHoverSnapshot(StatLineSnapshot aLine, out ItemDescriptorSnapshot aSnapshot)
        {
            int statValue = (int)Math.Round(aLine.Value, MidpointRounding.AwayFromZero);
            ClassData classData = ResolveOwnerClassData();

            switch (aLine.Name)
            {
                case "Stamina":
                    {
                        string statReport = $"+{FormatValue(statValue * 10d)} Health\n1 Stamina = 10 Health";
                        aSnapshot = new ItemDescriptorSnapshot("Stamina", "Increases maximum health.", statReport, 0, true, false);
                        return true;
                    }
                case "Strength":
                    {
                        int attackPowerPerPoint = classData?.MeleeAttackBonus == ClassData.MeleeAttackPowerBonus.Strength ? 2 : 1;
                        double attackPower = statValue * attackPowerPerPoint;
                        double blockValue = statValue / 20d;
                        string statReport = $"+{FormatValue(attackPower)} Attack Power\n+{FormatValue(blockValue)} Block Value";
                        aSnapshot = new ItemDescriptorSnapshot("Strength", "Improves melee power and block value.", statReport, 0, true, false);
                        return true;
                    }
                case "Agility":
                    {
                        int attackPower = classData?.MeleeAttackBonus == ClassData.MeleeAttackPowerBonus.Agility ? statValue : 0;
                        double critFromAgility = classData == null ? 0 : statValue * classData.AttackCritChanceScaler;
                        double dodgeFromAgility = classData == null ? 0 : statValue * classData.DodgeChanceScaler;
                        string statReport = $"+{FormatValue(statValue * 2d)} Armor";
                        if (attackPower > 0)
                        {
                            statReport += $"\n+{FormatValue(attackPower)} Attack Power";
                        }

                        statReport += $"\n+{FormatPercent(critFromAgility)} Crit Chance";
                        statReport += $"\n+{FormatPercent(dodgeFromAgility)} Dodge Chance";
                        aSnapshot = new ItemDescriptorSnapshot("Agility", "Improves armor and evasive combat stats.", statReport, 0, true, false);
                        return true;
                    }
                case "Intellect":
                    {
                        double spellCritScaler = classData?.SpellCritChanceScaler ?? (0.01d / 60d);
                        string statReport = $"+{FormatValue(statValue * 15d)} Mana\n+{FormatPercent(statValue * spellCritScaler)} Spell Crit Chance";
                        aSnapshot = new ItemDescriptorSnapshot("Intellect", "Improves mana and spell crit chance.", statReport, 0, true, false);
                        return true;
                    }
                case "Spirit":
                    {
                        string statReport = $"+{FormatValue(statValue * 0.2d)} HP/5\n+{FormatValue(statValue * 0.125d)} Mana/5";
                        aSnapshot = new ItemDescriptorSnapshot("Spirit", "Improves health and mana regeneration.", statReport, 0, true, false);
                        return true;
                    }
                case "Armor":
                    {
                        aSnapshot = new ItemDescriptorSnapshot("Armor", "Reduces incoming physical damage.", $"Current Armor: {FormatValue(aLine.Value)}", 0, true, false);
                        return true;
                    }
                case "Attack Power":
                    {
                        aSnapshot = new ItemDescriptorSnapshot("Attack Power", "Increases your weapon attack damage.", $"Current Attack Power: {FormatValue(aLine.Value)}", 0, true, false);
                        return true;
                    }
                case "Crit Chance":
                    {
                        aSnapshot = new ItemDescriptorSnapshot("Crit Chance", "Chance for attack critical strikes.", $"Current Crit Chance: {FormatPercent(aLine.Value)}", 0, true, false);
                        return true;
                    }
                case "Crit Damage":
                    {
                        aSnapshot = new ItemDescriptorSnapshot("Crit Damage", "Damage multiplier applied on attack crits.", $"Current Crit Damage Multiplier: {FormatValue(aLine.Value)}", 0, true, false);
                        return true;
                    }
                case "Hit Chance":
                    {
                        aSnapshot = new ItemDescriptorSnapshot("Hit Chance", "Bonus chance for attacks to hit.", $"Current Hit Chance: {FormatPercent(aLine.Value)}", 0, true, false);
                        return true;
                    }
                case "Dodge Chance":
                    {
                        aSnapshot = new ItemDescriptorSnapshot("Dodge Chance", "Chance to avoid incoming attacks.", $"Current Dodge Chance: {FormatPercent(aLine.Value)}", 0, true, false);
                        return true;
                    }
                case "Parry Chance":
                    {
                        aSnapshot = new ItemDescriptorSnapshot("Parry Chance", "Chance to parry incoming attacks.", $"Current Parry Chance: {FormatPercent(aLine.Value)}", 0, true, false);
                        return true;
                    }
                case "Spell Damage":
                    {
                        string report = BuildSpellBreakdownReport(aLine.Value, secondarySpellDetails.DamageBonuses, "Spell Damage", false);
                        aSnapshot = new ItemDescriptorSnapshot("Spell Damage", "Base spell damage plus school-specific bonuses.", report, 0, true, false);
                        return true;
                    }
                case "Spell Crit Chance":
                    {
                        string report = BuildSpellBreakdownReport(aLine.Value, secondarySpellDetails.CritChanceBonuses, "Spell Crit Chance", true);
                        aSnapshot = new ItemDescriptorSnapshot("Spell Crit Chance", "Base spell crit chance plus school-specific bonuses.", report, 0, true, false);
                        return true;
                    }
                case "Spell Crit Damage":
                    {
                        aSnapshot = new ItemDescriptorSnapshot("Spell Crit Damage", "Damage multiplier applied on spell crits.", $"Base Spell Crit Damage: {FormatValue(aLine.Value)}", 0, true, false);
                        return true;
                    }
                case "Spell Hit Chance":
                    {
                        string report = BuildSpellBreakdownReport(aLine.Value, secondarySpellDetails.HitChanceBonuses, "Spell Hit Chance", true);
                        aSnapshot = new ItemDescriptorSnapshot("Spell Hit Chance", "Base spell hit chance plus school-specific bonuses.", report, 0, true, false);
                        return true;
                    }
                default:
                    aSnapshot = default;
                    return false;
            }
        }

        string BuildSpellBreakdownReport(double aBaseValue, SpellSchoolBonusSnapshot[] aBonuses, string aDisplayName, bool isPercent)
        {
            StringBuilder builder = new StringBuilder();
            builder.Append("Base: ");
            builder.Append(isPercent ? FormatPercent(aBaseValue) : FormatValue(aBaseValue));

            if (aBonuses == null || aBonuses.Length == 0)
            {
                return builder.ToString();
            }

            List<SpellSchoolBonusSnapshot> ordered = new List<SpellSchoolBonusSnapshot>(aBonuses);
            ordered.Sort((a, b) => string.CompareOrdinal(a.SchoolName, b.SchoolName));

            for (int i = 0; i < ordered.Count; i++)
            {
                string schoolName = ordered[i].SchoolName;
                if (string.IsNullOrWhiteSpace(schoolName))
                {
                    continue;
                }

                double value = ordered[i].Value;
                if (Math.Abs(value) < 0.000001d)
                {
                    continue;
                }

                builder.Append('\n');
                builder.Append(isPercent ? FormatSignedPercent(value) : FormatSignedValue(value));
                builder.Append(' ');
                builder.Append(schoolName);
                builder.Append(' ');
                builder.Append(aDisplayName);
            }

            return builder.ToString();
        }

        ClassData ResolveOwnerClassData()
        {
            if (string.IsNullOrWhiteSpace(ownerClassName))
            {
                return null;
            }

            try
            {
                return ownerRelation switch
                {
                    RelationToPlayerKind.Self => ObjectFactory.GetPlayerClass(ownerClassName),
                    RelationToPlayerKind.Friendly => ObjectFactory.GetAllyClass(ownerClassName),
                    RelationToPlayerKind.Neutral or RelationToPlayerKind.Hostile => ObjectFactory.GetMobClass(ownerClassName),
                    _ => null
                };
            }
            catch
            {
                return null;
            }
        }
    }
}
