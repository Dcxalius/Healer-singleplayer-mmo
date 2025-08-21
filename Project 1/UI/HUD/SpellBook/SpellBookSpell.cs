using Microsoft.Xna.Framework;
using Project_1.Camera;
using Project_1.GameObjects.Spells;
using Project_1.Input;
using Project_1.Managers;
using Project_1.Textures;
using Project_1.UI.HUD.Managers;
using Project_1.UI.UIElements;
using Project_1.UI.UIElements.Buttons;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Project_1.UI.HUD.SpellBook
{

    internal class SpellBookSpell : GFXButton
    {

        CooldownTexture onCooldownGfx;
        Border emptyBorder;
        public Spell SpellData
        {
            get => spellData;
            set
            {
                spellData = value;
                if (value != null) imageOnButton.SetImage(spellData.GfxPath);
                else imageOnButton.ClearImage();
            }
        }
        Spell spellData;

        public SpellBookSpell(RelativeScreenPosition aPos, RelativeScreenPosition aSize, Spell aSpell = null) : base(Spell.GetGfxPath(aSpell), aPos, aSize, Color.White)
        {
            onCooldownGfx = new CooldownTexture(CooldownTexture.CooldownGfxType.LeftSwirl);
            if (aSpell != null)
            {
                spellData = aSpell;
                emptyBorder = null;

            }
        }



        protected override void ClickedOnMe(ClickEvent aClick)
        {
            base.ClickedOnMe(aClick);

            if (aClick.ButtonPressed != InputManager.ClickType.Left) return;

            if (spellData != null)
            {
                HUDManager.HoldSpell(spellData, InputManager.GetMousePosAbsolute() - Location);
            }
        }

        public override void ClickedOnAndReleasedOnMe()
        {
            base.ClickedOnAndReleasedOnMe();

            HUDManager.ReleaseSpell();
        }

        protected override void HoldReleaseAwayFromMe()
        {

            HUDManager.ReleaseSpell();

            if (spellData != null)
            {
                InputManager.CreateReleaseEvent(this, heldEvents.ClickThatCreated);
            }

            base.HoldReleaseAwayFromMe();


        }
    }
}
