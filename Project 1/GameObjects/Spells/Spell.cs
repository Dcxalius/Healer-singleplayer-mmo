using Project_1.GameObjects.Entities;
using Project_1.GameObjects.Entities.Players;
using Project_1.Managers;
using Project_1.Textures;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Project_1.GameObjects.Spells
{
    internal class Spell
    {
        Entity owner;
        SpellData spellData;
        public GfxPath GfxPath { get => spellData.GfxPath; }
        public bool OffCooldown { get => lastTimeCasted + spellData.Cooldown < TimeManager.TotalFrameTime; }
        public double RatioOfCooldownDone { get => Math.Min((TimeManager.TotalFrameTime - lastTimeCasted) / spellData.Cooldown, 1); }
        double lastTimeCasted;

        public static GfxPath GetGfxPath(Spell aSpell)
        {
            if (aSpell == null) return new GfxPath(GfxType.SpellImage, null);
            return new GfxPath(GfxType.SpellImage, aSpell.spellData.Name);
        }

        public Spell(string aName, Entity aOwner) 
        {
            spellData = SpellFactory.GetSpell(aName);
            owner = aOwner;
        }

        public Spell(SpellData aData, Entity aOwner)
        {
            spellData = aData;
            owner = aOwner;
        }


        public bool Trigger()
        {
            if (!OffCooldown)
            {
                return false;
            }

            if (owner.Target == null)
            {
                return Trigger(owner);
            }

            return Trigger(owner.Target);
        }

        bool Trigger(Entity aTarget)
        {
            if (!spellData.Targetable(aTarget.Relation))
            {
                return false;
            }

            lastTimeCasted = TimeManager.TotalFrameTime;

            SpellEffect[] effects = spellData.Effects;
            for (int i = 0; i < effects.Length; i++)
            {
                effects[i].Trigger(owner, aTarget);
            }
            return true;
        }
    }
}
