using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Project_1.GameObjects;
using Project_1.Managers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Project_1.Particles
{
    internal static class ParticleManager
    {
        static List<Particle> particles;
        
        public static void Init()
        {
            particles = new List<Particle>();
        }

        public static void SpawnParticle(ParticleBase aParticle, Vector2 aWorldPos, GameObject aParent)
        {
            particles.Add(new Particle(aWorldPos, aParticle, aParent));
        }

        public static void SpawnParticle(ParticleBase aParticle, Rectangle aWorldPos, GameObject aParent)
        {
            Vector2 pos = new Vector2((float)RandomManager.RollDouble(aWorldPos.Left, aWorldPos.Right), (float)RandomManager.RollDouble(aWorldPos.Top, aWorldPos.Bottom));
            SpawnParticle(aParticle, pos, aParent);
        }

        public static void SpawnParticle(ParticleBase aParticle, Rectangle aWorldPos, double aParticlesPerSecond, GameObject aParent)
        {
            if (aParticlesPerSecond >= 1)
            {
                for (int i = 0; i < (int)Math.Floor(aParticlesPerSecond); i++)
                {
                    SpawnParticle(aParticle, aWorldPos, aParent);
                }
            }

            double pps = aParticlesPerSecond - Math.Floor(aParticlesPerSecond);

            if (RandomManager.RollDouble() <= pps)
            {
                SpawnParticle(aParticle, aWorldPos, aParent);
            }
        }

        public static void Update()
        {
            for (int i = particles.Count - 1; i >= 0; i--) 
            { 
                particles[i].Update();
                if (particles[i].IsDead)
                {
                    particles.RemoveAt(i);
                }
            }
        }

        public static void Draw(SpriteBatch aBatch)
        {
            for (int i = 0; i < particles.Count; i++)
            {
                particles[i].Draw(aBatch);
            }
        }
    }
}
