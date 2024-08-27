using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Project_1.Managers
{
    internal static class TimeManager
    {
        public static GameTime gt;

        public static void Update(GameTime aGt)
        {
            gt = aGt;
        }

    }
}
