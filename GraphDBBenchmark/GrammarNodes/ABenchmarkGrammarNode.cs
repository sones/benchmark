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
using sones.GraphDBBenchmark.Plugin;
using Irony.Parsing;
using System.Collections.Generic;
using System.Text;
using Irony.Ast;

namespace sones.GraphDBBenchmark.Node
{
	public abstract class ABenchmarkGrammarNode  : AstNode
	{
		protected IGraphDS _graphDS;
		protected BenchmarkPluginManager _pluginManager;
		protected String _componentName;
		
		protected void InitNode (ParsingContext context, ParseTreeNode parseNode, IGraphDS myGraphDS)
		{
			base.Init(context, parseNode);
			_graphDS = myGraphDS;
			_pluginManager = new BenchmarkPluginManager();
		}
		
		protected Dictionary<String, Object> PreparePluginOptions(Dictionary<String, String> options, IGraphDS myGraphDS)
		{
			Dictionary<String, Object> pluginOptions = new Dictionary<String, Object>();
			foreach (var aOption in options)
			{
				pluginOptions.Add(aOption.Key, aOption.Value);
			}
			pluginOptions.Add("GraphDS", _graphDS);
			
			return pluginOptions;
		}
		
		protected bool CheckForComponent<T>(String myComponentName, ParsingContext context, SourceLocation myLocation)
		{
			if (!_pluginManager.HasPlugin<T>(myComponentName)) 
			{
				String typeName = typeof(T).Name;
				StringBuilder sb = new StringBuilder();
      			
				var plugins = _pluginManager.GetPluginNameForType<T>();
				
				if (plugins.Count() > 0) 
				{
					sb.AppendLine(String.Format("Could not find {0} plugin {1}.", typeName, _componentName));
					
					sb.AppendLine(String.Format("Available {0} plugins are:", typeName));
						
					foreach (var aImportPlugin in plugins)
					{
						sb.AppendLine(aImportPlugin);
					}
				}
				else 
				{
					sb.AppendLine(String.Format("There are no {0} plugins available. Insert a {1} plugin.", typeName, typeName));
				}
				
				context.AddParserMessage(ParserErrorLevel.Error, myLocation, sb.ToString());
				
				return false;
			}
			
			return true;
		}
	}
}

