using Microsoft.Xna.Framework;
using Project_1.Camera;
using Project_1.Textures;
using Project_1.UI.UIElements;
using Project_1.UI.UIElements.Boxes;
using System.Diagnostics;

namespace Project_1.UI.HUD.SpellBook
{
    internal class SpellBar : Box
    {
        SpellButton[] spellButtons;
        Border border;

        public SpellBar(Color aBackgroundColor, int aButtonCount, RelativeScreenPosition aPos, float aSizeX) : base(null, new UITexture("WhiteBackground", aBackgroundColor), aPos, new RelativeScreenPosition(aSizeX, calcY(aSizeX, aButtonCount)))
        {
            Vector2 offset = RelativeScreenPosition.GetSquareFromX(calcOffset(1, aButtonCount), Size);
            float buttonSize = calcButtonSize(1, aButtonCount);
            border = new Border(this, RelativeScreenPosition.Zero, RelativeScreenPosition.One);
            AddChild(border);
            spellButtons = new SpellButton[aButtonCount];

            for (int i = 0; i < spellButtons.Length; i++)
            {
                spellButtons[i] = new SpellButton(this, Input.KeyBindManager.KeyListner.SpellBar1Spell1 + i, new RelativeScreenPosition(offset.X + (offset.X + buttonSize) * i, offset.Y), RelativeScreenPosition.GetSquareFromX(buttonSize, Size));
            }
            AddChildren(spellButtons);
        }

        public void LoadBar(string[] aSpellNames)
        {
            Debug.Assert(aSpellNames.Length == spellButtons.Length);
            for (int i = 0; i < aSpellNames.Length; i++)
            {
                spellButtons[i].AssignSpell(aSpellNames[i]);
            }
        }

        public string[] SaveBar()
        {
            string[] returnable = new string[spellButtons.Length];
            for (int i = 0; i < spellButtons.Length; i++)
            {
                returnable[i] = spellButtons[i].SpellName;
            }
            return returnable;
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
