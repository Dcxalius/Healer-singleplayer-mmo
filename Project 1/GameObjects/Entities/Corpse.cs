using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Project_1.Input;
using Project_1.Items;
using Project_1.Particles;
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
        ParticleBase lootGlow;
        ParticleMovement lootGlowMovement;
        public Item[] Drop { get => drop; }
        LootTable loot;
        Item[] drop;
        public float LootLength { get => lootLength; }
        float lootLength;

        public bool IsEmpty { get => drop.All(drop => drop == null); }

        public Corpse(Textures.Texture aGfx, LootTable aLoot) : base(aGfx, Vector2.Zero)
        {
            loot = aLoot;
            lootLength = WorldRectangle.Size.ToVector2().Length();
            lootGlow = new ParticleBase((1000d, 1000d), ParticleBase.OpacityType.Fading, new Color[]{ Color.Yellow }, new Point(1));
            lootGlowMovement = new ParticleMovement(new Vector2(0, -1), new Vector2(0), 0.95f);
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
            if (aClickEvent.ButtonPressed != InputManager.ClickType.Right) return false;
            if ((ObjectManager.Player.FeetPos - Centre).Length() > lootLength) return false;
            if (drop.All(drop => drop == null)) return false;

            HUDManager.Loot(this);

            return true;
        }

        public override void Update()
        {
            base.Update();

            if (!IsEmpty)
            {
                ParticleManager.SpawnParticle(lootGlow, WorldRectangle, this, lootGlowMovement, 60d);
            }
        }

        public override void Draw(SpriteBatch aBatch)
        {
            Debug.Assert(gfx != null);
            gfx.Draw(aBatch, Camera.WorldPosToCameraSpace(Position), Position.Y);
        }
    }
}
