using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Project_1.Managers;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Project_1.Particles
{
    internal class ParticleBase
    {
        public enum OpacityType
        {
            Static,
            Fading

        }
        public double LifeSpan
        {
            get
            {

                if (lifeSpan.Item1 == lifeSpan.Item2)
                {
                    return lifeSpan.Item1;
                }

                return RandomManager.RollDouble(lifeSpan);
            }
        }

        (double, double) lifeSpan;

        public Color[] Colors { get => color; }
        Color[] color;

        public Texture2D Texture { get => texture; }
        Texture2D texture;


        public OpacityType Opacity { get => opacityType; }
        OpacityType opacityType;

        public ParticleBase((double, double) aLifeSpan, OpacityType aOpacity, Color[] aColor, Point aSize)
        {
            lifeSpan = aLifeSpan;

            //drag = aDrag;
            //momentum = aMomentum;
            //velocity = aVelocity;
            color = aColor;
            opacityType = aOpacity;

            texture = GraphicsManager.CreateNewTexture(aSize);
            Color[] C;
            C = Enumerable.Repeat(Color.White, aSize.X * aSize.Y).ToArray();
            texture.SetData(C);
        }
    }
}
