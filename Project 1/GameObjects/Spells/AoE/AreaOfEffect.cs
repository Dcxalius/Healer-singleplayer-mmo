using Microsoft.Xna.Framework;
using Project_1.Camera;
using Project_1.GameObjects.Entities;
using Project_1.Input;
using Project_1.Textures;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Project_1.GameObjects.Spells.AoE
{
    internal class AreaOfEffect : GameObject
    {
        public AreaOfEffect(string aGfxName, WorldSpace aStartingPos, WorldSpace aSize) : base(new Texture(new GfxPath(GfxType.Effect, aGfxName)), aStartingPos)
        {
        }

    }
}
