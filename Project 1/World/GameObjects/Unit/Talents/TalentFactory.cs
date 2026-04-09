using Project_1.Managers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Project_1.World.GameObjects.Unit.Talents
{
    internal static class TalentFactory
    {
        static List<Talent> talents;
        static List<TalentTree> talentTrees;

        public static void Init()
        {
            ThreadAffinity.AssertMainThread();
            talents = new List<Talent>();
            talentTrees = new List<TalentTree>();


        }

        internal static Talent GetTalent(int id)
        {
            throw new NotImplementedException();
        }


    }
}
