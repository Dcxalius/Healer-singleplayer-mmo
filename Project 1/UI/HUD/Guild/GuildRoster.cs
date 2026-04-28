using Microsoft.Xna.Framework;
using Project_1.Camera;
using Project_1.Messaging.Events;
using Project_1.Textures;
using Project_1.UI.UIElements.Boxes;
using Project_1.UI.UIElements.Buttons;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Project_1.UI.HUD.Guild
{
    internal class GuildRoster : ScrollableBox<GuildMemberListing>
    {
        public enum SortBy
        {
            Name,
            Level,
            Class,
            Count
        }
        static public SortBy[] sortOrder = new SortBy[(int)SortBy.Count] { SortBy.Name, SortBy.Class, SortBy.Level };

        RelativeScreenPosition size;


        List<GuildMemberListing> guildMembers; //TODO: Should be removed
        public GuildRoster(UI.UIElements.UIElement aParent, RelativeScreenPosition aPos, RelativeScreenPosition aSize) : base(aParent, 10, new UITexture("WhiteBackground", Color.AliceBlue), Color.DarkSeaGreen, aPos, aSize)
        {
            RelativeScreenPosition spacing = RelativeScreenPosition.GetSquareFromX(0.005f, Size);
            guildMembers = new List<GuildMemberListing>();
            size = new RelativeScreenPosition(1 - spacing.X * 2, 0.05f);
        }

        public void SetGuildMemberInviteStatus(List<string> aName, List<TwoStateGFXButton.State> aState)
        {
            Debug.Assert(aName.Count == aState.Count);
            for (int i = 0; i < guildMembers.Count; i++)
            {
                for (int j = 0; j < aName.Count; j++)
                {
                    if (!guildMembers[i].BelongsTo(aName[j])) continue;

                    guildMembers[i].SetInviteButtonState(aState[j]);
                    aName.RemoveAt(j);
                    aState.RemoveAt(j);
                }

                if (aName.Count == 0) break;
            }
        }

        public void UpdateListing(int ownerRenderId, int level)
        {
            for (int i = 0; i < guildMembers.Count; i++)
            {
                if (!guildMembers[i].BelongsTo(ownerRenderId)) continue;
                guildMembers[i].UpdateLevel(level);
                break;
            }
        }

        public void SetData(EntityUiSnapshot[] aData)
        {
            guildMembers.Clear();
            KillAllChildren();

            for (int i = 0; i < aData.Length; i++)
            {
                GuildMemberListing gm = new GuildMemberListing(this, aData[i], ElementSize.ToAbsoluteScreenPos(Size));

                AddScrollableElement(gm);
                guildMembers.Add(gm);
            }
            Sort();
        }

        public void AddMember(EntityUiSnapshot aData)
        {
            GuildMemberListing gm = new GuildMemberListing(this, aData, ElementSize.ToAbsoluteScreenPos(Size));
            AddScrollableElement(gm);

            guildMembers.Add(gm);
            Sort();
        }

        public void RemoveMember()
        {
            throw new NotImplementedException();
        }

        internal override void Sort()
        {
            guildMembers.Sort();
            base.Sort();
            //for (int i = 0; i < guildMembers.Count; i++)
            //{
            //    guildMembers[i].Move(firstPosition + changeInY * i);
            //}
        }
    }
}
