﻿using Microsoft.Xna.Framework;
using Project_1.Textures;
using Project_1.UI.UIElements;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Project_1.Input;
using Project_1.Managers;
using Project_1.GameObjects.Spells;
using Microsoft.Xna.Framework.Graphics;

namespace Project_1.UI.HUD
{
    internal class SpellButton : GFXButton
    {
        CooldownTexture onCooldownGfx;
        Border emptyBorder;

        KeyBindManager.KeyListner keyListner;

        Spell spellData;

        public SpellButton(KeyBindManager.KeyListner aKeyListner, Vector2 aPos, Vector2 aSize, Spell aSpell = null) : base(Spell.GetGfxPath(aSpell), aPos, aSize, Color.Gray)
        {
            keyListner = aKeyListner;
            onCooldownGfx = new CooldownTexture();
            emptyBorder = new Border(Vector2.Zero, aSize);
            //gfx = new UITexture(Spell img);
        }



        public void AssignSpell(Spell aSpell)
        {
            spellData = aSpell;
            gfx = new UITexture(Spell.GetGfxPath(aSpell), Color.White);
        }

        public override void Update(in UIElement aParent)
        {
            base.Update(aParent);

            if (spellData == null) return;

            onCooldownGfx.UpdateDuration(spellData.RatioOfCooldownDone);

            if (KeyBindManager.GetPress(keyListner))
            {
                Triggered();
            }
        }

        public override void HoldReleaseOnMe()
        {
            base.HoldReleaseOnMe();
            Triggered();
        }

        void Triggered()
        {
            if (spellData == null) return;

            spellData.Trigger();
        }

        public override void Rescale()
        {
            base.Rescale();
            emptyBorder.Rescale();
        }

        public override void Draw(SpriteBatch aBatch)
        {
            base.Draw(aBatch);

            if (spellData == null)
            {
                emptyBorder.Draw(aBatch);
                return;
            }

            if (!spellData.OffCooldown)
            {
                onCooldownGfx.Draw(aBatch, pos, Color.White);
            }
        }
    }
}