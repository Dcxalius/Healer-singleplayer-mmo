using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Newtonsoft.Json.Linq;
using Project_1.Camera;
using Project_1.GameObjects.Entities.Corspes;
using Project_1.GameObjects.Entities.GroundEffect;
using Project_1.GameObjects.Entities.Friendlies.Players;
using Project_1.GameObjects.FloatingTexts;
using Project_1.GameObjects.Spells;
using Project_1.GameObjects.Spells.Buff;
using Project_1.GameObjects.Unit;
using Project_1.GameObjects.Unit.Classes;
using Project_1.GameObjects.Unit.Resources;
using Project_1.GameObjects.Unit.Stats;
using Project_1.Items;
using Project_1.Managers;
using Project_1.Particles;
using Project_1.Textures;
using Project_1.Textures.AnimatedTextures;
using Project_1.Tiles;
using Project_1.UI.HUD;
using Project_1.UI.HUD.Managers;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing.Printing;
using System.Threading.Tasks.Dataflow;
using static Project_1.GameObjects.Unit.Equipment;

namespace Project_1.GameObjects.Entities
{
    internal abstract partial class Entity : WorldObject
    {

        public Entity(UnitData aUnitData) : base(new RandomAnimatedTexture(aUnitData.GfxPath, new Point(32), 0, TimeSpan.FromMilliseconds(500)), aUnitData.Position)
        {
            unitData = aUnitData;
            buffList = new BuffList();
            unitData.Equipment.SetOwner(this);
            unitData.BaseStats.SetOwner(this);
            unitData.Destination.SetOwner(this);
            unitData.InitializeWeaponSkill(this);
            bloodsplatter = new ParticleBase((1000d, 2000d), ParticleBase.OpacityType.Fading, ParticleBase.ColorType.Static, new Color[] { Color.Red }, new Point(1));
            namePlateRequiresUpdate = false;
            
            spellCast = new SpellCast(this);
            aggroTablesIAmOn = new List<NonFriendly>();

            velocity = unitData.Velocity;
            momentum = UnitData.Momentum;


            groundEffects.Add(new Shadow());
            groundEffects.Add(new SelectRing());
            CreateNamePlate();

        }

        public void Delete()
        {
            Events.Clear();
            RemoveNamePlate();

        }

        public override void Update()
        {
            ThreadAffinity.AssertSimThread();
            if (AmIDead()) return;
            TargetAliveCheck();
            if (unitData.BaseStats.CheckIfResourceRegened()) FlagForRefresh();

            WorldSpace startFeetPosition = FeetPosition;
            Movement(); //TODO: base.Update() is in here which is ugly AF
            if (HasNamePlate && startFeetPosition != FeetPosition) FlagForRefresh();
            AttackTarget();
            spellCast.UpdateSpellChannel();
            buffList.Update(this);


            //HUDManager.RefreshPlateBox(this);
        }

        public void ServerTick() //"Server tick"
        {
            ThreadAffinity.AssertSimThread();
            if (unitData.Tick(InCombat))
            {
                FlagForRefresh();
            }
        }
       
    }
}
