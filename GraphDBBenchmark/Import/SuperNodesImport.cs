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
using System.Threading.Tasks;
using System.Collections.Concurrent;
using sones.GraphQL.Result;

namespace sones.GraphDBBenchmark.Import
{
    public sealed class SuperNodesImport : IImport
    {
        #region data

        private long _countOfUsers;

        private String _vtEntity 		= "Entity";
        private String _vtPlace 		= "Place";
        private String _vtUser 			= "User";
        private String _vtCountry 		= "Country";
        private String _vtCity 			= "City";

        private String _pName 			= "Name";
        private String _pHasVisited 	= "HasVisited";
        private String _pCities 		= "Cities";
        //private String _pIsInCountry 	= "IsInCountry";
        private String _pInCountry 		= "InCountry";


        #endregion

        #region constructors

        /// <summary>
        /// Due to IPLuginable
        /// </summary>
        public SuperNodesImport()
        {

        }

        public SuperNodesImport(long myCountOfUsers)
        {
            _countOfUsers = myCountOfUsers;
        }

        #endregion

        #region private helper

        public void SocialNetwork(IGraphDS myGraphDS)
        {
            #region ontology [API]

            var entityTypeDefinition = new VertexTypePredefinition(_vtEntity)
                        .AddProperty(new PropertyPredefinition(_pName).SetAttributeType(typeof(String).Name));

            var userTypeDefinition = 
				new VertexTypePredefinition(_vtUser)
                        .SetSuperVertexTypeName(_vtEntity)
                        .AddOutgoingEdge(new OutgoingEdgePredefinition(_pHasVisited).SetAttributeType(_vtCity).SetMultiplicityAsMultiEdge());

            var placeTypeDefinition = 
				new VertexTypePredefinition(_vtPlace)
                        .SetSuperVertexTypeName(_vtEntity);

            var countryTypeDefinition = 
				new VertexTypePredefinition(_vtCountry)
                        .SetSuperVertexTypeName(_vtPlace)
                        .AddIncomingEdge(new IncomingEdgePredefinition(_pCities).SetOutgoingEdge(_vtCity, _pInCountry));

            var cityTypeDefinition = 
				new VertexTypePredefinition(_vtCity)
                        .SetSuperVertexTypeName(_vtPlace)
                        .AddOutgoingEdge(new OutgoingEdgePredefinition(_pInCountry).SetAttributeType(_vtCountry));

            Dictionary<String, IVertexType> vertexTypes = myGraphDS.CreateVertexTypes(null, null,
                new RequestCreateVertexTypes(new List<VertexTypePredefinition> 
					{ 	entityTypeDefinition, 
						userTypeDefinition, 
						placeTypeDefinition,
						countryTypeDefinition,
						cityTypeDefinition
					}),
                (stats, types) => types.ToDictionary(vType => vType.Name, vType => vType));

            #endregion

            #region country [GQL]
            ExecuteQuery("insert into " + _vtCountry + " values ( " + _pName + " = 'UK' )", myGraphDS);

            #endregion

            #region cities [GQL]

            var cityVertexIDs = new List<long>();
            foreach (var aCity in new List<String> { "London", "Manchester", "Edinburgh", "Cambridge", "Oxford" })
            {
                cityVertexIDs.Add(ExecuteQuery("insert into " + _vtCity + " values ( " + _pName + " = '" + aCity + "', " + _pInCountry + " = REF(" + _pName + " = 'UK'))", myGraphDS).First().GetProperty<long>("VertexID"));
            }

            #endregion

            #region user [API]

            var userType = vertexTypes[_vtUser];
            var cityType = vertexTypes[_vtCity];

            Parallel.ForEach(
                Partitioner.Create(0, _countOfUsers, _countOfUsers / Environment.ProcessorCount),
                range =>
                {
                    for (long i = range.Item1; i < range.Item2; i++)
                    {
                        CreateANewUser(userType, i, myGraphDS, cityVertexIDs, cityType);
                    }
                });

            #endregion
        }

        public QueryResult ExecuteQuery(string myQuery, IGraphDS myGraphDS)
        {
            return myGraphDS.Query(null, null, myQuery, "sones.gql");
        }

        void CreateANewUser(IVertexType myUsertype, long myCounter, IGraphDS myGraphDS, List<long> myCityVertexIDs, IVertexType myCityType)
        {
            myGraphDS.Insert<long>(null, null,
                    new RequestInsertVertex(_vtUser)
                        .AddStructuredProperty(_pName, "User" + myCounter)
                        .AddEdge(new EdgePredefinition(_pHasVisited).AddVertexID(_vtCity, myCityVertexIDs)),
                    GetVertexID);

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
            long countOfUsers = 100000;
            if (myParameters != null && myParameters.ContainsKey("countOfUsers"))
                countOfUsers = (Int64)Convert.ChangeType(myParameters["countOfUsers"], typeof(Int64));

            return new SuperNodesImport(countOfUsers);
        }

        public string PluginName
        {
            get { return "superNodes"; }
        }

        public PluginParameters<Type> SetableParameters
        {
            get
            {
                return new PluginParameters<Type>()
				{
					{"countOfUsers", 	typeof(long)}
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

