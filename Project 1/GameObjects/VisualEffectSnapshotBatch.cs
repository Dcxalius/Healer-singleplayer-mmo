using Microsoft.Xna.Framework.Graphics;
using Project_1.Camera;
using Project_1.GameObjects.Entities;
using Project_1.Managers;

namespace Project_1.GameObjects
{
    /// <summary>
    /// Effect snapshot batch with inline storage for the common small-count case.
    /// Falls back to an array only when more than 2 effects are active.
    /// </summary>
    internal readonly struct VisualEffectSnapshotBatch
    {
        readonly int count;
        readonly VisualEffectRenderSnapshot effect0;
        readonly VisualEffectRenderSnapshot effect1;
        readonly VisualEffectRenderSnapshot[] overflow;
        readonly int overflowCount;

        public static readonly VisualEffectSnapshotBatch Empty = new VisualEffectSnapshotBatch(0, default, default, null);

        public VisualEffectSnapshotBatch(int count, VisualEffectRenderSnapshot effect0, VisualEffectRenderSnapshot effect1, VisualEffectRenderSnapshot[] overflow)
        {
            this.count = count;
            this.effect0 = effect0;
            this.effect1 = effect1;
            this.overflow = overflow;
            overflowCount = count > 2 ? count - 2 : 0;
        }

        public void Draw(SpriteBatch batch, WorldSpace position, float feetPosY)
        {
            ThreadAffinity.AssertMainThread();
            if (count <= 0) return;

            effect0.Draw(batch, position, feetPosY);
            if (count == 1) return;

            effect1.Draw(batch, position, feetPosY);
            for (int i = 0; i < overflowCount; i++)
            {
                overflow[i].Draw(batch, position, feetPosY);
            }
        }
    }
}
