using Newtonsoft.Json;
using Project_1.GameObjects.Spells;
using Project_1.Textures;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Project_1.World.GameObjects.Unit.Talents
{
    internal class Talent
    {
        public bool HasChanges(Spell aSpell) => changes.Any(x => x.spellDataId == aSpell.SpellDataId);

        GfxPath gfxPath;

        public int Id => id;
        int id;
        (int id, int amount)[] required;
        public int MaxRank => maxRank;
        int maxRank;

        public double GetChanges(int id, int rank, TalentChange change, bool flat) => changes.Where(x => x.spellDataId == id).SelectMany(x => x.changes).Where(x => x.change == change && flat == x.flat).Sum(x => x.amount * rank);
        List<((TalentChange change, float amount, bool flat)[] changes, int spellDataId)> changes;

        public string Name => name;
        string name;
        string description;

        [JsonConstructor]
        public Talent(int id, string name, string gfxName, (int id, int amount)[] required, List<((TalentChange change, float amount, bool flat)[] changes, int spellId)> changes)
        {
            //TODO: Should the gfxtype be spell image? Maybe talents should have their own gfx type?
            gfxPath = new GfxPath(GfxType.SpellImage, gfxName);

            description = GenerateDescription();
            this.name = name;
            this.id = id;
            this.required = required;
            this.changes = changes;
        }

        //TODO: Change this
        string GenerateDescription() => $"Talent: {name}\n\nRequires:\n{string.Join("\n", required.Select(x => $"{x.amount} points in {TalentFactory.GetTalent(x.id).Name}"))}\n\nChanges:\n{string.Join("\n", changes.SelectMany(x => x.changes).Select(x => $"- {(x.flat ? "Flat" : "Percent")} {x.change} increase by {x.amount}"))}";
    }

    public enum TalentChange
    {
        CastSpeed,
        Amount,
        Duration,
        Range,
        Radius,
        Cooldown,
        Cost
    }
}
