using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Project_1.Managers
{
    internal static class RandomManager
    {
        static Random random;
        static readonly object randomLock = new object();
        static bool initialized;

        public static void Init()
        {
            ThreadAffinity.AssertMainThread();
            if (initialized) return;
            initialized = true;
            if (DebugManager.Mode(DebugMode.FalseRandom))
            {
                random = new Random(1);
                return;
            }
            random = new Random();
        }

        public static int RollInt()
        {
            lock (randomLock)
            {
                return random.Next();
            }
        }

        public static int RollInt(int aMaxSize)  // 0 to max -1
        {
            lock (randomLock)
            {
                return random.Next(aMaxSize);
            }
        }

        public static int RollInt(int aMinSize, int aMaxSize) // min to max -1
        {
            lock (randomLock)
            {
                return random.Next(aMinSize, aMaxSize);
            }
        }

        
        public static int RollInt((int, int) aMinMax)
        {
            return RollInt(aMinMax.Item1, aMinMax.Item2);
        }

        public static int RollIntWithAvoidant(int aMaxSize, int aAvoidant)
        {
            int i = RollInt(aMaxSize - 1);

            if (i >= aAvoidant) { i++; }

            return i;
        }

        public static double RollDouble()
        {
            lock (randomLock)
            {
                return random.NextDouble();
            }
        }

        public static double RollDouble(double aMax)
        {
            lock (randomLock)
            {
                return random.NextDouble() * aMax;
            }
        }

        public static double RollDouble(double aMin, double aMax)
        {
            lock (randomLock)
            {
                return aMin + random.NextDouble() * (aMax - aMin);
            }
        }

        public static double RollDouble((double, double) aMinMax)
        {
            return RollDouble(aMinMax.Item1, aMinMax.Item2);
        }
    }
}
