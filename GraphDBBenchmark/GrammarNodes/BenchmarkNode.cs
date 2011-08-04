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
using Irony.Parsing;
using sones.GraphDS;
using Irony.Interpreter;
using sones.Library.VersionedPluginManager;
using sones.GraphDBBenchmark.Benchmark;
using sones.GraphDBBenchmark.Plugin;
using System.Diagnostics;
using System.Collections.Generic;
using Irony.Ast;
using sones.GraphQL.Structure.Nodes.Misc;

namespace sones.GraphDBBenchmark.Node
{
	public sealed class BenchmarkNode : ABenchmarkGrammarNode
	{
		private IBenchmark _benchmark;
		private long _iterations = 1;
		
		public void Init (ParsingContext context, ParseTreeNode parseNode, IGraphDS myGraphDS)
		{
			base.InitNode(context, parseNode, myGraphDS);
			_pluginManager.Discover();
			
			_componentName = parseNode.ChildNodes[1].Token.ValueString;
			
			if (base.CheckForComponent<IBenchmark>(_componentName, context, parseNode.ChildNodes[1].Token.Location)) 
			{
				if (parseNode.ChildNodes[2].ChildNodes.Count > 0) 
				{
					try
					{
						_iterations = Convert.ToInt64(parseNode.ChildNodes[2].ChildNodes[2].Token.ValueString);
					}
					catch(Exception)
					{
                        context.AddParserMessage(ParserErrorLevel.Error, parseNode.ChildNodes[2].ChildNodes[2].Token.Location, "This is not a valid iteration count");
						return;
					}
				}
				
				Dictionary<String, String> options;
				if (parseNode.ChildNodes[3].ChildNodes.Count > 0) 
				{
					options = ((OptionsNode)(parseNode.ChildNodes[3].AstNode)).Options;
				}
				else 
				{
					options = new Dictionary<String, String>();
				}
				
				_benchmark = _pluginManager.GetAndInitializePlugin<IBenchmark>(
					_componentName, 
					base.PreparePluginOptions(options, _graphDS));
			}
		}
		
		public override void EvaluateNode(EvaluationContext context, AstMode mode)
		{
			if (_iterations < 1) 
			{
                context.Data.Push(String.Format("It's not possible to execute {0} iterations.", _iterations));
			}
			else 
			{
				Stopwatch sw = Stopwatch.StartNew();
				_benchmark.Execute(_graphDS, _iterations, _ => context.Data.Push(_));
				sw.Stop();

                context.Data.Push(String.Format("Executed the {0} benchmark in {1} seconds.", _componentName, sw.Elapsed.TotalSeconds));
			}
    	}
	}
}

