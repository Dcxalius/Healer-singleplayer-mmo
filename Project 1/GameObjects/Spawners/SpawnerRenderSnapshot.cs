using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Project_1.Camera;
using Project_1.GameObjects.Entities;
using Project_1.Managers;

namespace Project_1.GameObjects.Spawners
{
    internal readonly struct SpawnerRenderSnapshot
    {
        readonly bool hasSpawn;
        readonly EntityRenderSnapshot spawn;

        public SpawnerRenderSnapshot(EntityRenderSnapshot spawn, bool hasSpawn)
        {
            this.spawn = spawn;
            this.hasSpawn = hasSpawn;
        }

        public static SpawnerRenderSnapshot Empty => new SpawnerRenderSnapshot(default, false);

        public void Draw(SpriteBatch batch)
        {
            ThreadAffinity.AssertMainThread();
            if (!hasSpawn) return;
            spawn.Draw(batch);
        }

        public void DrawMinimap(SpriteBatch batch, WorldSpace origin, AbsoluteScreenPosition minimapOffset, AbsoluteScreenPosition minimapSize)
        {
            ThreadAffinity.AssertMainThread();
            if (!hasSpawn) return;
            spawn.DrawMinimap(batch, origin, minimapOffset, minimapSize);
        }
    }
}
