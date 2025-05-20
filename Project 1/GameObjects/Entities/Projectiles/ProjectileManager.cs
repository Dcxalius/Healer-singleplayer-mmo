using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Project_1.GameObjects.Entities.Projectiles
{
    internal static class ProjectileManager
    {
        static List<Projectile> projectiles;

        public static void Init()
        {
            projectiles = new List<Projectile>();
        }

        public static void AddProjectile(Projectile projectile) => projectiles.Add(projectile);

        public static void Update()
        {
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
            for (int i = 0; i < projectiles.Count; i++)
            {
                projectiles[i].Draw(aBatch);
            }
        }
    }
}
