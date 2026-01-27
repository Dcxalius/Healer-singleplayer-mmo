using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Project_1.Camera;
using Project_1.Managers;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;

namespace Project_1.Particles
{
    internal static class ParticleManager
    {
        static List<Particle> particles;
        static readonly ConcurrentQueue<ParticleSpawnRequest> pendingSpawns = new ConcurrentQueue<ParticleSpawnRequest>();
        static bool initialized;

        public static void Init()
        {
            ThreadAffinity.AssertMainThread();
            if (initialized) return;
            initialized = true;
            particles = new List<Particle>();

        }

        public static void SpawnParticle(ParticleBase aParticle, WorldSpace aWorldPos, float aLayerFeetY, ParticleMovement aParticleMovement)
        {
            if (ThreadAffinity.IsMainThread)
            {
                particles.Add(new Particle(aWorldPos, aParticle, aLayerFeetY, aParticleMovement));
                return;
            }
            pendingSpawns.Enqueue(new ParticleSpawnRequest(aWorldPos, aParticle, aParticleMovement, aLayerFeetY));
        }

        public static void SpawnParticle(ParticleBase aParticle, Rectangle aWorldPos, float aLayerFeetY, ParticleMovement aParticleMovement)
        {
            ThreadAffinity.AssertSimThread();
            WorldSpace pos = new WorldSpace((float)RandomManager.RollDouble(aWorldPos.Left, aWorldPos.Right), (float)RandomManager.RollDouble(aWorldPos.Top, aWorldPos.Bottom));
            SpawnParticle(aParticle, pos, aLayerFeetY, aParticleMovement);
        }

        public static void SpawnParticle(ParticleBase aParticle, Rectangle aWorldPos, float aLayerFeetY, ParticleMovement aParticleMovement, double aParticlesPerSecond)
        {
            ThreadAffinity.AssertSimThread();
            aParticlesPerSecond *= TimeManager.SecondsSinceLastFrame;
            if (aParticlesPerSecond >= 1)
            {
                for (int i = 0; i < (int)Math.Floor(aParticlesPerSecond); i++)
                {
                    SpawnParticle(aParticle, aWorldPos, aLayerFeetY, aParticleMovement);
                }
            }

            double pps = aParticlesPerSecond - Math.Floor(aParticlesPerSecond);

            if (RandomManager.RollDouble() <= pps)
            {
                SpawnParticle(aParticle, aWorldPos, aLayerFeetY, aParticleMovement);
            }
        }

        public static void SpawnParticle(ParticleBase aParticle, Rectangle aWorldPos, float aLayerFeetY, ParticleMovement aParticleMovement, int aParticleCount)
        {
            ThreadAffinity.AssertSimThread();
            for (int i = 0; i < aParticleCount; i++)
            {
                SpawnParticle(aParticle, aWorldPos, aLayerFeetY, aParticleMovement);
            }
        }

        public static void Update()
        {
            ThreadAffinity.AssertMainThread();
            while (pendingSpawns.TryDequeue(out ParticleSpawnRequest request))
            {
                particles.Add(new Particle(request.WorldPos, request.Particle, request.LayerFeetY, request.Movement));
            }
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
            for (int i = 0; i < particles.Count; i++)
            {
                particles[i].Draw(aBatch);
            }
        }

        readonly struct ParticleSpawnRequest
        {
            public ParticleSpawnRequest(WorldSpace worldPos, ParticleBase particle, ParticleMovement movement, float layerFeetY)
            {
                WorldPos = worldPos;
                Particle = particle;
                Movement = movement;
                LayerFeetY = layerFeetY;
            }

            public WorldSpace WorldPos { get; }
            public ParticleBase Particle { get; }
            public ParticleMovement Movement { get; }
            public float LayerFeetY { get; }
        }
    }
}
