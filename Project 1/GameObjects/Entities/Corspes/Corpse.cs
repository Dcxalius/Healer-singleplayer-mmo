using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Newtonsoft.Json;
using Newtonsoft.Json.Bson;
using Project_1.Camera;
using Project_1.Input;
using Project_1.Items;
using Project_1.Managers;
using Project_1.Particles;
using Project_1.Textures;
using Project_1.UI.HUD.Managers;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Project_1.GameObjects.Entities.Corspes
{
    internal class Corpse : WorldObject
    {
        static ParticleBase lootGlow;
        static ParticleMovement lootGlowMovement;

        [JsonProperty]
        public LootDrop Drop => drop;
        LootTable loot;
        LootDrop drop;
        [JsonIgnore]
        public float LootLength => lootLength;
        float lootLength;

        [JsonIgnore]
        public bool IsEmpty
        {
            get
            {
                if (drop == null) return true;
                return drop.IsEmpty;
            }
        }

        [JsonIgnore]
        public bool Despawned => isDespawning && timeDespawnStart + despawnTime < TimeManager.TotalFrameTime;

        const double hardDecayTime = 30000;
        const double softDecayTime = 60000;

        const double despawnTime = 1000;

        double timeDied;

        bool isDespawning;
        double timeDespawnStart;

        public override float MaxSpeed => 0;

        [JsonProperty]
        string CorpseName => corpseName;
        string corpseName;

        [JsonProperty]
        WorldSpace Pos => Position;

        public static void Init()
        {

            lootGlow = new ParticleBase((1000d, 1000d), ParticleBase.OpacityType.Fading, ParticleBase.ColorType.Static, new Color[] { Color.Yellow }, new Point(1));
            lootGlowMovement = new ParticleMovement(new WorldSpace(0, -1), WorldSpace.Zero, 0.95f);
        }

        public Corpse(GfxPath aPath, LootTable aLoot, WorldSpace aPos) : this(aPath, aPos)
        {
            if (aLoot != null)
            {
                drop = aLoot.GenerateDrop(this);
            }
        }

        [JsonConstructor] //TODO: Make this take timers for despawnlogic
        Corpse(string corpseName, LootDrop drop, WorldSpace pos) : this(new GfxPath(GfxType.Corpse, corpseName), pos)
        {
            this.drop = drop;
        }

        Corpse(GfxPath aPath, WorldSpace aPosition) : base(new Textures.Texture(aPath), aPosition)
        {
            corpseName = aPath.Name;

            lootLength = WorldRectangle.Size.ToVector2().Length(); //TODO: Should this be rect size / 2 + const from player?

            isDespawning = false;

            timeDied = TimeManager.TotalFrameTime;

            CorpseManager.AddCorpse(this);

        }

        public override bool Click(ClickEvent aClickEvent)
        {
            if (aClickEvent.ButtonPressed != InputManager.ClickType.Right) return false;
            if (!Camera.Camera.WorldRectToScreenRect(WorldRectangle).Contains(aClickEvent.AbsolutePos.ToPoint())) return false;
            if (ObjectManager.Player.FeetPosition.DistanceTo(Centre) > lootLength) return false;
            if (drop.IsEmpty) return false;

            HUDManager.Loot(drop);

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
            if (!Despawned) return;

            CorpseManager.RemoveCorpse(this);
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
                gfx.Draw(aBatch, Position.ToAbsoltueScreenPosition().ToVector2(), Color.White * (float)(1 - (TimeManager.TotalFrameTime - timeDespawnStart) / despawnTime), FeetPosition.Y);
                return;
            }
            gfx.Draw(aBatch, Position.ToAbsoltueScreenPosition().ToVector2(), FeetPosition.Y);

        }
    }
}
