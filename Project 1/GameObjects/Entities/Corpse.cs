using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Newtonsoft.Json.Bson;
using Project_1.Camera;
using Project_1.Input;
using Project_1.Items;
using Project_1.Managers;
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
        public Item[] Drop => drop;
        LootTable loot;
        Item[] drop;
        public float LootLength => lootLength;
        float lootLength;

        public bool IsEmpty => drop.All(drop => drop == null);

        const double hardDecayTime = 10000;
        const double softDecayTime = 20000;

        const double despawnTime = 1000;

        double timeDied;

        bool isDespawning;
        double timeDespawnStart;

        public Corpse(GfxPath aPath, LootTable aLoot) : base(new Textures.Texture(aPath), WorldSpace.Zero)
        {
            loot = aLoot;
            lootLength = WorldRectangle.Size.ToVector2().Length();
            lootGlow = new ParticleBase((1000d, 1000d), ParticleBase.OpacityType.Fading, ParticleBase.ColorType.Static, new Color[]{ Color.Yellow }, new Point(1));
            lootGlowMovement = new ParticleMovement(new WorldSpace(0, -1), WorldSpace.Zero, 0.95f);
            drop = new Item[0];
            isDespawning = false;
        }

        public void SpawnCorpe(WorldSpace aPos)
        {
            if (loot!= null)
            {
                drop = loot.GenerateDrop();
            }
            Position = aPos;
            ObjectManager.AddCorpse(this);

            timeDied = TimeManager.TotalFrameTime;
        }

        public override bool Click(ClickEvent aClickEvent)
        {
            if (aClickEvent.ButtonPressed != InputManager.ClickType.Right) return false;
            if (!Camera.Camera.WorldRectToScreenRect(WorldRectangle).Contains(aClickEvent.AbsolutePos.ToPoint())) return false;
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

                DespawnWithoutLootInside();
            }

            DespawnWithLootStillInside();

            FinishDespawn();
        }

        void FinishDespawn()
        {
            if (!isDespawning) return;
            if (timeDespawnStart + despawnTime > TimeManager.TotalFrameTime) return;

            ObjectManager.DespawnCorpse(this);
        }

        void DespawnWithoutLootInside()
        {
            if (isDespawning) return;
            if (timeDied + softDecayTime < TimeManager.TotalFrameTime)
            {
                StartDespawn();
            }
        }

        void DespawnWithLootStillInside()
        {
            if (isDespawning) return;
            if (timeDied + hardDecayTime < TimeManager.TotalFrameTime)
            {
                StartDespawn();
            }
        }

        void StartDespawn()
        {
            isDespawning = true; 
            timeDespawnStart = TimeManager.TotalFrameTime;
        }

        public override void Draw(SpriteBatch aBatch)
        {
            Debug.Assert(gfx != null);
            //gfx.Draw(aBatch, Camera.Camera.WorldPosToCameraSpace(Position), Position.Y);
            if (isDespawning)
            {
                gfx.Draw(aBatch, Position.ToAbsoltueScreenPosition().ToVector2(), Color.White * (float)(1 - ((TimeManager.TotalFrameTime - timeDespawnStart) / despawnTime)), Position.Y);
                return;
            }
            gfx.Draw(aBatch, Position.ToAbsoltueScreenPosition().ToVector2(), Position.Y);

        }
    }
}
