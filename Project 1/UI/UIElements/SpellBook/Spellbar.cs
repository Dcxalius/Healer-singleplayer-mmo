using Microsoft.Xna.Framework;
using Project_1.Camera;
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
        public SpellBar(UITexture aGfx, int aButtonCount, RelativeScreenPosition aPos, float aSizeX) : base(aGfx, aPos, new RelativeScreenPosition(aSizeX, calcY(aSizeX, aButtonCount)))
        {
            Vector2 offset = RelativeScreenPosition.GetSquareFromX(calcOffset(aSizeX, aButtonCount));
            float buttonSize = calcButtonSize(aSizeX, aButtonCount);
            //border = new Border(Vector2.Zero, new Vector2(aSizeX, calcY(aSizeX, aButtonCount)));
            border = new Border(RelativeScreenPosition.Zero, RelativeSize);
            children.Add(border);
            //Vector2 size = new Vector2(aSize.Y - 0.05f);
            spellButtons = new SpellButton[aButtonCount];

            for (int i = 0; i < spellButtons.Length; i++)
            {
                spellButtons[i] = new SpellButton(Input.KeyBindManager.KeyListner.SpellBar1Spell1 + i, new RelativeScreenPosition(offset.X + (offset.X + buttonSize) * i, offset.Y), RelativeScreenPosition.GetSquareFromX(buttonSize));
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
            return RelativeScreenPosition.GetSquareFromX(calcButtonSize(aSizeX, aButtonCount)).Y + RelativeScreenPosition.GetSquareFromX(calcOffset(aSizeX, aButtonCount)).Y * 2;
        }
    }
}
