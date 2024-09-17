using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms.Design;

namespace Project_1.Managers
{
    internal static class RandomManager
    {
        static Random random;
        public static void Init()
        {
            random = new Random();
        }

        public static int RollInt()
        {
            return random.Next();
        }

        public static int RollInt(int aMaxSize)  // 0 to max -1
        {
            return random.Next(aMaxSize);
        }

        public static int RollInt(int aMinSize, int aMaxSize) // min to max -1
        {
            return random.Next(aMinSize, aMaxSize);
        }

        public static int RollIntWithAvoidant(int aMaxSize, int aAvoidant)
        {
            int i = RollInt(aMaxSize - 1);

            if (i >= aAvoidant) { i++; }

            return i;
        }

        public static double RollDouble()
        {
            return random.NextDouble();
        }
    }
}
