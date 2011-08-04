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
using sones.GraphDS.PluginManager;
using sones.GraphDSServer;
using System.Threading;
using sones.GraphDB;
using System.Globalization;
using System.Collections.Generic;
using sones.Library.VersionedPluginManager;
using System.Net;
using sones.Library.Commons.Security;
using sones.GraphDS;
using sones.GraphDBBenchmark.Node;
using sones.GraphQL.Structure.Nodes.Misc;
using Irony.Parsing;
using Irony.Interpreter;
using sones.Library.DiscordianDate;

namespace sones.GraphDBBenchmark
{
	public class Program
	{
		public static void Main (string[] args)
		{
            try
            {
                var benchmarkStartup = new BenchmarkStartup();
            }
            catch (Exception e)
            {
				Console.WriteLine(e.Message);
                Console.WriteLine();
                Console.WriteLine("Press <return> to exit.");
                Console.ReadLine();
			}
		}
	}
	
	public class BenchmarkStartup
	{
		private GraphDS_Server _dsServer;
		private String _culture = "en-us";
		private UInt16 _listeningPort = 9975;
		private String _userName = "test";
		private String _password = "test";
		
		public BenchmarkStartup()
		{
			Thread.CurrentThread.CurrentCulture = System.Globalization.CultureInfo.GetCultureInfo(_culture);

            #region Start REST, WebDAV and WebAdmin services, send GraphDS notification

            var GraphDB = new SonesGraphDB(null, true, new CultureInfo(_culture));

            #region Configure PlugIns
            // Plugins are loaded by the GraphDS with their according PluginDefinition and only if they are listed
            // below - there is no auto-discovery for plugin types in GraphDS (!)

            #region Query Languages
            // the GQL Query Language Plugin needs the GraphDB instance as a parameter
            List<PluginDefinition> QueryLanguages = new List<PluginDefinition>();
            Dictionary<string, object> GQL_Parameters = new Dictionary<string, object>();
            GQL_Parameters.Add("GraphDB", GraphDB);

            QueryLanguages.Add(new PluginDefinition("sones.gql", GQL_Parameters));
            #endregion

            #region REST Service Plugins
            List<PluginDefinition> SonesRESTServices = new List<PluginDefinition>();
            
            #endregion
        
            #endregion

            GraphDSPlugins PluginsAndParameters = new GraphDSPlugins(SonesRESTServices,QueryLanguages);

            _dsServer = new GraphDS_Server(GraphDB, _listeningPort, _userName, _password, IPAddress.Any, PluginsAndParameters);
            _dsServer.LogOn(new UserPasswordCredentials(_userName, _password));

            _dsServer.StartRESTService("", _listeningPort, IPAddress.Any);

            #endregion
			
			#region start grammar and console
			
			Grammar benchmarkGrammar = new BenchmarkGrammar(_dsServer, _listeningPort, _userName, _password);
			
			var commandLine = new CommandLine(benchmarkGrammar);
      		commandLine.Run();
			
			#endregion
			
            _dsServer.Shutdown(null);
		}
	}
}
