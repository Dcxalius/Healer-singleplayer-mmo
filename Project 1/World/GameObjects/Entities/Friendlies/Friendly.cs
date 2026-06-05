using Microsoft.Xna.Framework;
using Project_1.Camera;
using Project_1.GameObjects.Unit;
using Project_1.GameObjects.Unit.Classes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Project_1.GameObjects.Entities.Friendlies.GuildMembers;

namespace Project_1.GameObjects.Entities.Friendlies
{
    internal class Friendly : Entity
    {
        public override Color MinimapColor => Color.Pink;
        public new FriendlyClassData ClassData => base.ClassData as FriendlyClassData;
        public int RemainingTalentPoints => Math.Max(0, CurrentTalentPoints - UnitData.LearntTalents.Sum(x => x.rank));

        public Friendly(UnitData aUnitData) : base(aUnitData)
        {

        }

        int CurrentTalentPoints => Level.CurrentLevel - 9;

        public bool TryLearnTalent(int aTalentId)
        {
            //TODO: This is too big, break it up.
            if (RemainingTalentPoints <= 0) return false;
            if (ClassData?.TalentTrees == null) return false;

            for (int treeIndex = 0; treeIndex < ClassData.TalentTrees.Length; treeIndex++)
            {
                var tree = ClassData.TalentTrees[treeIndex];
                if (tree?.Talents == null) continue;

                int pointsSpentInTree = tree.GetIds.Sum(GetTalentRank);
                for (int rowIndex = 0; rowIndex < tree.Talents.Length; rowIndex++)
                {
                    var row = tree.Talents[rowIndex];
                    if (row == null) continue;

                    for (int talentIndex = 0; talentIndex < row.Length; talentIndex++)
                    {
                        var talent = row[talentIndex];
                        if (talent == null || talent.Id != aTalentId) continue;

                        if (GetTalentRank(aTalentId) >= talent.MaxRank) return false;
                        if (pointsSpentInTree < rowIndex * 5) return false;
                        if (talent.Required.Any(x => GetTalentRank(x.id) < x.amount)) return false;

                        var learntTalents = UnitData.LearntTalents;
                        for (int i = 0; i < learntTalents.Length; i++)
                        {
                            if (learntTalents[i].id != aTalentId) continue;
                            learntTalents[i].rank++;
                            UnitData.BaseStats.RefreshStats();
                            FlagForRefresh();
                            return true;
                        }
                        throw new Exception("Talent not found in learnt talents");
                    }
                }
            }

            return false;
        }


        public GuildMember.GuildMemberData CreateGuildMemberData() => new GuildMember.GuildMemberData(Name, CurrentLevel, Class);
        public override void ExpToParty(int aExpAmount)
        {
            //TODO: Not sure what this should do. Or should it just be abstracted?
            throw new NotImplementedException();
        }

        protected override bool CheckForRelation()
        {
            //TODO: What should this do? Or should it just be abstracted?
            throw new NotImplementedException();
        }
    }
}
