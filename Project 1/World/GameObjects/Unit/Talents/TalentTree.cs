using Project_1.GameObjects.Spells;
using Project_1.Textures;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace Project_1.World.GameObjects.Unit.Talents
{
    internal class TalentTree
    {
        string name;
        public void Spell(Spell aS)
        {
            for (int i = 0; i < talents.GetLength(0); i++)
            {
                for (int j = 0; j < talents.GetLength(1); j++)
                {
                    if (talents[i][j].HasChanges(aS))
                    {
                        aS.AddTalent(talents[i][j]);
                    }
                }
            }
        }

        GfxPath background;
        Talent[][] talents;

        [JsonConstructor]
        public TalentTree(Talent[][] talents, string name, string gfxName)
        {
            this.name = name;
            background = new GfxPath(GfxType.UI, gfxName);
            this.talents = talents;
            int rowMaxRanks = 0;
            for (int i = 0; i < talents.GetLength(0); i++)
            {
                rowMaxRanks += talents[i].Sum(x => x.MaxRank);
                Debug.Assert(rowMaxRanks < 5 * (i + 1), "Too few maxranks, unreachable talents detected");
            }
        }


        
    }
}
