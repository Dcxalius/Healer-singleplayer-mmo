using Microsoft.Xna.Framework.Graphics;
using System;
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
        static volatile Projectile[] renderProjectiles = Array.Empty<Projectile>();
        static bool initialized;

        public static void Init()
        {
            if (initialized) return;
            initialized = true;
            projectiles = new List<Projectile>();
        }

        public static void AddProjectile(Projectile projectile) => projectiles.Add(projectile);

        public static void Update()
        {
            ThreadAffinity.AssertSimThread();
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
            renderProjectiles = projectiles.ToArray();
        }
    }
}
