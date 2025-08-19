using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Project_1.Textures;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Project_1.Managers
{
    internal interface IEffects
    {
        public string Effect { get; }
        public Point Size => Texture.size;

        public Textures.Texture Texture { get; }
        public RenderTarget2D ReturnedRenderTarget { get; set; }
        public void AddEffectToNextDraw() => EffectManager.AddEffectToProcess(this);

        public EffectManager.SimpleEffectParam SimpleEffectParam { get; }
    }

    
}
