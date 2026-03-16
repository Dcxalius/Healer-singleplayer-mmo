using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Project_1.Camera;
using Project_1.Managers;
using System;

namespace Project_1.Particles
{
    internal class Particle
    {
        ParticleBase particleBase;
        double timeSpawned;
        double lifeSpan;

        float layerFeetY;

        public bool IsDead { get => isDead; }
        bool isDead;

        double opacity;

        WorldSpace pos;

        WorldSpace momentum;

        Color color;

        ParticleMovement particleMovement;
        float rotation;

        public Particle(WorldSpace aPos, ParticleBase aParticleBase, float aLayerFeetY, ParticleMovement aMovement)
        {
            isDead = false;

            particleBase = aParticleBase;
            pos = aPos;

            timeSpawned = TimeManager.TotalFrameTime;
            momentum = aMovement.Momentum;
            lifeSpan = particleBase.LifeSpan;

            color = aParticleBase.Color;
            opacity = 1d;

            layerFeetY = aLayerFeetY;

            particleMovement = aMovement;
            rotation = (float)Math.Atan2(aMovement.Momentum.Y, aMovement.Momentum.X);
        }


        public void Update()
        {
            if (timeSpawned + lifeSpan < TimeManager.TotalFrameTime)
            {
                isDead = true;
                return;
            }

            if (particleBase.Opacity == ParticleBase.OpacityType.Fading)
            {
                opacity = 1 - ((TimeManager.TotalFrameTime - timeSpawned) / lifeSpan);
            }

            momentum *= particleMovement.Drag;
            momentum += particleMovement.Velocity;
            pos += momentum;
        }

        Color GetOpacityColor(Color aColor, double aOpacity)
        {
            if (opacity == 1d)
            {
                return aColor;
            }

            byte r;
            byte g;
            byte b;
            byte a;

            aColor.Deconstruct(out r, out g, out b, out a);

            r = (byte) (r * aOpacity);
            g = (byte) (g * aOpacity);
            b = (byte) (b * aOpacity);
            a = (byte) (a * aOpacity);

            return new Color(r, g, b, a);
        }

        public void Draw(SpriteBatch aBatch)
        {
            ThreadAffinity.AssertMainThread();
            if (!Camera.Camera.WorldspaceBoundsCheck(pos)) return;
            
            aBatch.Draw(particleBase.Texture, pos.ToAbsoltueScreenPosition().ToVector2(), null, GetOpacityColor(color, opacity), rotation, Vector2.Zero, 1f, SpriteEffects.None, (layerFeetY + 1) / (Camera.Camera.WorldRectangle.Bottom)); //TODO: Make this use the layerDepth
            //aBatch.Draw(particleBase.Texture, Camera.Camera.WorldPosToCameraSpace(worldPos), null, GetOpacityColor(color, opacity), rotation, Vector2.Zero, 1f, SpriteEffects.None, (parent.FeetPos.Y + 1) / (Camera.Camera.WorldRectangle.Bottom)); //TODO: Make this use the layerDepth
        }

    }
}
