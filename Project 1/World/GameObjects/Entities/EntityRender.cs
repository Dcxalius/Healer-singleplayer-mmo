using Microsoft.Xna.Framework;
using Project_1.GameObjects.Entities.Friendlies.GuildMembers;
using Project_1.Managers;
using Project_1.Textures;
using System;

namespace Project_1.GameObjects.Entities
{
    internal partial class Entity
    {
        //TODO: Snapshot management should be moved to the main thread as much as possible
        internal EntityRenderSnapshot BuildRenderSnapshot()
        {
            ThreadAffinity.AssertSimThread();
            Color shadowColor = Color.Black;
            if (this is GuildMember guildMember && ObjectManager.Player != null && ObjectManager.Player.Party.IsInCommand(guildMember))
            {
                shadowColor = Color.DarkGreen;
            }

            bool isSelected = ObjectManager.Player != null && ObjectManager.Player.Target == this;

            Texture.TextureRenderSnapshot textureSnapshot = ApplyOpacity(gfx.BuildRenderSnapshot(), VisualOpacity);

            return new EntityRenderSnapshot(
                RenderId,
                Position,
                FeetPosition.Y,
                Size,
                textureSnapshot,
                BuildEffectSnapshotBatch(),
                isSelected,
                RelationColor,
                MinimapColor,
                shadowColor);
        }

        //TODO: Is there a cleaner way than applying the opacity like this? Should we instead have somewhere in the model class that sets this and keeps track of its own oppacity?
        static Texture.TextureRenderSnapshot ApplyOpacity(Texture.TextureRenderSnapshot aSnapshot, float aOpacity)
        {
            float opacity = Math.Clamp(aOpacity, 0f, 1f);
            if (opacity >= 1f) return aSnapshot;

            Color color = aSnapshot.Color;
            color.A = (byte)Math.Round(color.A * opacity, MidpointRounding.AwayFromZero);
            return new Texture.TextureRenderSnapshot(
                aSnapshot.Path,
                aSnapshot.Visible,
                color,
                aSnapshot.Rotation,
                aSnapshot.Offset,
                aSnapshot.Flip,
                aSnapshot.Size);
        }
    }
}
