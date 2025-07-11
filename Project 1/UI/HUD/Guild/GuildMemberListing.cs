using Microsoft.Xna.Framework;
using Project_1.Camera;
using Project_1.GameObjects.Entities;
using Project_1.GameObjects.Entities.Players;
using Project_1.Textures;
using Project_1.UI.UIElements;
using Project_1.UI.UIElements.Boxes;
using Project_1.UI.UIElements.Buttons;
using SharpDX.XAudio2;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.Remoting;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms.Design.Behavior;

namespace Project_1.UI.HUD.Guild
{
    internal class GuildMemberListing : Box, IComparable //TODO: Should this be button?
    {
        Friendly friendly;
        Label name;
        Label level;
        Label @class;
        OpenInventory openInventory;
        OpenInspectWindow openInspectWindow;
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


        public GuildMemberListing(Friendly aFriendly, AbsoluteScreenPosition aSizeForButtonScaling) : base(new UITexture("WhiteBackground", Color.Pink), RelativeScreenPosition.One, RelativeScreenPosition.One) //TODO: Make this sortable
        {
            friendly = aFriendly;

            spacing = RelativeScreenPosition.GetSquareFromX(0.005f, aSizeForButtonScaling);
            buttonSize = RelativeScreenPosition.GetSquareFromY(1 - spacing.Y * 2, aSizeForButtonScaling);
            buttonPos = new RelativeScreenPosition(1, spacing.Y);
            changeInY = new RelativeScreenPosition(-buttonSize.X - spacing.X, 0);

            openInspectWindow = new OpenInspectWindow(friendly, GetButtonPos, buttonSize);
            openInventory = new OpenInventory(GetButtonPos, buttonSize);

            float labelPosX = (buttonPos.X - spacing.X - spacing.X) / 3;
            RelativeScreenPosition labelSize = new RelativeScreenPosition(labelPosX, 1);

            name = new Label(aFriendly.Name, new RelativeScreenPosition(spacing.X, 0), labelSize, Label.TextAllignment.CentreLeft);
            level = new Label(aFriendly.CurrentLevel.ToString(), new RelativeScreenPosition(spacing.X + labelPosX, 0), labelSize, Label.TextAllignment.Centred);
            @class = new Label(aFriendly.Class, new RelativeScreenPosition(spacing.X + labelPosX * 2, 0), labelSize, Label.TextAllignment.CentreRight);

            AddChild(name);
            AddChild(level);
            AddChild(@class);
            AddChild(openInspectWindow);
            AddChild(openInventory);

            if (aFriendly.GetType() != typeof(Player))
            {
                invite = new InviteButton(aFriendly, GetButtonPos, buttonSize);
                AddChild(invite);
            }
        }

        public void SetInviteButtonState(TwoStateGFXButton.State aState)
        {
            invite.state = aState;
        }

        public void RefreshData(GuildMember.GuildMemberData aGuildMember)
        {
            name.Text = aGuildMember.Name;
            level.Text = aGuildMember.Level;
            @class.Text = aGuildMember.Class;
        }

        public bool BelongsTo(string aName)
        {
            return name.Text == aName;
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
                    int comparedToLevel = int.Parse(aComparedTo.level.Text);
                    if (actualLevel > comparedToLevel)
                    {
                        return -1;
                    }
                    else if (actualLevel < comparedToLevel)
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
