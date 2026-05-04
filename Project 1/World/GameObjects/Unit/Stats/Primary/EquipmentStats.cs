using Project_1.GameObjects.Unit.Stats;

namespace Project_1.World.GameObjects.Unit.Stats.Primary
{
    internal class EquipmentStats : PrimaryStats //TODO: Shouldn't this be in items? Also I dont think I like that both items and the units both uses this
    {
        public Armor Armor => armor;
        Armor armor;

        public EquipmentStats(int[] aStats, int aArmor) : base(aStats)
        {
            armor = new Armor(aArmor);
        }
    }
}
