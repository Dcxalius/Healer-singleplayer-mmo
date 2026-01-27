using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Concurrent;
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
        static volatile Projectile[] renderProjectiles = Array.Empty<Projectile>();
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

        public static void Draw(SpriteBatch aBatch)
        {
            ThreadAffinity.AssertMainThread();
            Projectile[] snapshot = renderProjectiles;
            for (int i = 0; i < snapshot.Length; i++)
            {
                snapshot[i].Draw(aBatch);
            }
        }

        internal static void BuildRenderSnapshot()
        {
            ThreadAffinity.AssertSimThread();
            renderProjectiles = projectiles.ToArray();
        }
    }
}
