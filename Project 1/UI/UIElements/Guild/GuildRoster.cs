using Microsoft.Xna.Framework;
using Project_1.Camera;
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
        RelativeScreenPosition spacing = RelativeScreenPosition.GetSquareFromX(0.05f);
        RelativeScreenPosition firstPosition = RelativeScreenPosition.GetSquareFromX(0.005f);
        RelativeScreenPosition size;
        RelativeScreenPosition changeInY;

        List<GuildMember> guildMembers;
        public GuildRoster(RelativeScreenPosition aPos, RelativeScreenPosition aSize) : base(new UITexture("WhiteBackground", Color.AliceBlue), aPos, aSize)
        {
            guildMembers = new List<GuildMember>();
            size = new RelativeScreenPosition(aSize.X, 0.05f);
            changeInY = new RelativeScreenPosition(0, size.Y + spacing.Y);
            guildMembers.Add(new GuildMember("xdd", firstPosition, spacing));

            children.AddRange(guildMembers);
        }


    }
}
