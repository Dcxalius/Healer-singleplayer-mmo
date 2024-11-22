using Microsoft.Xna.Framework;
using Project_1.GameObjects.Spells;
using Project_1.Textures;
using Project_1.UI.UIElements;
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
        public SpellBookBox(Vector2 aPos, Vector2 aSize) : base(new UITexture("WhiteBackground", Color.SaddleBrown), aPos, aSize)
        {
            visibleKey = Input.KeyBindManager.KeyListner.SpellBook;
            spellBookSpells = new SpellBookSpell[cols * rows];
            Vector2 startPos = Camera.GetRelativeSquare(0.01f);
            Vector2 spacing = Camera.GetRelativeSquare(0.005f);
            Vector2 size = Camera.GetRelativeSquare(0.015f); 
            for (int i = 0; i < spellBookSpells.Length; i++)
            {
                Vector2 pos = new Vector2(startPos.X + (float)Math.Floor((double)i / rows)* (size.X + spacing.X), startPos.Y + i % rows * (size.Y + spacing.Y));
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
