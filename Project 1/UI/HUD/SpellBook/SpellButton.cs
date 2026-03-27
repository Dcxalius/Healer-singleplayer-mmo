using Microsoft.Xna.Framework;
using Project_1.Textures;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Project_1.Input;
using Project_1.Managers;
using Microsoft.Xna.Framework.Graphics;
using Project_1.Camera;
using Project_1.Messaging;
using Project_1.Messaging.Events;
using Project_1.UI;
using Project_1.UI.UIElements;
using Project_1.UI.UIElements.Buttons;
using Project_1.GameObjects.Entities.Friendlies.Players;

namespace Project_1.UI.HUD.SpellBook
{
    internal class SpellButton : GFXButton
    {
        CooldownTexture onCooldownGfx;
        KeyBindManager.KeyListner keyListner;

        public string SpellName => spellName;
        string spellName;
        bool spellOffCooldown = true;
        double spellCooldownRatio = 1d;

        public SpellButton(KeyBindManager.KeyListner aKeyListner, RelativeScreenPosition aPos, RelativeScreenPosition aSize, UIElement aParent = null) : base(GfxPath.NullPath, aPos, aSize, Color.Gray, aParent)
        {
            keyListner = aKeyListner;
            onCooldownGfx = new CooldownTexture(CooldownTexture.CooldownGfxType.LeftSwirl);
        }

        public void AssignSpell(string aSpellName)
        {
            spellName = aSpellName;
            if (string.IsNullOrWhiteSpace(spellName))
            {
                imageOnButton.ClearImage();
                gfx.Color = Color.Gray;
                spellOffCooldown = true;
                spellCooldownRatio = 1d;
                return;
            }

            if (UiPlayerStateCache.TryGetSpellSnapshot(spellName, out SpellUiSnapshot snapshot))
            {
                imageOnButton.SetImage(snapshot.GfxPath);
                spellOffCooldown = snapshot.OffCooldown;
                spellCooldownRatio = snapshot.CooldownRatio01;
            }
            else
            {
                imageOnButton.ClearImage();
                spellOffCooldown = true;
                spellCooldownRatio = 1d;
            }
            gfx.Color = Color.White;
        }

        public override void Update()
        {
            base.Update();

            if (string.IsNullOrWhiteSpace(spellName)) return;

            if (UiPlayerStateCache.TryGetSpellSnapshot(spellName, out SpellUiSnapshot snapshot))
            {
                imageOnButton.SetImage(snapshot.GfxPath);
                spellOffCooldown = snapshot.OffCooldown;
                spellCooldownRatio = snapshot.CooldownRatio01;
            }

            onCooldownGfx.Update();
            onCooldownGfx.Ratio = (float)Math.Min(spellCooldownRatio, UiPlayerStateCache.GlobalCooldownRatio);
            if (spellOffCooldown && UiPlayerStateCache.OffGlobalCooldown) onCooldownGfx.Ratio = 0;


            if (UiKeyBindStateCache.GetPress(keyListner))
            {
                Triggered();
            }
        }

        public override void ClickedOnAndReleasedOnMe()
        {
            base.ClickedOnAndReleasedOnMe();
            Triggered();
        }

        protected override void OnHover()
        {
            base.OnHover();
            if (string.IsNullOrWhiteSpace(spellName)) return;
            if (!UiPlayerStateCache.TryGetSpellSnapshot(spellName, out SpellUiSnapshot snapshot)) return;
            MailboxManager.PublishUiEvent(new SpellDescriptorBoxSet(snapshot.Descriptor, RelativePositionOnScreen.ToAbsoluteScreenPos()));
        }

        protected override void OnDeHover()
        {
            base.OnDeHover();
            MailboxManager.PublishUiEvent(new DescriptorBoxClear());
        }

        public override void ReleaseOnMe(ReleaseEvent aRelease)
        {
            base.ReleaseOnMe(aRelease);

            if (aRelease.Creator == null) return;
            if (aRelease.Creator.GetType() != typeof(SpellBookSpell)) return;

            AssignSpell((aRelease.Creator as SpellBookSpell).SpellName);
        }

        void Triggered()
        {
            if (string.IsNullOrWhiteSpace(spellName)) return;

            MailboxManager.PublishSimCommand(new SpellCastRequested(spellName));

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

            if (string.IsNullOrWhiteSpace(spellName))
            {
                return;
            }

            if (!spellOffCooldown || !UiPlayerStateCache.OffGlobalCooldown)
            {
                onCooldownGfx.Draw(aBatch, AbsolutePos, Color.White);
            }


        }
    }
}
