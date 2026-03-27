using Microsoft.Xna.Framework;
using Project_1.Camera;
using Project_1.Textures;
using Project_1.UI.HUD.SpellBook;
using System;
using System.Diagnostics;
using System.Linq;

namespace Project_1.UI.HUD.Windows
{
    internal class SpellBookWindow : Window
    {
        SpellBookSpell[] spellBookSpells;
        const int rows = 3;
        const int cols = 2;
        public SpellBookWindow() : base(new UITexture("WhiteBackground", Color.SaddleBrown)) //TODO: Rework, This should grab things from the spellbook instead of it being assigned to this object
        {
            visibleKey = Input.KeyBindManager.KeyListner.SpellBook;
            spellBookSpells = new SpellBookSpell[cols * rows];
            RelativeScreenPosition startPos = RelativeScreenPosition.GetSquareFromX(0.01f, Size);
            RelativeScreenPosition spacing = RelativeScreenPosition.GetSquareFromX(0.01f, Size);
            RelativeScreenPosition size = RelativeScreenPosition.GetSquareFromX(0.15f, Size);

            //Spell[] spells = ObjectManager.Player.SpellBook.Spells;

            for (int i = 0; i < spellBookSpells.Length; i++)
            {
                RelativeScreenPosition pos = new RelativeScreenPosition(startPos.X + (float)Math.Floor((double)i / rows) * (size.X + spacing.X), startPos.Y + i % rows * (size.Y + spacing.Y));
                //if (i < spells.Length)
                //{
                //    spellBookSpells[i] = new SpellBookSpell(pos, size, spells[i]);
                //    continue;
                //}
                spellBookSpells[i] = new SpellBookSpell(pos, size, this);
            }

            AddChildren(spellBookSpells);

            //ToggleVisibilty();
        }

        public void ClearSpells()
        {
            for (int i = 0; i < spellBookSpells.Length; i++)
            {
                spellBookSpells[i].SpellName = null;
            }
        }

        public void RefreshSpells(string[] aSpellNames)
        {
            Debug.Assert(aSpellNames.Length <= spellBookSpells.Length); //TODO: Add more spellslots. Also spell pages
            ClearSpells();
            for (int i = 0; i < aSpellNames.Length; i++)
            {
                spellBookSpells[i].SpellName = aSpellNames[i];
            }
        }

        public void AssignSpell(string aSpellName)
        {
            SpellBookSpell openSlot = spellBookSpells.FirstOrDefault(spell => string.IsNullOrWhiteSpace(spell.SpellName));
            if (openSlot == null) return;
            openSlot.SpellName = aSpellName;
        }
    }
}
