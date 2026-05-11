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
        float drawnCooldownRatio;

        public SpellButton(UIElement aParent, KeyBindManager.KeyListner aKeyListner, RelativeScreenPosition aPos, RelativeScreenPosition aSize) : base(aParent, GfxPath.NullPath, aPos, aSize, Color.Gray)
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
                Color = Color.Gray;
                spellOffCooldown = true;
                spellCooldownRatio = 1d;
                drawnCooldownRatio = 0f;
                MarkRenderStale();
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
            Color = Color.White;
            MarkRenderStale();
        }

        public override void Update()
        {
            base.Update();

            if (string.IsNullOrWhiteSpace(spellName)) return;

            if (UiPlayerStateCache.TryGetSpellSnapshot(spellName, out SpellUiSnapshot snapshot))
            {
                imageOnButton.SetImage(snapshot.GfxPath);
                if (spellOffCooldown != snapshot.OffCooldown || Math.Abs(spellCooldownRatio - snapshot.CooldownRatio01) > 0.001d)
                {
                    spellOffCooldown = snapshot.OffCooldown;
                    spellCooldownRatio = snapshot.CooldownRatio01;
                    MarkRenderStale();
                }
            }

            float cooldownRatio = (float)Math.Min(spellCooldownRatio, UiPlayerStateCache.GlobalCooldownRatio);
            if (spellOffCooldown && UiPlayerStateCache.OffGlobalCooldown) cooldownRatio = 0f;
            if (Math.Abs(drawnCooldownRatio - cooldownRatio) > 0.001f)
            {
                drawnCooldownRatio = cooldownRatio;
                MarkRenderStale();
            }
            onCooldownGfx.Ratio = cooldownRatio;
            onCooldownGfx.Update();


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

        protected override void DrawSelf(SpriteBatch aBatch)
        {
            Project_1.Managers.ThreadAffinity.AssertMainThread();
            base.DrawSelf(aBatch);

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
