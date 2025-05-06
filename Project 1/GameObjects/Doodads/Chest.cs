using Project_1.Camera;
using Project_1.Input;
using Project_1.Items;
using Project_1.Textures;
using Project_1.UI.HUD;
using Project_1.UI.UIElements.Inventory;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Project_1.GameObjects.Doodads
{
    internal class Chest : Doodad
    {
        static LootTable t = LootFactory.GetData("Chest");
        LootDrop lootDrop;
        public Chest(WorldSpace aStartingPos) : base(new Texture(new GfxPath(GfxType.Object, "Chest")), aStartingPos)
        {
            
        }

        public override void ClickedOn(ClickEvent aEvent)
        {
            base.ClickedOn(aEvent);

            if (lootDrop == null) lootDrop = t.GenerateDrop(this);

            HUDManager.Loot(lootDrop);
        }
    }
}
