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

        public enum ColorType
        {
            Static, //Always grabs the first color
            Random, //Grabs a random color from the given array
            Step //Steps through all the colors before repeating
        }
        public Color Color
        {
            get
            {
                switch (colorType)
                {
                    case ColorType.Static:
                        return color[0];
                    case ColorType.Random:
                        return color[RandomManager.RollInt(0, color.Length)];
                    case ColorType.Step:
                        if (stepper >= color.Length)
                        {
                            stepper = 0;
                        }
                        Color c = color[stepper];
                        stepper++;
                        return c;
                    default:
                        throw new NotImplementedException();
                }
            }
        }
        int stepper = 0;
        ColorType colorType;
        Color[] color;

        public enum TextureType
        {
            Static,
            Fading

        }

        public Texture2D Texture { get => texture; }
        TextureType textureType;
        Texture2D texture;


        public enum OpacityType
        {
            Static,
            Fading
        }
        public OpacityType Opacity { get => opacityType; }
        OpacityType opacityType;


        public ParticleBase((double, double) aLifeSpan, OpacityType aOpacity, ColorType aColorType, Color[] aColor, Point aSize, TextureType aTextureType = TextureType.Static)
        {
            lifeSpan = aLifeSpan;

            //drag = aDrag;
            //momentum = aMomentum;
            //velocity = aVelocity;
            colorType = aColorType;
            color = aColor;

            opacityType = aOpacity;

            textureType = aTextureType; 
            texture = GraphicsManager.CreateNewTexture(aSize);
            Color[] textureData;
            switch (textureType)
            {
                case TextureType.Static:
                    textureData = Enumerable.Repeat(Color.White, aSize.X * aSize.Y).ToArray();
                    break;
                case TextureType.Fading:
                    Color color = Color.White;
                    textureData = new Color[aSize.X * aSize.Y];
                    for (int i = 0; i < aColor.Length; i++)
                    {
                        textureData[i] = color;
                        if (i % aSize.X == 0)
                        {
                            color.R = (byte)(color.R / 2);
                            color.G = (byte)(color.G / 2);
                            color.B = (byte)(color.B / 2);
                            color.A = (byte)(color.A / 2);
                        }
                    }
                    break;
                default:
                    throw new NotImplementedException();
            }
            texture.SetData(textureData);
        }
    }
}
