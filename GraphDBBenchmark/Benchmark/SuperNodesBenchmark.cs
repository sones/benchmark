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
using System.Collections.Generic;
using System.Linq;
using System.Text;
using sones.GraphDS;
using sones.Library.VersionedPluginManager;
using sones.Library.PropertyHyperGraph;
using sones.GraphDB.Request;
using System.Diagnostics;
using sones.GraphDB.TypeSystem;
using System.Collections.Concurrent;
using System.Threading.Tasks;

namespace sones.GraphDBBenchmark.Benchmark
{
    public sealed class SuperNodes : IBenchmark
    {

        #region constructor

        public SuperNodes()
        {
        }

        #endregion

        #region IBenchmark Members

        public void Execute(IGraphDS myGraphDS, long myIterations, Converter.WriteLineToConsole MyWriteLine)
        {
            var vertexType = myGraphDS.GetVertexType<IVertexType>(null, null, new RequestGetVertexType("City"), (stats, vType) => vType);
            var inCountryProperty = vertexType.GetOutgoingEdgeDefinition("InCountry");
            var nameProperty = vertexType.GetPropertyDefinition("Name");

            List<double> timeForCityCountryTraversal = new List<double>();

            for (int i = 0; i < myIterations; i++)
            {
                Stopwatch sw = Stopwatch.StartNew();

                foreach (var aCity in myGraphDS.GetVertices<IEnumerable<IVertex>>(null, null, new RequestGetVertices("City"), (stats, v) => v))
                {
                    var UK_Vertex = aCity.GetOutgoingSingleEdge(inCountryProperty.ID).GetTargetVertex();
                }

                sw.Stop();

                timeForCityCountryTraversal.Add(sw.Elapsed.TotalMilliseconds);
            }

            String result =  GenerateTable(timeForCityCountryTraversal) + Environment.NewLine + String.Format("Average: {0}ms Median: {1}ms StandardDeviation {2}ms ", Statistics.Average(timeForCityCountryTraversal), Statistics.Median(timeForCityCountryTraversal), Statistics.StandardDeviation(timeForCityCountryTraversal));
            Console.WriteLine(result);

            MyWriteLine(result);
        }

        public String GenerateTable(List<double> timeForCityCountryTraversal)
        {
            StringBuilder sb = new StringBuilder();

            //first row
            sb.Append("Execution\t");
            for (int i = 0; i < timeForCityCountryTraversal.Count; i++)
            {
                sb.Append(i + 1 + "\t");
            }
            sb.AppendLine("");
            sb.Append("Time in ms\t");
            for (int i = 0; i < timeForCityCountryTraversal.Count; i++)
            {
                sb.Append(timeForCityCountryTraversal[i] + "\t");
            }

            return sb.ToString();
        }

        #endregion

        #region IPluginable Members

        public IPluginable InitializePlugin(string UniqueString, Dictionary<string, object> myParameters = null)
        {
            return new SuperNodes();
        }

        public string PluginName
        {
            get { return "Supernodes"; }
        }

        public PluginParameters<Type> SetableParameters
        {
            get
            {
                return new PluginParameters<Type>();
            }
        }

        #endregion

        #region IDisposable Members

        public void Dispose()
        {

        }

        #endregion
    }
}
