using Microsoft.Xna.Framework;
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
using Project_1.GameObjects;

namespace Project_1.UI.UIElements.SpellBook
{
    internal class SpellButton : GFXButton
    {
        CooldownTexture onCooldownGfx;
        //Border emptyBorder;

        KeyBindManager.KeyListner keyListner;

        Spell spellData;

        public SpellButton(KeyBindManager.KeyListner aKeyListner, Vector2 aPos, Vector2 aSize, Spell aSpell = null) : base(Spell.GetGfxPath(aSpell), aPos, aSize, Color.Gray)
        {
            keyListner = aKeyListner;
            onCooldownGfx = new CooldownTexture();
            //emptyBorder = new Border(Vector2.Zero, aSize);
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

            onCooldownGfx.Ratio = Math.Min(spellData.RatioOfCooldownDone, ObjectManager.Player.RatioOfGlobalCooldownDone); //TODO: Consider splitting the cd effect to two seperate ones

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

        public override void ReleaseOnMe(ReleaseEvent aRelease)
        {
            base.ReleaseOnMe(aRelease);

            if (aRelease.Parent.GetType() != typeof(SpellBookSpell)) return;

            AssignSpell((aRelease.Parent as SpellBookSpell).SpellData);
        }

        void Triggered()
        {
            if (spellData == null) return;

            ObjectManager.Player.CastSpell(spellData);
        }

        public override void Rescale()
        {
            base.Rescale();
            //emptyBorder.Rescale();
        }

        public override void Draw(SpriteBatch aBatch)
        {
            base.Draw(aBatch);

            if (spellData == null)
            {
                //emptyBorder.Draw(aBatch);
                return;
            }

            if (!spellData.OffCooldown || !ObjectManager.Player.OffGlobalCooldown)
            {
                onCooldownGfx.Draw(aBatch, AbsolutePos, Color.White);
            }
        }
    }
}
