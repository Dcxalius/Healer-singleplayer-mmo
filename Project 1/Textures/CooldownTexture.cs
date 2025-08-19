using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Project_1.Managers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Project_1.Textures
{
    internal class CooldownTexture : UITexture, IEffects
    {
        public enum CooldownGfxType
        {
            SweepRight,
            SweepUp,
            LeftSwirl
        }

        public double Ratio { set => ratio = value; }

        public string Effect => "CooldownSwirl";

        public Texture Texture => swirlTexture;
        static Texture swirlTexture = new UITexture("LeftSwirl", Color.White);
        public RenderTarget2D ReturnedRenderTarget { get => effectsRenderTarget; set => effectsRenderTarget = value; }
        RenderTarget2D effectsRenderTarget;

        public EffectManager.SimpleEffectParam SimpleEffectParam => simpleEffectParam;
        EffectManager.SimpleEffectParam simpleEffectParam;
        double ratio;
        CooldownGfxType cdType;

        public CooldownTexture() : base("Cooldown", Color.White)
        {
            Visible = new Rectangle(Point.Zero, size);
            simpleEffectParam = new EffectManager.SimpleEffectParam("duration", ratio);
        }

        public Point GetReducedSize(Point aSize)
        {
            Point reduceSizeBy;

            switch (cdType)
            {
                case CooldownGfxType.SweepRight:
                    reduceSizeBy = new Point((int)(aSize.X * (1 - ratio)), 0); 
                    break;
                case CooldownGfxType.SweepUp:
                    reduceSizeBy = new Point(0, (int)(aSize.Y * (1 - ratio)));
                    break;
                case CooldownGfxType.LeftSwirl:
                    return aSize;
                default:
                    throw new NotImplementedException();
            }

            return aSize - reduceSizeBy;
        }

        public void ListForEffect()
        {
            switch (cdType)
            {
                case CooldownGfxType.SweepRight:
                case CooldownGfxType.SweepUp:
                    break;
                case CooldownGfxType.LeftSwirl:
                    simpleEffectParam.value = ratio;
                    ((IEffects)this).AddEffectToNextDraw();
                    break;
                default:
                    break;
            }
        }

        public override void Draw(SpriteBatch aBatch, Rectangle aPosition, Color aColor)
        {
            switch (cdType)
            {
                case CooldownGfxType.SweepRight:
                case CooldownGfxType.SweepUp:
                    base.Draw(aBatch, new Rectangle(aPosition.Location, GetReducedSize(aPosition.Size)), aColor);
                    break;
                case CooldownGfxType.LeftSwirl:
                    if (effectsRenderTarget == null) return;
                    aBatch.Draw(effectsRenderTarget, aPosition, aColor);
                    break;
                default:
                    break;
            }
        }
    }
}
