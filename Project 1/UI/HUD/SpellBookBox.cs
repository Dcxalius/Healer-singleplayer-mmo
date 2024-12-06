using Microsoft.Xna.Framework;
using Project_1.Camera;
using Project_1.GameObjects;
using Project_1.GameObjects.Spells;
using Project_1.Textures;
using Project_1.UI.UIElements;
using Project_1.UI.UIElements.SpellBook;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Project_1.UI.HUD
{
    internal class SpellBookBox : Box
    {
        SpellBookSpell[] spellBookSpells;
        const int rows = 3;
        const int cols = 2;
        public SpellBookBox(RelativeScreenPosition aPos, RelativeScreenPosition aSize) : base(new UITexture("WhiteBackground", Color.SaddleBrown), aPos, aSize)
        {
            visibleKey = Input.KeyBindManager.KeyListner.SpellBook;
            spellBookSpells = new SpellBookSpell[cols * rows];
            RelativeScreenPosition startPos = Camera.Camera.GetRelativeXSquare(0.01f);
            RelativeScreenPosition spacing = Camera.Camera.GetRelativeXSquare(0.005f);
            RelativeScreenPosition size = Camera.Camera.GetRelativeXSquare(0.015f);

            Spell[] spells = ObjectManager.Player.SpellBook.Spells;

            for (int i = 0; i < spellBookSpells.Length; i++)
            {
                RelativeScreenPosition pos = new RelativeScreenPosition(startPos.X + (float)Math.Floor((double)i / rows) * (size.X + spacing.X), startPos.Y + i % rows * (size.Y + spacing.Y));
                if (i < spells.Length)
                {
                    spellBookSpells[i] = new SpellBookSpell(pos, size, spells[i]);
                    continue;
                }
                spellBookSpells[i] = new SpellBookSpell(pos, size);
            }

            children.AddRange(spellBookSpells);
            ToggleVisibilty();

        }

        public void AssignSpell(Spell aSpell)
        {
            spellBookSpells.First(spell => spell.SpellData == null).SpellData = aSpell;
        }
    }
}
