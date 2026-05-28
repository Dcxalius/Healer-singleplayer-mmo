using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Concurrent;
using Project_1.GameObjects;
using Project_1.Managers;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Project_1.GameObjects.Entities.Projectiles
{
    internal static class ProjectileManager
    {
        static List<Projectile> projectiles;
        static readonly ConcurrentQueue<Projectile> pendingAdds = new ConcurrentQueue<Projectile>();
        static readonly RenderCache<WorldObjectRenderSnapshot> renderProjectiles = new RenderCache<WorldObjectRenderSnapshot>();
        static readonly HashSet<int> knownProjectileIds = new HashSet<int>();
        static readonly HashSet<int> currentProjectileIds = new HashSet<int>();
        static bool initialized;

        public static void Init()
        {
            ThreadAffinity.AssertMainThread();
            if (initialized) return;
            initialized = true;
            projectiles = new List<Projectile>();
        }

        public static void AddProjectile(Projectile projectile)
        {
            if (projectile == null) return;
            if (ThreadAffinity.IsSimThread)
            {
                projectiles.Add(projectile);
                return;
            }
            pendingAdds.Enqueue(projectile);
        }

        public static void Update()
        {
            ThreadAffinity.AssertSimThread();
            while (pendingAdds.TryDequeue(out Projectile pending))
            {
                projectiles.Add(pending);
            }
            for (int i = projectiles.Count - 1; i >= 0; i--)
            {
                projectiles[i].Update();
                if (projectiles[i].IsFinished)
                {
                    projectiles.RemoveAt(i);
                }
            }
        }

        internal static void DrawSnapshots(SpriteBatch aBatch)
        {
            ThreadAffinity.AssertMainThread();
            // Snapshot-only draw path. Do not read live sim projectile list here.
            renderProjectiles.ApplyUpdates();
            foreach (WorldObjectRenderSnapshot snapshot in renderProjectiles.Values)
            {
                snapshot.Draw(aBatch);
            }
        }

        internal static void BuildRenderSnapshot()
        {
            ThreadAffinity.AssertSimThread();
            currentProjectileIds.Clear();
            for (int i = 0; i < projectiles.Count; i++)
            {
                WorldObjectRenderSnapshot snapshot = projectiles[i].BuildRenderSnapshot();
                renderProjectiles.EnqueueUpdate(snapshot);
                currentProjectileIds.Add(snapshot.RenderId);
            }
            PublishRemovals();
        }

        static void PublishRemovals()
        {
            foreach (int id in knownProjectileIds)
            {
                if (!currentProjectileIds.Contains(id))
                {
                    renderProjectiles.EnqueueRemove(id);
                }
            }
            knownProjectileIds.Clear();
            foreach (int id in currentProjectileIds)
            {
                knownProjectileIds.Add(id);
            }
        }
    }
}
