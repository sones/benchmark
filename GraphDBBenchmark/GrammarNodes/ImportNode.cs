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
using sones.GraphDS;
using Irony.Parsing;
using Irony.Interpreter;
using sones.GraphDBBenchmark.Plugin;
using sones.GraphDBBenchmark.Benchmark;
using System.Collections.Generic;
using sones.GraphDBBenchmark.Import;
using System.Diagnostics;
using System.Text;
using Irony.Ast;
using sones.GraphQL.Structure.Nodes.Misc;

namespace sones.GraphDBBenchmark.Node
{
	public sealed class ImportNode : ABenchmarkGrammarNode
	{
		private IImport _import;
		
		public void Init (ParsingContext context, ParseTreeNode parseNode, IGraphDS myGraphDS)
		{
			base.InitNode(context, parseNode, myGraphDS);
			_pluginManager.Discover();
			
			_componentName = parseNode.ChildNodes[1].Token.ValueString;
			
			if (base.CheckForComponent<IImport>(_componentName, context, parseNode.ChildNodes[1].Token.Location)) 
			{
				Dictionary<String, String> options;
				if (parseNode.ChildNodes[2].ChildNodes.Count > 0) 
				{
					options = ((OptionsNode)(parseNode.ChildNodes[2].AstNode)).Options;
				}
				else 
				{
					options = new Dictionary<String, String>();
				}
				
				_import = _pluginManager.GetAndInitializePlugin<IImport>(
					_componentName, 
					base.PreparePluginOptions(options, _graphDS));	
			}
		}
		
		public override void EvaluateNode(EvaluationContext context, AstMode mode)
		{
			if (_import != null) 
			{
				Stopwatch sw = Stopwatch.StartNew();
				_import.Execute(_ => context.Data.Push(_), _graphDS);
				sw.Stop();
			
                context.Data.Push(String.Format("Executed {0} import in {1} seconds.", _import.PluginName, sw.Elapsed.TotalSeconds)); 
			}
    	}
	}
}

