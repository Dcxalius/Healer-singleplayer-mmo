using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Project_1.Camera;
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
        static volatile Particle[] renderParticles = Array.Empty<Particle>();
        static bool initialized;

        public static void Init()
        {
            if (initialized) return;
            initialized = true;
            particles = new List<Particle>();

        }

        public static void SpawnParticle(ParticleBase aParticle, WorldSpace aWorldPos, GameObject aParent, ParticleMovement aParticleMovement)
        {
            particles.Add(new Particle(aWorldPos, aParticle, aParent, aParticleMovement));
        }

        public static void SpawnParticle(ParticleBase aParticle, Rectangle aWorldPos, GameObject aParent, ParticleMovement aParticleMovement)
        {
            WorldSpace pos = new WorldSpace((float)RandomManager.RollDouble(aWorldPos.Left, aWorldPos.Right), (float)RandomManager.RollDouble(aWorldPos.Top, aWorldPos.Bottom));
            SpawnParticle(aParticle, pos, aParent, aParticleMovement);
        }

        public static void SpawnParticle(ParticleBase aParticle, Rectangle aWorldPos, GameObject aParent, ParticleMovement aParticleMovement, double aParticlesPerSecond)
        {
            aParticlesPerSecond *= TimeManager.SecondsSinceLastFrame;
            if (aParticlesPerSecond >= 1)
            {
                for (int i = 0; i < (int)Math.Floor(aParticlesPerSecond); i++)
                {
                    SpawnParticle(aParticle, aWorldPos, aParent, aParticleMovement);
                }
            }

            double pps = aParticlesPerSecond - Math.Floor(aParticlesPerSecond);

            if (RandomManager.RollDouble() <= pps)
            {
                SpawnParticle(aParticle, aWorldPos, aParent, aParticleMovement);
            }
        }

        public static void SpawnParticle(ParticleBase aParticle, Rectangle aWorldPos, GameObject aParent, ParticleMovement aParticleMovement, int aParticleCount)
        {
            for (int i = 0; i < aParticleCount; i++)
            {
                SpawnParticle(aParticle, aWorldPos, aParent, aParticleMovement);
            }
        }

        public static void Update()
        {
            ThreadAffinity.AssertSimThread();
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
            ThreadAffinity.AssertMainThread();
            Particle[] snapshot = renderParticles;
            for (int i = 0; i < snapshot.Length; i++)
            {
                snapshot[i].Draw(aBatch);
            }
        }

        internal static void BuildRenderSnapshot()
        {
            renderParticles = particles.ToArray();
        }
    }
}
