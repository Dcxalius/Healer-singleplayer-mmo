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
using System.Windows.Forms;

namespace Project_1.GameObjects.Spells
{
    internal class AreaOfEffect : GameObject
    {
        AreaOfEffectData effectData;
        Entity owner;
        public AreaOfEffect(string aGfxName, WorldSpace aStartingPos, WorldSpace aSize) : base(new Texture(new GfxPath(GfxType.Effect, aGfxName)), aStartingPos)
        {
        }

    }
}
