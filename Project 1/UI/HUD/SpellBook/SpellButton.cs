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
using Project_1.Camera;
using Project_1.Messaging;
using Project_1.Messaging.Events;
using Project_1.UI;
using Project_1.UI.UIElements.Buttons;
using Project_1.GameObjects.Entities.Friendlies.Players;

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
            onCooldownGfx = new CooldownTexture(CooldownTexture.CooldownGfxType.LeftSwirl);
        }



        public void AssignSpell(Spell aSpell)
        {
            spellData = aSpell;
            imageOnButton.SetImage(Spell.GetGfxPath(aSpell));
            if (aSpell == null) gfx.Color = Color.Gray;


            //else gfx = new UITexture()

        }

        public override void Update()
        {
            base.Update();

            if (spellData == null) return;

            onCooldownGfx.Update();
            onCooldownGfx.Ratio = (float)Math.Min(spellData.RatioOfCooldownDone, UiPlayerStateCache.GlobalCooldownRatio); //TODO: Consider splitting the cd effect to two seperate ones
            if (spellData.OffCooldown && UiPlayerStateCache.OffGlobalCooldown) onCooldownGfx.Ratio = 0;


            if (UiKeyBindStateCache.GetPress(keyListner))
            {
                Triggered();
            }

            if (!UiPlayerStateCache.HasTarget)
            {
                gfx.Color = Color.White;
                return;
            }
            if (UiPlayerStateCache.TargetFeet.DistanceTo(UiPlayerStateCache.PlayerFeet) > spellData.CastDistance) gfx.Color = Color.Red;
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

            if (aRelease.Creator == null) return;
            if (aRelease.Creator.GetType() != typeof(SpellBookSpell)) return;

            AssignSpell((aRelease.Creator as SpellBookSpell).SpellData);
        }

        void Triggered()
        {
            if (spellData == null) return;

            Mailboxes.Main.Publish(new SpellCastRequested(spellData.Name));

        }

        public override void Rescale()
        {
            base.Rescale();
            //emptyBorder.Rescale();
        }

        public override void Draw(SpriteBatch aBatch)
        {
            Project_1.Managers.ThreadAffinity.AssertMainThread();
            base.Draw(aBatch);

            if (spellData == null)
            {
                //emptyBorder.Draw(aBatch);
                return;
            }

            if (!spellData.OffCooldown || !UiPlayerStateCache.OffGlobalCooldown)
            {
                onCooldownGfx.Draw(aBatch, AbsolutePos, Color.White);
            }


        }
    }
}
