using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Project_1.Input;
using Project_1.Items;
using Project_1.Textures;
using Project_1.UI.HUD;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Project_1.GameObjects.Entities
{
    internal class Corpse : GameObject
    {
        public Item[] Drop { get => drop; }
        LootTable loot;
        Item[] drop;
        public float LootLength { get => lootLength; }
        float lootLength;

        public Corpse(Textures.Texture aGfx, LootTable aLoot) : base(aGfx, Vector2.Zero)
        {
            loot = aLoot;
            lootLength = WorldRectangle.Size.ToVector2().Length();
        }

        public void SpawnCorpe(Vector2 aPos)
        {
            if (loot!= null)
            {
                drop = loot.GenerateDrop();
            }
            Position = aPos;
            ObjectManager.AddCorpse(this);
        }

        public override bool Click(ClickEvent aClickEvent)
        {
            if (aClickEvent.ButtonPressed != ClickEvent.ClickType.Right) return false;
            if ((ObjectManager.Player.FeetPos - Centre).Length() > lootLength) return false;

            HUDManager.Loot(this);

            return true;
        }

        public override void Draw(SpriteBatch aBatch)
        {
            Debug.Assert(gfx != null);
            gfx.Draw(aBatch, Camera.WorldPosToCameraSpace(Position), Position.Y);
        }
    }
}
