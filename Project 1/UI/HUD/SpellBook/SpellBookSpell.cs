using Microsoft.Xna.Framework;
using Project_1.Camera;
using Project_1.Input;
using Project_1.Managers;
using Project_1.Textures;
using Project_1.UI.HUD.Managers;
using Project_1.UI.UIElements;
using Project_1.UI.UIElements.Buttons;
using Project_1.Messaging;
using Project_1.Messaging.Events;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Project_1.GameObjects.Entities.Friendlies.Players;

namespace Project_1.UI.HUD.SpellBook
{

    internal class SpellBookSpell : GFXButton
    {
        public string SpellName
        {
            get => spellName;
            set
            {
                spellName = value;
                TryApplySnapshotVisual();
            }
        }
        string spellName;

        public SpellBookSpell(RelativeScreenPosition aPos, RelativeScreenPosition aSize) : base(GfxPath.NullPath, aPos, aSize, Color.White)
        {
        }

        public override void Update()
        {
            base.Update();
            TryApplySnapshotVisual();
        }

        void TryApplySnapshotVisual()
        {
            if (string.IsNullOrWhiteSpace(spellName))
            {
                imageOnButton.ClearImage();
                return;
            }

            if (UiPlayerStateCache.TryGetSpellSnapshot(spellName, out SpellUiSnapshot snapshot))
            {
                imageOnButton.SetImage(snapshot.GfxPath);
                return;
            }

            imageOnButton.ClearImage();
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



        protected override void ClickedOnMe(ClickEvent aClick)
        {
            base.ClickedOnMe(aClick);

            if (aClick.ButtonPressed != InputManager.ClickType.Left) return;

            if (!string.IsNullOrWhiteSpace(spellName))
            {
                MailboxManager.PublishUiEvent(new HeldSpellStart(spellName, UiMouseStateCache.Absolute - Location));
            }
        }

        public override void ClickedOnAndReleasedOnMe()
        {
            base.ClickedOnAndReleasedOnMe();

            MailboxManager.PublishUiEvent(new HeldSpellEnd());
        }

        protected override void HoldReleaseAwayFromMe()
        {

            MailboxManager.PublishUiEvent(new HeldSpellEnd());

            if (!string.IsNullOrWhiteSpace(spellName))
            {
                UiInputBridge.PublishRelease(this, heldEvents.ClickThatCreated);
            }

            base.HoldReleaseAwayFromMe();


        }
    }
}
