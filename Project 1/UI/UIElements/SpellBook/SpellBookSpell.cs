using Microsoft.Xna.Framework;
using Project_1.Camera;
using Project_1.GameObjects.Spells;
using Project_1.Input;
using Project_1.Managers;
using Project_1.Textures;
using Project_1.UI.HUD;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Project_1.UI.UIElements.SpellBook
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
                gfxOnButton = new UITexture(spellData.GfxPath, Color.White);
            }
        }
        Spell spellData;

        public SpellBookSpell(RelativeScreenPosition aPos, RelativeScreenPosition aSize, Spell aSpell = null) : base(Spell.GetGfxPath(aSpell), aPos, aSize, Color.White)
        {
            onCooldownGfx = new CooldownTexture();
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
                HUDManager.HoldSpell(spellData, (InputManager.GetMousePosAbsolute() - AbsolutePos.Location).ToVector2());
            }
        }

        public override void HoldReleaseOnMe()
        {
            base.HoldReleaseOnMe();

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
