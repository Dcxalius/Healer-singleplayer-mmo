using Microsoft.Xna.Framework;
using Project_1.Camera;
using Project_1.Messaging.Events;
using Project_1.Messaging;
using Project_1.Textures;
using Project_1.UI.UIElements;
using Project_1.UI.UIElements.Boxes;
using Project_1.UI.UIElements.Buttons;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.Remoting;
using System.Text;
using System.Threading.Tasks;
namespace Project_1.UI.HUD.Guild
{
    internal class GuildMemberListing : Box, IComparable //TODO: Should this be button?
    {
        EntityUiSnapshot data;
        Label name;
        Label level;
        Label @class;
        OpenInventory openInventory;
        OpenInspectWindow openInspectWindow;
        Button talentButton;
        InviteButton invite;
        Button nodeViewer;

        RelativeScreenPosition buttonSize;
        RelativeScreenPosition spacing = RelativeScreenPosition.GetSquareFromX(0.005f);
        RelativeScreenPosition buttonPos;
        RelativeScreenPosition changeInX;

        RelativeScreenPosition GetButtonPos
        {
            get
            {
                buttonPos += changeInX;
                return buttonPos;
            }
        }


        public GuildMemberListing(UIElement aParent, EntityUiSnapshot aData, AbsoluteScreenPosition aSizeForButtonScaling) : base(aParent, new UITexture("WhiteBackground", Color.Pink), RelativeScreenPosition.One, RelativeScreenPosition.One) //TODO: Make this sortable
        {
            data = aData;

            spacing = RelativeScreenPosition.GetSquareFromX(0.005f, aSizeForButtonScaling);
            buttonSize = RelativeScreenPosition.GetSquareFromY(1 - spacing.Y * 2, aSizeForButtonScaling);
            buttonPos = new RelativeScreenPosition(1, spacing.Y);
            changeInX = new RelativeScreenPosition(-buttonSize.X - spacing.X, 0);

            openInspectWindow = new OpenInspectWindow(this, aData, GetButtonPos, buttonSize);
            openInventory = new OpenInventory(this, GetButtonPos, buttonSize);
            talentButton = new Button(this, new List<Action> { OpenTalentWindow }, GetButtonPos, buttonSize, Color.LightGoldenrodYellow, "Tal", Color.Black);
            if (aData.RelationToPlayer != RelationToPlayerKind.Self)
            {
                invite = new InviteButton(this, aData.RenderId, GetButtonPos, buttonSize);
                nodeViewer = new Button(this, new List<Action> { NodeViewerOpener }, GetButtonPos, buttonSize, Color.LightGray);
            }

            float labelPosX = (buttonPos.X - spacing.X - spacing.X) / 3;
            RelativeScreenPosition labelSize = new RelativeScreenPosition(labelPosX, 1);

            name = new Label(this, new RelativeScreenPosition(spacing.X, 0), labelSize, Label.TextAllignment.CentreLeft, aText: aData.Name);
            level = new Label(this, new RelativeScreenPosition(spacing.X + labelPosX, 0), labelSize, Label.TextAllignment.Centred, aText: aData.Level.ToString());
            @class = new Label(this, new RelativeScreenPosition(spacing.X + labelPosX * 2, 0), labelSize, Label.TextAllignment.CentreRight, aText: aData.ClassName);
        }

        void OpenTalentWindow()
        {
            MailboxManager.PublishUiEvent(new TalentWindowToggled(data));
        }

        void NodeViewerOpener()
        {
            MailboxManager.PublishUiEvent(new LogicWindowOpened(data.RenderId));
        }

        public void SetInviteButtonState(TwoStateGFXButton.State aState)
        {
            if (invite == null) return;
            invite.state = aState;
        }

        public void RefreshData(in EntityUiSnapshot aGuildMember)
        {
            data = aGuildMember;
            name.Text = aGuildMember.Name;
            level.Text = aGuildMember.Level.ToString();
            @class.Text = aGuildMember.ClassName;
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
