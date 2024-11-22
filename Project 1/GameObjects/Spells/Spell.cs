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
        Player player;
        SpellData spellData;
        public GfxPath GfxPath { get => spellData.GfxPath; }
        public bool OffCooldown { get => lastTimeCasted + spellData.Cooldown < TimeManager.TotalFrameTime; }
        public double RatioOfCooldownDone { get => Math.Max(TimeManager.TotalFrameTime - lastTimeCasted / lastTimeCasted + spellData.Cooldown, 1); }
        double lastTimeCasted;

        public Spell(string aName, Player aOwner) 
        {
            spellData = SpellFactory.GetSpell(aName);
            player = aOwner;
        }

        public Spell(SpellData aData, Player aOwner)
        {
            spellData = aData;
            player = aOwner;
        }

        public static GfxPath GetGfxPath(Spell aSpell)
        {
            if (aSpell == null) return new GfxPath(GfxType.SpellImage, null);
            return new GfxPath(GfxType.SpellImage, aSpell.spellData.Name);
        }

        public bool Trigger()
        {
            if (!OffCooldown)
            {
                return false;
            }

            if (player.Target == null)
            {
                return Trigger(player);
            }

            if (Trigger(player.Target))
            {
                return Trigger(player);
            }

            return false;
        }

        bool Trigger(Entity aTarget)
        {
            if (!spellData.Targetable(aTarget.Relation))
            {
                return false;
            }

            lastTimeCasted = TimeManager.TotalFrameTime;
            //TODO: Do stuff to target
            return true;
        }
    }
}
