using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace FuriousLibrary_XNA4.HelperClasses
{
    public static class RandomHelper
    {
        public static Random random = new Random();

        /// <summary>
        /// returns a random float between and including the given values
        /// </summary>
        /// <param name="min">min value that can be returned</param>
        /// <param name="max">max value that can be returned</param>
        /// <returns>a randomly generated float</returns>
        public static float RandomFloatBetween(float min, float max)
        {
            float ret = min + (float)random.NextDouble() * (max - min);
            return ret;
        }

        /// <summary>
        /// returns a random int between and including the given values
        /// </summary>
        /// <param name="min">min value that can be returned</param>
        /// <param name="max">max value that can be returned</param>
        /// <returns>a randomly generated integer</returns>
        public static int RandomIntBetween(int min, int max)
        {
            int ret = min + random.Next(max - min + 1);
            return ret;
        }
    }
}
