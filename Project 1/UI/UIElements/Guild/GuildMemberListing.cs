using Microsoft.Xna.Framework;
using Project_1.Camera;
using Project_1.GameObjects.Entities;
using Project_1.GameObjects.Entities.Players;
using Project_1.Textures;
using Project_1.UI.UIElements.Boxes;
using SharpDX.XAudio2;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.Remoting;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms.Design.Behavior;

namespace Project_1.UI.UIElements.Guild
{
    internal class GuildMemberListing : Box, IComparable //TODO: Should this be button?
    {
        Label name;
        Label level;
        Label @class;
        OpenInventory openInventory;
        OpenCharacterWindow openCharacterWindow;
        InviteButton invite;

        RelativeScreenPosition buttonSize;
        RelativeScreenPosition spacing = RelativeScreenPosition.GetSquareFromX(0.005f);
        RelativeScreenPosition buttonPos;
        RelativeScreenPosition changeInY;

        RelativeScreenPosition GetButtonPos
        {
            get
            {
                buttonPos += changeInY;
                return buttonPos;
            }
        }


        public GuildMemberListing(Friendly aGuildMember, RelativeScreenPosition aPos, RelativeScreenPosition aSize) : base(new UITexture("WhiteBackground", Color.Pink), aPos, aSize)
        {
            buttonSize = RelativeScreenPosition.GetSquareFromY(aSize.Y - spacing.Y - spacing.Y);
            buttonPos = new RelativeScreenPosition(aSize.X, spacing.Y);
            changeInY = new RelativeScreenPosition(-buttonSize.X - spacing.X, 0);
            
            openCharacterWindow = new OpenCharacterWindow(GetButtonPos, buttonSize);
            openInventory = new OpenInventory(GetButtonPos, buttonSize);
            invite = new InviteButton(aGuildMember, GetButtonPos, buttonSize);

            float labelPosX = (buttonPos.X - spacing.X - spacing.X) / 3;
            RelativeScreenPosition labelSize = new RelativeScreenPosition(labelPosX, aSize.Y);

            name = new Label(aGuildMember.Name, new RelativeScreenPosition(spacing.X, 0), labelSize, Label.TextAllignment.CentreLeft);
            level = new Label(aGuildMember.CurrentLevel.ToString(), new RelativeScreenPosition(spacing.X + labelPosX, 0), labelSize, Label.TextAllignment.Centred);
            @class = new Label(aGuildMember.Class, new RelativeScreenPosition(spacing.X + labelPosX * 2, 0), labelSize, Label.TextAllignment.CentreRight);

            AddChild(name);
            AddChild(level);
            AddChild(@class);
            AddChild(openCharacterWindow);
            AddChild(openInventory);

            if (aGuildMember.GetType() != typeof(Player))
            {
                AddChild(invite);
            }
        }

        public void RefreshData(GameObjects.Entities.GuildMember.GuildMemberData aGuildMember)
        {
            name.Text = aGuildMember.Name;
            level.Text = aGuildMember.Level;
            @class.Text = aGuildMember.Class;
        }

        public int CompareTo(object obj)
        {
            Debug.Assert(obj.GetType() == GetType());
            GuildMemberListing comparedTo = obj as GuildMemberListing;

            return CompareTo(comparedTo, 0);
        }

        int CompareTo(GuildMemberListing aComparedTo, int aSortLevel)
        {
            switch (GuildRoster.sortOrder[aSortLevel])
            {
                case GuildRoster.SortBy.Name:
                    return string.Compare(name.Text, aComparedTo.name.Text);
                case GuildRoster.SortBy.Level:
                    int actualLevel = int.Parse(level.Text);
                    int comaredToLevel = int.Parse(aComparedTo.level.Text);
                    if (actualLevel > comaredToLevel)
                    {
                        return -1;
                    }
                    else if (actualLevel < comaredToLevel)
                    {
                        return 1;
                    }
                    else
                    {
                        return CompareTo(aComparedTo, aSortLevel + 1);
                    }
                case GuildRoster.SortBy.Class:
                    int comparer = string.Compare(@class.Text, aComparedTo.@class.Text);
                    if (comparer == 0)
                    {
                        return CompareTo(aComparedTo, aSortLevel + 1);
                    }
                    return comparer;
                default:
                    throw new NotImplementedException();
            }
        }
    }
}
