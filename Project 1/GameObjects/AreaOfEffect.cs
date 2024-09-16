using Microsoft.Xna.Framework;
using Project_1.Input;
using Project_1.Textures;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Project_1.GameObjects
{
    internal class AreaOfEffect : GameObject
    {
        Rectangle hitBox;

        public AreaOfEffect(string aGfxName, Vector2 aStartingPos, Vector2 aSize) : base(new Texture(new GfxPath(GfxType.Effect, aGfxName)), aStartingPos)
        {
            hitBox = new Rectangle(aStartingPos.ToPoint(), aSize.ToPoint());
        }

    }
}
