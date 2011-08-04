﻿/*
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
    public sealed class VTPS_PartitionedParallel : IBenchmark
    {

        #region data

        private String _interestingVertexType = String.Empty;

        #endregion

        #region constructor

        public VTPS_PartitionedParallel()
        {
        }

        public VTPS_PartitionedParallel(String myInterestingVertexType)
        {
            _interestingVertexType = myInterestingVertexType;
        }

        #endregion

        #region IBenchmark Members

        public void Execute(IGraphDS myGraphDS, long myIterations, Converter.WriteLineToConsole MyWriteLine)
        {
            var vertexType = myGraphDS.GetVertexType<IVertexType>(null, null, new RequestGetVertexType(_interestingVertexType), (stats, vType) => vType);
            var vertexList = myGraphDS.GetVertices<List<IVertex>>(null, null, new RequestGetVertices(_interestingVertexType), (stats, vertices) => vertices.ToList());
            Stopwatch sw = Stopwatch.StartNew();

            for (int i = 0; i < myIterations - 1; i++)
            {
                CountAllEdgesParallelPartitioner(vertexType, myGraphDS, vertexList);
            }

            var edgeCount = CountAllEdgesParallelPartitioner(vertexType, myGraphDS, vertexList);

            sw.Stop();

            MyWriteLine(String.Format("Counted {0} edges.", edgeCount));

            MyWriteLine(String.Format("Traversed {0} edges per second", edgeCount / (sw.Elapsed.TotalSeconds / myIterations)));
        }

        #endregion

        #region private helper

        Int64 CountAllEdgesParallelPartitioner(IVertexType usertype, IGraphDS myGraphDB, List<IVertex> vertices)
        {
            object lockObject = new object();
            Int64 edgeCount = 0L;
            var rangePartitioner = Partitioner.Create(0, vertices.Count);

            Parallel.ForEach(
                rangePartitioner,
                () => 0L,
                (range, loopstate, initialValue) =>
                {
                    Int64 localCount = initialValue;

                    for (int i = range.Item1; i < range.Item2; i++)
                    {
                        foreach (var aOutGoingEdge in vertices[i].GetAllOutgoingHyperEdges())
                        {
                            foreach (var aInnerVertex in aOutGoingEdge.Item2.GetTargetVertices())
                            {
                                localCount++;
                            }
                        }
                    }

                    return localCount;

                },
                (localSum) =>
                {
                    lock (lockObject)
                    {
                        edgeCount += localSum;
                    }
                });

            return edgeCount;
        }

        #endregion

        #region IPluginable Members

        public IPluginable InitializePlugin(string UniqueString, Dictionary<string, object> myParameters = null)
        {
            String interestingVertexType = "User";
            if (myParameters != null && myParameters.ContainsKey("vertexTypeName"))
                interestingVertexType = (String)Convert.ChangeType(myParameters["vertexTypeName"], typeof(String));

            return new VTPS_PartitionedParallel(interestingVertexType);
        }

        public string PluginName
        {
            get { return "VTPS_PartitionedParallel"; }
        }

        public PluginParameters<Type> SetableParameters
        {
            get
            {
                return new PluginParameters<Type>()
				{
					{"vertexTypeName", 	typeof(String)},
				};
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