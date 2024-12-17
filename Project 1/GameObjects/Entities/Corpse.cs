using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Project_1.Camera;
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

        public Corpse(Textures.Texture aGfx, LootTable aLoot) : base(aGfx, WorldSpace.Zero)
        {
            loot = aLoot;
            lootLength = WorldRectangle.Size.ToVector2().Length();
            lootGlow = new ParticleBase((1000d, 1000d), ParticleBase.OpacityType.Fading, ParticleBase.ColorType.Static, new Color[]{ Color.Yellow }, new Point(1));
            lootGlowMovement = new ParticleMovement(new WorldSpace(0, -1), WorldSpace.Zero, 0.95f);
        }

        public void SpawnCorpe(WorldSpace aPos)
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
            if (!Camera.Camera.WorldRectToScreenRect(WorldRectangle).Contains(aClickEvent.AbsolutePos.ToPoint())) return false;
            //if (!Camera.Camera.WorldPosToCameraSpace(WorldRectangle).Contains(aClickEvent.AbsolutePos.ToPoint())) return false;
            if (aClickEvent.ButtonPressed != InputManager.ClickType.Right) return false;
            if (ObjectManager.Player.FeetPosition.DistanceTo(Centre) > lootLength) return false;
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
            gfx.Draw(aBatch, Position.ToAbsoltueScreenPosition().ToVector2(), Position.Y);
            //gfx.Draw(aBatch, Camera.Camera.WorldPosToCameraSpace(Position), Position.Y);
        }
    }
}
