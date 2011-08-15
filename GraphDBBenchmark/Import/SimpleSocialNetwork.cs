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
using sones.GraphDBBenchmark.Import;
using sones.GraphDBBenchmark;
using sones.Library.VersionedPluginManager;
using sones.GraphDS;
using sones.GraphDB.TypeSystem;
using sones.GraphDB.Request;
using sones.Library.PropertyHyperGraph;

namespace GraphDBBenchmark.Import
{
    public sealed class SimpleSocialNetwork : IImport
    {
        #region data

        private int _countOfUsers;
        private int _minCountOfEdges;
        private int _maxCountOfEdges;

        #endregion

        #region constructors

        /// <summary>
        /// Due to IPLuginable
        /// </summary>
        public SimpleSocialNetwork()
        {

        }

        public SimpleSocialNetwork(int myCountOfUsers, int myMinCountOfEdges, int myMaxCountOfEdges)
        {
            if (myMinCountOfEdges > myMaxCountOfEdges)
            {
                throw new ArgumentOutOfRangeException("myMaxCountOfEdges", "The maximum count of edges should be greater than the minimum of edges per vertex.");
            }

            _countOfUsers = myCountOfUsers;
            _maxCountOfEdges = myMaxCountOfEdges;
            _minCountOfEdges = myMinCountOfEdges;
        }

        #endregion

        #region private helper

        public void SocialNetwork(IGraphDS myGraphDS)
        {
            Random PRNG = new Random();
            List<long> vertexIDs = new List<long>();

            //create the user type
            var usertype = myGraphDS.CreateVertexType<IVertexType>(null, null,
                new RequestCreateVertexType(
                    new VertexTypePredefinition("User")
                        .AddProperty(new PropertyPredefinition("Name").SetAttributeType("String"))
                        .AddProperty(new PropertyPredefinition("Age").SetAttributeType("Int32"))
                        .AddIndex(new IndexPredefinition("MyAgeIndex").SetIndexType("MultipleValueIndex").AddProperty("Age").SetVertexType("User"))
                        .AddOutgoingEdge(new OutgoingEdgePredefinition("Friends").SetAttributeType("User").SetMultiplicityAsMultiEdge())
                ),
                (stats, vType) => vType);

            Console.Write("Imported User: ");

            for (long i = 0; i < _countOfUsers; i++)
            {
                if (i % 1000 == 0)
                {
                    String iString = i.ToString();
                    Console.Write(iString);
                    Console.CursorLeft -= iString.Length;
                }

                vertexIDs.Add(CreateANewUser(usertype, i, vertexIDs, PRNG, myGraphDS));
            }
        }

        long CreateANewUser(IVertexType usertype, long i, List<long> recentVertexIDs, Random myPRNG, IGraphDS myGraphDS)
        {
            if (recentVertexIDs.Count == 0)
            {
                return myGraphDS.Insert<long>(null, null,
                    new RequestInsertVertex("User")
                        .AddStructuredProperty("Name", "User" + i)
                        .AddStructuredProperty("Age", myPRNG.Next(18, 90)),
                    GetVertexID);
            }
            else
            {
                return myGraphDS.Insert<long>(null, null,
                    new RequestInsertVertex("User")
                        .AddStructuredProperty("Name", "User" + i)
                        .AddStructuredProperty("Age", myPRNG.Next(18, 90))
                        .AddEdge(CreateEdge(recentVertexIDs, myPRNG, usertype.ID)),
                    GetVertexID);
            }
        }


        EdgePredefinition CreateEdge(List<long> recentVertexIDs, Random myPRNG, long vertexTypeID)
        {
            var result = new EdgePredefinition("Friends");

            if (_maxCountOfEdges > recentVertexIDs.Count)
            {
                AddSingleEdges(result, recentVertexIDs, vertexTypeID);
            }
            else
            {
                HashSet<long> destinationVertexIDs = new HashSet<long>();
                var countEdges = myPRNG.Next(_minCountOfEdges, _maxCountOfEdges);

                do
                {
                    destinationVertexIDs.Add(recentVertexIDs[myPRNG.Next(0, recentVertexIDs.Count)]);
                } while (destinationVertexIDs.Count < countEdges);

                AddSingleEdges(result, destinationVertexIDs, vertexTypeID);
            }

            return result;

        }

        void AddSingleEdges(EdgePredefinition result, IEnumerable<long> destinationVertexIDs, long vertexTypeID)
        {
            foreach (var aDestinationID in destinationVertexIDs)
            {
                result.AddVertexID(vertexTypeID, aDestinationID);
            }
        }

        long GetVertexID(IRequestStatistics myStats, IVertex myVertex)
        {
            return myVertex.VertexID;
        }

        #endregion


        #region IImport Members

        public void Execute(sones.GraphDBBenchmark.Converter.WriteLineToConsole MyWriteLine, IGraphDS myGraphDS)
        {
            SocialNetwork(myGraphDS);
        }

        #endregion

        #region IPluginable Members

        public IPluginable InitializePlugin(string UniqueString, Dictionary<string, object> myParameters = null)
        {
            int countOfUsers = 100000;
            if (myParameters != null && myParameters.ContainsKey("countOfUsers"))
                countOfUsers = (Int32)Convert.ChangeType(myParameters["countOfUsers"], typeof(Int32));

            int countOfMinEdges = 20;
            if (myParameters != null && myParameters.ContainsKey("minCountOfEdges"))
                countOfMinEdges = (Int32)Convert.ChangeType(myParameters["minCountOfEdges"], typeof(Int32));

            int countOfMaxEdges = 30;
            if (myParameters != null && myParameters.ContainsKey("maxCountOfEdges"))
                countOfMaxEdges = (Int32)Convert.ChangeType(myParameters["maxCountOfEdges"], typeof(Int32));

            return new SimpleSocialNetwork(countOfUsers, countOfMinEdges, countOfMaxEdges);
        }

        public string PluginName
        {
            get { return "simpleNetwork"; }
        }

        public PluginParameters<Type> SetableParameters
        {
            get
            {
                return new PluginParameters<Type>()
				{
					{"countOfUsers", 	typeof(int)},
					{"minCountOfEdges", typeof(int)},
					{"maxCountOfEdges", typeof(int)}
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
