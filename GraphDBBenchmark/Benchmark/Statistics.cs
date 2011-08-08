/*
* sones GraphDB - Community Edition - http://www.sones.com
* Copyright (C) 2007-2011 sones GmbH
*
* This file is part of sones GraphDB Community Edition.
*
* sones GraphDB is free software: you can redistribute it and/or modify
* it under the terms of the GNU Affero General Public License as published by
* the Free Software Foundation, version 3 of the License.
* 
* sones GraphDB is distributed in the hope that it will be useful,
* but WITHOUT ANY WARRANTY; without even the implied warranty of
* MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the
* GNU Affero General Public License for more details.
*
* You should have received a copy of the GNU Affero General Public License
* along with sones GraphDB. If not, see <http://www.gnu.org/licenses/>.
* 
*/

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

