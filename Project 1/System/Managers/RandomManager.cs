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

        //TODO: This class should be mostly rewritten.
        //Q: Does anything other than Sim use random? If not, why are we locking? If so, should we have a separate RandomManager for each thread that randoms

        //TODO: World Generation, Loot, and Combat should all use separate seeds so they can be replayed independently.
        //TODO: World Generation should also have a non-order dependant generation Chunk x y should be the same no matter when it is generated
        //Q: What other systems use random? And should they have separate seeds? Should each type of seed have a seperate manager?
        //TODO: The player should at world gen be able to set the seeds (possibly, definitly if debug is enabled)
            
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

        /// <summary>
        /// Random integer from 0 to int.maxvalue
        /// </summary>
        /// <returns></returns>
        public static int RollInt()
        {
            lock (randomLock)
            {
                return random.Next();
            }
        }

        /// <summary>
        /// Random integer from 0 to aMaxSize - 1. Use with avoidant if you want to exclude a number.
        /// </summary>
        /// <param name="aMaxSize"></param>
        /// <returns></returns>
        public static int RollInt(int aMaxSize) 
        {
            lock (randomLock)
            {
                return random.Next(aMaxSize);
            }
        }

        /// <summary>
        /// Generates a random positive integer within the specified range.
        /// </summary>
        /// <param name="aMinSize">The inclusive lower bound of the random number to generate.</param>
        /// <param name="aMaxSize">The exclusive upper bound of the random number to generate. Must be greater than <paramref
        /// name="aMinSize"/>.</param>
        /// <returns>A random integer that is greater than or equal to <paramref name="aMinSize"/> and less than <paramref
        /// name="aMaxSize"/>.</returns>
        public static int RollInt(int aMinSize, int aMaxSize) // min to max -1
        {
            lock (randomLock)
            {
                return random.Next(aMinSize, aMaxSize);
            }
        }

        /// <summary>
        /// Generates a random positive integer within the specified range.
        /// </summary>
        /// <param name="aRange"></param>
        /// <returns></returns>
        public static int RollInt((int min, int max) aRange)
        {
            return RollInt(aRange.min, aRange.max);
        }

        /// <summary>
        /// Generate a random integer from 0 to aMaxSize - 1, but if the result is greater than or equal to aAvoidant, add 1 to it. This effectively generates a random integer from 0 to aMaxSize - 1, excluding aAvoidant.
        /// </summary>
        /// <param name="aMaxSize"></param>
        /// <param name="aAvoidant"></param>
        /// <returns></returns>
        public static int RollIntWithAvoidant(int aMaxSize, int aAvoidant)
        {
            int i = RollInt(aMaxSize - 1);

            if (i >= aAvoidant) { i++; }

            return i;
        }

        /// <summary>
        /// Generates a random double from 0.0 to 1.0.
        /// </summary>
        /// <returns></returns>
        public static double RollDouble()
        {
            lock (randomLock)
            {
                return random.NextDouble();
            }
        }

        /// <summary>
        /// Generates a random double from 0.0 to aMax.
        /// </summary>
        /// <param name="aMax"></param>
        /// <returns></returns>
        public static double RollDouble(double aMax)
        {
            lock (randomLock)
            {
                return random.NextDouble() * aMax;
            }
        }

        /// <summary>
        /// Generates a random double within the specified range.
        /// </summary>
        /// <param name="aMin"></param>
        /// <param name="aMax"></param>
        /// <returns></returns>
        public static double RollDouble(double aMin, double aMax) //TODO: Check if this can return negative numbers, not a problem but should be noted in summary if that is the case
        {
            lock (randomLock)
            {
                return aMin + random.NextDouble() * (aMax - aMin);
            }
        }

        /// <summary>
        /// Generates a random double within the specified range.
        /// </summary>
        /// <param name="aRange"></param>
        /// <returns></returns>
        public static double RollDouble((double min, double max) aRange) => RollDouble(aRange.min, aRange.max);
    }
}
