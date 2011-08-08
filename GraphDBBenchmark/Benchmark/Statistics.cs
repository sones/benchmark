using System;
using System.Linq;
using System.Collections.Generic;

namespace sones.GraphDBBenchmark.Benchmark
{
    public static class Statistics
    {
        public static double Average(IEnumerable<double> myNumbers)
        {
            return myNumbers.Sum() / myNumbers.Count();
        }

        public static double StandardDeviation(IEnumerable<double> myNumbers)
        {
            var average = Average(myNumbers);

            return Math.Sqrt(myNumbers.Sum(_ => _ * _) / myNumbers.Count() - (average * average));
        }

        public static double Median(IEnumerable<double> myNumbers)
        {
            if (myNumbers.Count() == 0)
                throw new InvalidOperationException("Invalid count of numbers... must be greater that zero!");

            if (myNumbers.Count() % 2 == 0)
            {
                return myNumbers.OrderBy(_ => _).Skip(myNumbers.Count() / 2 - 1).Take(2).Sum() / 2;
            }
            else
            {
                return myNumbers.OrderBy(_ => _).ElementAt((int)Math.Floor((decimal)(myNumbers.Count() / 2)));
            }
        }
    }
}

