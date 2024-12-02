using Project_1.GameObjects.Entities;
using Project_1.GameObjects.Entities.Players;
using Project_1.Managers;
using Project_1.Textures;
using System;
using System.CodeDom;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Project_1.GameObjects.Spells
{
    internal class Spell
    {
        public static GfxPath GetGfxPath(Spell aSpell)
        {
            if (aSpell == null) return new GfxPath(GfxType.SpellImage, null);

            return new GfxPath(GfxType.SpellImage, aSpell.spellData.Name);
        }

        public float CastDistance { get => spellData.CastDistance; }
        public double CastTime { get => spellData.CastTime; }

        Entity owner;
        SpellData spellData;
        public float ResourceCost { get => spellData.ResourceCost; }
        public GfxPath GfxPath { get => spellData.GfxPath; }
        public bool OffCooldown { get => lastTimeCasted + spellData.Cooldown < TimeManager.TotalFrameTime; }
        public double RatioOfCooldownDone { get => Math.Min((TimeManager.TotalFrameTime - lastTimeCasted) / spellData.Cooldown, 1); }
        double lastTimeCasted;

        public Spell(string aName, Entity aOwner) 
        {
            spellData = SpellFactory.GetSpell(aName);
            owner = aOwner;
            lastTimeCasted = double.NegativeInfinity;
        }

        public Spell(SpellData aData, Entity aOwner)
        {
            spellData = aData;
            owner = aOwner;
            lastTimeCasted = double.NegativeInfinity;
        }




        public bool Trigger(Entity aTarget)
        {

            if (!OffCooldown)
            {
                return false;
            }

            if (aTarget == null)
            {
                return Trigger(owner);
            }

            if (!spellData.Targetable(aTarget.Relation))
            {
                return false;
            }

            lastTimeCasted = TimeManager.TotalFrameTime;

            
            for (int i = 0; i < spellData.Effects.Length; i++)
            {
                spellData.Effects[i].Trigger(owner, aTarget);
            }
            return true;
        }
    }
}
