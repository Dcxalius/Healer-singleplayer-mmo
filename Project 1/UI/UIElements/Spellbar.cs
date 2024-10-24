using Microsoft.Xna.Framework;
using Project_1.Textures;
using Project_1.UI.HUD;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Project_1.UI.UIElements
{
    internal class Spellbar : Box
    {
        SpellButton[] spellButtons;
        Border border;
        public Spellbar(UITexture aGfx, int aButtonCount, Vector2 aPos, float aXSize) : base(aGfx, aPos, new Vector2(aXSize, calcOffset(aXSize, aButtonCount) *2 + calcButtonSize(aXSize, aButtonCount)))
        {
            float offset = calcOffset(aXSize, aButtonCount);
            float buttonSize = calcButtonSize(aXSize, aButtonCount);
            Vector2 size = new Vector2(aXSize, offset *2 + buttonSize);
            border = new Border(Vector2.Zero, size);
            children.Add(border);
            //Vector2 size = new Vector2(aSize.Y - 0.05f);
            //Vector2 size = Camera.TransformAbsoluteToRelativeScreenSpace(new Point(Camera.TransformRelativeToAbsoluteScreenSpace(aSize - new Vector2(0.02f)).Y)); //Ugliest code I've ever written? Yes, but it should give squares on creation
            Vector2 xddSize = Camera.TransformAbsoluteToRelativeScreenSpace(new Point(Camera.TransformRelativeToAbsoluteScreenSpace(size - new Vector2(offset) * 2).X)); //Ugliest code I've ever written? Yes, but it should give squares on creation
            spellButtons = new SpellButton[aButtonCount];

            for (int i = 0; i < spellButtons.Length; i++)
            {
                spellButtons[i] = new SpellButton(Input.KeyBindManager.KeyListner.SpellBar1Spell1 + i, new Vector2(offset + (offset + buttonSize) * i, offset), xddSize);
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
    }
}
