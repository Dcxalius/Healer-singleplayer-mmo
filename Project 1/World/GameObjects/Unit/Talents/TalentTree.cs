using Project_1.GameObjects.Spells;
using Project_1.Textures;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Project_1.World.GameObjects.Unit.Talents
{
    internal class TalentTree
    {
        public int Id => id;
        public string Name => name;
        public GfxPath Background => background;
        public Talent[][] Talents => talents;
        int id;
        string name;
        GfxPath background;
        Talent[][] talents;

        public void Spell(Spell aS)
        {
            for (int i = 0; i < talents.Length; i++)
            {
                for (int j = 0; j < talents[i].Length; j++)
                {
                    if (talents[i][j].HasChanges(aS))
                    {
                        aS.AddTalent(talents[i][j]);
                    }
                }
            }
        }

        public int[] GetIds
        {
            get
            {
                int[] ids = new int[talents.Sum(x => x.Length)];
                int index = 0;
                for (int i = 0; i < talents.Length; i++)
                {
                    for (int j = 0; j < talents[i].Length; j++)
                    {
                        ids[index] = talents[i][j].Id;
                        index++;
                    }
                }
                return ids;
            }
        }

        public TalentTree(int id, Talent[][] talents, string name, string gfxName)
        {
            this.id = id;
            this.name = name;
            background = new GfxPath(GfxType.UI, gfxName);
            this.talents = talents;
            int cumulativeMaxRanks = 0;
            for (int i = 0; i < talents.Length; i++)
            {
                

                cumulativeMaxRanks += talents[i].Sum(x => x.MaxRank);
                Debug.Assert(cumulativeMaxRanks >= 5 * (i + 1), "Too few maxranks, unreachable talents detected");
            }


        }
    }
}
