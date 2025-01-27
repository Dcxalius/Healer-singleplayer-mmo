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
using Project_1.GameObjects.Entities.Players;
using Project_1.Camera;

namespace Project_1.UI.UIElements.SpellBook
{
    internal class SpellButton : GFXButton
    {
        CooldownTexture onCooldownGfx;
        //Border emptyBorder;

        KeyBindManager.KeyListner keyListner;

        Spell spellData;

        public SpellButton(KeyBindManager.KeyListner aKeyListner, RelativeScreenPosition aPos, RelativeScreenPosition aSize, Spell aSpell = null) : base(Spell.GetGfxPath(aSpell), aPos, aSize, Color.Gray)
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

        public override void HoldReleaseOnMe()
        {
            base.HoldReleaseOnMe();
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

        public override void Draw(SpriteBatch aBatch, float aLayer)
        {
            base.Draw(aBatch, aLayer + 0.01f);

            if (spellData == null)
            {
                //emptyBorder.Draw(aBatch);
                return;
            }

            if (!spellData.OffCooldown || !ObjectManager.Player.OffGlobalCooldown)
            {
                onCooldownGfx.Draw(aBatch, AbsolutePos, Color.White, aLayer + 0.05f);
            }


        }
    }
}
