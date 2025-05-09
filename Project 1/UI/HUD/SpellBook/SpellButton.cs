using Microsoft.Xna.Framework;
using Project_1.Textures;
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
using Project_1.GameObjects.Entities.Players;
using Project_1.Camera;
using Project_1.UI.UIElements.Buttons;

namespace Project_1.UI.HUD.SpellBook
{
    internal class SpellButton : GFXButton
    {
        CooldownTexture onCooldownGfx;
        //Border emptyBorder;

        KeyBindManager.KeyListner keyListner;

        public Spell SpellData => spellData;
        Spell spellData;

        public SpellButton(KeyBindManager.KeyListner aKeyListner, RelativeScreenPosition aPos, RelativeScreenPosition aSize, Spell aSpell = null) : base(Spell.GetGfxPath(aSpell), aPos, aSize, Color.Gray)
        {
            keyListner = aKeyListner;
            onCooldownGfx = new CooldownTexture();
        }



        public void AssignSpell(Spell aSpell)
        {
            spellData = aSpell;
            gfxOnButton = new UITexture(Spell.GetGfxPath(aSpell), Color.White);
            if (aSpell == null) gfx.Color = Color.Gray;


            //else gfx = new UITexture()

        }

        public override void Update()
        {
            base.Update();

            if (spellData == null) return;

            onCooldownGfx.Ratio = Math.Min(spellData.RatioOfCooldownDone, ObjectManager.Player.RatioOfGlobalCooldownDone); //TODO: Consider splitting the cd effect to two seperate ones
            if (spellData.OffCooldown && ObjectManager.Player.OffGlobalCooldown) onCooldownGfx.Ratio = 0;


            if (KeyBindManager.GetPress(keyListner))
            {
                Triggered();
            }

            Player P = ObjectManager.Player;
            if (P.Target == null)
            {
                gfx.Color = Color.White;
                return;
            }
            if (P.Target.FeetPosition.DistanceTo(P.FeetPosition) > spellData.CastDistance) gfx.Color = Color.Red;
            else gfx.Color = Color.White;
        }

        public override void ClickedOnAndReleasedOnMe()
        {
            base.ClickedOnAndReleasedOnMe();
            Triggered();
        }

        public override void ReleaseOnMe(ReleaseEvent aRelease)
        {
            base.ReleaseOnMe(aRelease);

            if (aRelease.Creator.GetType() != typeof(SpellBookSpell)) return;

            AssignSpell((aRelease.Creator as SpellBookSpell).SpellData);
        }

        void Triggered()
        {
            if (spellData == null) return;

            ObjectManager.Player.StartCast(spellData);

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
