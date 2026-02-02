using Microsoft.Xna.Framework;
using Project_1.GameObjects.Entities.Friendlies.GuildMembers;
using Project_1.Managers;

namespace Project_1.GameObjects.Entities
{
    internal partial class Entity
    {
        internal EntityRenderSnapshot BuildRenderSnapshot()
        {
            ThreadAffinity.AssertSimThread();
            Color shadowColor = Color.Black;
            if (this is GuildMember guildMember && ObjectManager.Player != null && ObjectManager.Player.Party.IsInCommand(guildMember))
            {
                shadowColor = Color.DarkGreen;
            }

            bool isSelected = ObjectManager.Player != null && ObjectManager.Player.Target == this;

            return new EntityRenderSnapshot(
                RenderId,
                Position,
                FeetPosition.Y,
                Size,
                gfx.BuildRenderSnapshot(),
                BuildEffectSnapshot(),
                isSelected,
                RelationColor,
                MinimapColor,
                shadowColor);
        }
    }
}
