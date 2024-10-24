using Microsoft.Xna.Framework;
using Project_1.Textures;
using Project_1.UI.UIElements;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Project_1.UI.HUD
{
    internal class FirstSpellBar : Spellbar
    {
        
        public FirstSpellBar(int aButtonCount, Vector2 aPos, float aXSize) : base(new UITexture("WhiteBackground", Color.White), aButtonCount, aPos, aXSize) //TODO: Ponder if this is acutally usefull in anyway or if it HUD should just create spellbars directly
        {

        }
    }
}
