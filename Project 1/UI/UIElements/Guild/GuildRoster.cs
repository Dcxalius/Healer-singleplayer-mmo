using Microsoft.Xna.Framework;
using Project_1.Camera;
using Project_1.GameObjects.Entities;
using Project_1.Textures;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Project_1.UI.UIElements.Guild
{
    internal class GuildRoster : Box
    {
        public enum SortBy
        {
            Name,
            Level,
            Class,
            Count
        }
        static public SortBy[] sortOrder = new SortBy[(int)SortBy.Count] { SortBy.Name, SortBy.Class, SortBy.Level };

        RelativeScreenPosition spacing = RelativeScreenPosition.GetSquareFromX(0.005f);
        RelativeScreenPosition firstPosition = RelativeScreenPosition.GetSquareFromX(0.005f);
        RelativeScreenPosition size;
        RelativeScreenPosition changeInY;
        

        List<GuildMemberListing> guildMembers;
        public GuildRoster(RelativeScreenPosition aPos, RelativeScreenPosition aSize) : base(new UITexture("WhiteBackground", Color.AliceBlue), aPos, aSize)
        {
            guildMembers = new List<GuildMemberListing>();
            size = new RelativeScreenPosition(aSize.X - spacing.X * 2, 0.05f);
            changeInY = new RelativeScreenPosition(0, size.Y + spacing.Y);
        }

        public void SetData(Friendly[] aData)
        {
            guildMembers.Clear();
            KillAllChildren();

            for (int i = 0; i < aData.Length; i++)
            {
                guildMembers.Add(new GuildMemberListing(aData[i], firstPosition + changeInY * i, size));
            }
            Sort();
            AddChildren(guildMembers);
        }

        public void AddMember(Friendly aData)
        {
            guildMembers.Add(new GuildMemberListing(aData, firstPosition, size));
            AddChild(guildMembers.Last());
            Sort();
        }

        public void RemoveMember()
        {
            throw new NotImplementedException();
        }

        public void Sort()
        {
            guildMembers.Sort();
            for (int i = 0; i < guildMembers.Count; i++)
            {
                guildMembers[i].Move(firstPosition + changeInY * i);
            }
        }
    }
}
