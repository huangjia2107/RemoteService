using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ClientCore.Utils
{
    static class CommonHelper
    {
        // Fisher-Yates shuffle algorithm
        public static void Shuffle(int[] numbers)
        {
            var random = new Random();
            var temp = 0;

            for (int i = numbers.Length - 1; i > 0; i--)
            {
                var r = random.Next(1, i);

                temp = numbers[i];
                numbers[i] = numbers[r];
                numbers[r] = temp;
            }
        }
    }
}
