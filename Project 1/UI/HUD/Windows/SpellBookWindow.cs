﻿using Microsoft.Xna.Framework;
using Project_1.Camera;
using Project_1.GameObjects;
using Project_1.GameObjects.Spells;
using Project_1.Textures;
using Project_1.UI.HUD.SpellBook;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
                spellBookSpells[i] = new SpellBookSpell(pos, size);
            }

            AddChildren(spellBookSpells);

            //ToggleVisibilty();
        }

        public void ClearSpells()
        {
            for (int i = 0; i < spellBookSpells.Length; i++)
            {
                spellBookSpells[i].SpellData = null;
            }
        }

        public void RefreshSpells(Spell[] aSpells)
        {
            Debug.Assert(aSpells.Length <= spellBookSpells.Length);
            ClearSpells();
            for (int i = 0; i < aSpells.Length; i++)
            {

                spellBookSpells[i].SpellData = aSpells[i];
            }
        }

        public void AssignSpell(Spell aSpell)
        {
            spellBookSpells.First(spell => spell.SpellData == null).SpellData = aSpell;
        }
    }
}
