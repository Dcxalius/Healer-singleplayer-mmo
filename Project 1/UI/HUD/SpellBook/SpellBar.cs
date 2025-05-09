using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Project_1.Camera;
using Project_1.GameObjects.Spells;
using Project_1.Textures;
using Project_1.UI.UIElements;
using Project_1.UI.UIElements.Boxes;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Project_1.UI.HUD.SpellBook
{
    internal class SpellBar : Box
    {
        SpellButton[] spellButtons;
        Border border;
        public SpellBar(UITexture aGfx, int aButtonCount, RelativeScreenPosition aPos, float aSizeX) : base(aGfx, aPos, new RelativeScreenPosition(aSizeX, calcY(aSizeX, aButtonCount)))
        {
            Vector2 offset = RelativeScreenPosition.GetSquareFromX(calcOffset(1, aButtonCount), Size);
            float buttonSize = calcButtonSize(1, aButtonCount);
            //border = new Border(Vector2.Zero, new Vector2(aSizeX, calcY(aSizeX, aButtonCount)));
            border = new Border(RelativeScreenPosition.Zero, RelativeScreenPosition.One);
            AddChild(border);
            //Vector2 size = new Vector2(aSize.Y - 0.05f);
            spellButtons = new SpellButton[aButtonCount];

            for (int i = 0; i < spellButtons.Length; i++)
            {
                spellButtons[i] = new SpellButton(Input.KeyBindManager.KeyListner.SpellBar1Spell1 + i, new RelativeScreenPosition(offset.X + (offset.X + buttonSize) * i, offset.Y), RelativeScreenPosition.GetSquareFromX(buttonSize, Size));
            }
            AddChildren(spellButtons);
        }

        public void LoadBar(Spell[] aSpells)
        {
            Debug.Assert(aSpells.Length == spellButtons.Length);
            for (int i = 0; i < aSpells.Length; i++)
            {

                //if (aSpells[i] == null)
                //{
                //    spellButtons[i].AssignSpell(null);
                //    continue;
                //}

                spellButtons[i].AssignSpell(aSpells[i]);
            }
        }

        public string[] SaveBar()
        {
            string[] returnable = new string[spellButtons.Length];
            for (int i = 0; i < spellButtons.Length; i++)
            {
                Spell spell = spellButtons[i].SpellData;
                if (spell == null) continue;
                returnable[i] = spell.Name;
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
