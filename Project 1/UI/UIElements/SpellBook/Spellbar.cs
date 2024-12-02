using Microsoft.Xna.Framework;
using Project_1.Textures;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Project_1.UI.UIElements.SpellBook
{
    internal class SpellBar : Box
    {
        SpellButton[] spellButtons;
        Border border;
        public SpellBar(UITexture aGfx, int aButtonCount, Vector2 aPos, float aSizeX) : base(aGfx, aPos, new Vector2(aSizeX, calcY(aSizeX, aButtonCount)))
        {
            Vector2 offset = Camera.Camera.GetRelativeXSquare(calcOffset(aSizeX, aButtonCount));
            float buttonSize = calcButtonSize(aSizeX, aButtonCount);
            //border = new Border(Vector2.Zero, new Vector2(aSizeX, calcY(aSizeX, aButtonCount)));
            border = new Border(Vector2.Zero, RelativeSize);
            children.Add(border);
            //Vector2 size = new Vector2(aSize.Y - 0.05f);
            spellButtons = new SpellButton[aButtonCount];

            for (int i = 0; i < spellButtons.Length; i++)
            {
                spellButtons[i] = new SpellButton(Input.KeyBindManager.KeyListner.SpellBar1Spell1 + i, new Vector2(offset.X + (offset.X + buttonSize) * i, offset.Y), Camera.Camera.GetRelativeXSquare(buttonSize));
            }
            children.AddRange(spellButtons);
        }

        static float calcOffset(float aSizeX, int aButtonCount)
        {
            return aSizeX / (aButtonCount * 10);
        }

        static float calcButtonSize(float aSizeX, int aButtonCount)
        {
            return (aSizeX - calcOffset(aSizeX, aButtonCount) * (aButtonCount + 1)) / aButtonCount;
        }

        static float calcY(float aSizeX, int aButtonCount)
        {
            return Camera.Camera.GetRelativeXSquare(calcButtonSize(aSizeX, aButtonCount)).Y + Camera.Camera.GetRelativeXSquare(calcOffset(aSizeX, aButtonCount)).Y * 2;
        }
    }
}
