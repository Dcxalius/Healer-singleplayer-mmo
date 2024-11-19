using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Project_1.GameObjects;
using Project_1.Managers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms.Design;

namespace Project_1.Particles
{
    internal class Particle
    {
        ParticleBase particleBase;
        double timeSpawned;
        double lifeSpan;

        GameObject parent;

        public bool IsDead { get => isDead; }
        bool isDead;

        double opacity;

        Vector2 worldPos;

        Vector2 momentum;

        Color color;

        public Particle(Vector2 aWorldPos, ParticleBase aParticleBase, GameObject aParent)
        {
            isDead = false;

            particleBase = aParticleBase;
            worldPos = aWorldPos;

            timeSpawned = TimeManager.TotalFrameTime;
            momentum = particleBase.Momentum;
            lifeSpan = particleBase.LifeSpan;

            color = aParticleBase.Colors[0];
            opacity = 1d;

            parent = aParent;
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

            momentum *= particleBase.Drag;
            momentum += particleBase.Velocity;
            worldPos += momentum;
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
            if (!Camera.MomAmIInFrame(worldPos)) return;
            
            aBatch.Draw(particleBase.Texture, Camera.WorldPosToCameraSpace(worldPos), null, GetOpacityColor(color, opacity), 0f, Vector2.Zero, 1f, SpriteEffects.None, (parent.FeetPos.Y + 1) / (Camera.WorldRectangle.Bottom)); //TODO: Make this use the layerDepth
        }

    }
}
