using Microsoft.Xna.Framework.Graphics;
using Project_1.Camera;
using Project_1.Managers;
using System;

namespace Project_1.GameObjects.Spawners
{
    internal readonly struct SpawnZoneRenderSnapshot : IRenderSnapshot
    {
        readonly int renderId;
        readonly SpawnerRenderSnapshot[] spawners;

        public SpawnZoneRenderSnapshot(int renderId, SpawnerRenderSnapshot[] spawners)
        {
            this.renderId = renderId;
            this.spawners = spawners ?? Array.Empty<SpawnerRenderSnapshot>();
        }

        public int RenderId => renderId;

        public void Draw(SpriteBatch batch)
        {
            ThreadAffinity.AssertMainThread();
            for (int i = 0; i < spawners.Length; i++)
            {
                spawners[i].Draw(batch);
            }
        }

        public void MinimapDraw(SpriteBatch batch, WorldSpace origin, AbsoluteScreenPosition minimapOffset, AbsoluteScreenPosition minimapSize)
        {
            ThreadAffinity.AssertMainThread();
            for (int i = 0; i < spawners.Length; i++)
            {
                spawners[i].DrawMinimap(batch, origin, minimapOffset, minimapSize);
            }
        }
    }
}
