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
	
	public sealed class BenchmarkGrammar : Grammar
	{
		
		#region data
		
		private IGraphDS _graphDS;
		
		#endregion
		
		#region Constructor
		
		public BenchmarkGrammar(IGraphDS myGraphDS, ushort myListeningPort, string myUsername, string myPassword)
			:base(false)
		{
			#region data
			
			_graphDS = myGraphDS;
			
			#endregion
			
			#region grammar
			
			#region Terminals
            var numberLiteral = new NumberLiteral("number", NumberOptions.AllowSign | NumberOptions.DisableQuickParse);
      		var stringLiteral = new StringLiteral("string", "'", StringOptions.AllowsDoubledQuote | StringOptions.AllowsLineBreak);
			var identifier = new IdentifierTerminal("identifier", "ÄÖÜäöüß0123456789_", "ÄÖÜäöü0123456789$_");
			var S_IMPORT = ToTerm("IMPORT");
			var S_BENCHMARK = ToTerm("BENCHMARK");
			var S_ITERATIONS = ToTerm("ITERATIONS");
			var S_EQUALS = ToTerm("=");
			var S_TRUE = ToTerm("TRUE");
			var S_FALSE = ToTerm("FALSE");
			var S_OPTIONS = ToTerm("OPTIONS");
			var S_BRACKETLEFT = ToTerm("(");
			var S_BRACKETRIGHT = ToTerm(")");
            var S_COMMA = ToTerm(",");
            var S_CLEAR = ToTerm("CLEAR");
            var S_LIST = ToTerm("LIST");

			#endregion
			
      		#region Non-terminals
			
			var NT_options = new NonTerminal("Options", CreateOptionsNode);
           	var NT_KeyValueList = new NonTerminal("ValueList", CreateKeyValueListNode);
			var NT_KeyValuePair = new NonTerminal("KeyValuePair", CreateKeyValuePairNode);
            var NT_BooleanVal = new NonTerminal("BooleanVal");
			var NT_iterations = new NonTerminal("Iterations");
            var NT_import = new NonTerminal("Import", CreateImportNode);
            var NT_clear = new NonTerminal("Clear", CreateClearNode);
            var NT_benchmark = new NonTerminal("Benchmark", CreateBenchmarkNode);
            var NT_list = new NonTerminal("List", CreateListNode);
      		var NT_Stmt = new NonTerminal("Stmt", CreateStatementNode);
			
			#endregion
			
			#region BNF rules
      		
            NT_BooleanVal.Rule = S_TRUE | S_FALSE;

            NT_KeyValuePair.Rule = 	identifier + S_EQUALS + stringLiteral
                                | 	identifier + S_EQUALS + numberLiteral
                                | 	identifier + S_EQUALS + NT_BooleanVal;

            NT_KeyValueList.Rule = MakePlusRule(NT_KeyValueList, S_COMMA, NT_KeyValuePair);

			NT_options.Rule =	Empty
                          	|   S_OPTIONS + S_BRACKETLEFT + NT_KeyValueList + S_BRACKETRIGHT;

            NT_Stmt.Rule = NT_import | NT_benchmark | NT_clear | NT_list;
			
			NT_iterations.Rule = 	Empty
						 		|	S_ITERATIONS + S_EQUALS + numberLiteral;

            NT_import.Rule = S_IMPORT + stringLiteral + NT_options;

            NT_clear.Rule = S_CLEAR;

            NT_list.Rule = S_LIST;
			
			NT_benchmark.Rule = S_BENCHMARK + stringLiteral + NT_iterations + NT_options;

      		this.Root = NT_Stmt;       // Set grammar root
			
			#endregion
			
      		#region Token filter
      		//we need to add continuation symbol to NonGrammarTerminals because it is not used anywhere in grammar
      		NonGrammarTerminals.Add(ToTerm(@"\"));
			
			#endregion
			
      		#region Initialize console attributes
  
            DiscordianDate ddate = new DiscordianDate();

			ConsoleTitle = "sones GraphDB Benchmark Console";
      		ConsoleGreeting =
@"
sones GraphDB version 2.0 - " +ddate.ToString() + @"
(C) sones GmbH 2007-2011 - http://www.sones.com
-----------------------------------------------

This GraphDB Instance offers the following options:

   * REST Service is started at http://localhost:" + myListeningPort + @"
      * access it directly by passing the GraphQL query using the
        REST interface or a client library. (see documentation)
      * if you want JSON Output add ACCEPT: application/json
        to the client request header (or application/xml or
        application/text)

   * we recommend to use the AJAX WebShell.
        Browse to http://localhost:" + myListeningPort + @"/WebShell and use
        the username """ + myUsername+ @""" and password """+ myPassword + @"""

   * Benchmark commands are:
      * IMPORT 'importPluginName' [key = value [, key = value]]
      * BENCHMARK 'benchmarkPluginName' ITERATIONS = countOfIterations [key = value [, key = value]]
      * CLEAR
      * LIST

Press Ctrl-C to exit the program at any time.
";
      		ConsolePrompt = "Benchmark> "; 
      		ConsolePromptMoreInput = "...";
      
			#endregion
			
      		this.LanguageFlags = LanguageFlags.CreateAst | LanguageFlags.CanRunSample;
            MarkPunctuation(S_BRACKETLEFT.ToString(), S_BRACKETRIGHT.ToString());

			#endregion
			
		}
		
		#endregion
		
		#region delegates
		
		public void CreateImportNode(ParsingContext context, ParseTreeNode node)
		{
			ImportNode aImportNode = new ImportNode();

            aImportNode.Init(context, node, _graphDS);

            node.AstNode = aImportNode;			
		}
		
		public void CreateBenchmarkNode(ParsingContext context, ParseTreeNode node)
		{
			BenchmarkNode aBenchmarkNode = new BenchmarkNode();

            aBenchmarkNode.Init(context, node, _graphDS);

            node.AstNode = aBenchmarkNode;
		}

        public void CreateListNode(ParsingContext context, ParseTreeNode node)
		{
            ListNode aListNode = new ListNode();

            aListNode.Init(context, node, _graphDS);

            node.AstNode = aListNode;
		}

        public void CreateStatementNode(ParsingContext context, ParseTreeNode node)
		{
            StatementNode aStatementNode = new StatementNode();

            aStatementNode.Init(context, node);

            node.AstNode = aStatementNode;
		}

        public void CreateClearNode(ParsingContext context, ParseTreeNode node)
		{
            ClearNode aClearNode = new ClearNode();

            aClearNode.Init(context, node, _graphDS);

            node.AstNode = aClearNode;
		}

		private void CreateOptionsNode(ParsingContext context, ParseTreeNode parseNode)
        {
            OptionsNode aOptionsNode = new OptionsNode();

            aOptionsNode.Init(context, parseNode);

            parseNode.AstNode = aOptionsNode;
        }
		
		private void CreateKeyValueListNode(ParsingContext context, ParseTreeNode parseNode)
        {
            KeyValueListNode aKeyValueListNode = new KeyValueListNode();

            aKeyValueListNode.Init(context, parseNode);

            parseNode.AstNode = aKeyValueListNode;
        }
		
		private void CreateKeyValuePairNode(ParsingContext context, ParseTreeNode parseNode)
        {
            KeyValuePairNode aKeyValuePairNode = new KeyValuePairNode();

            aKeyValuePairNode.Init(context, parseNode);

            parseNode.AstNode = aKeyValuePairNode;
        }
		
		#endregion
		
	}
}
