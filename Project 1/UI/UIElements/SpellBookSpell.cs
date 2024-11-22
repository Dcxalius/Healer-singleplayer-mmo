using Microsoft.Xna.Framework;
using Project_1.GameObjects.Spells;
using Project_1.Managers;
using Project_1.Textures;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Project_1.UI.UIElements
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

        public SpellBookSpell(Vector2 aPos, Vector2 aSize, Spell aSpell = null) : base(Spell.GetGfxPath(aSpell), aPos, aSize, Color.White)
        {
            onCooldownGfx = new CooldownTexture();
            emptyBorder = new Border(Vector2.Zero, aSize);
            children.Add(emptyBorder);
            if (aSpell != null)
            {
                emptyBorder.ToggleVisibilty();
            }
        }
    }
}
