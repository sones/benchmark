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
using sones.GraphDB.Request;

namespace sones.GraphDBBenchmark.Node
{
	public sealed class ClearNode : ABenchmarkGrammarNode
	{
		public void Init (ParsingContext context, ParseTreeNode parseNode, IGraphDS myGraphDS)
		{
			base.InitNode(context, parseNode, myGraphDS);
		}
		
		public override void EvaluateNode(EvaluationContext context, AstMode mode)
		{
            context.Data.Push(String.Format("Cleared the GraphDB in {0} seconds.", _graphDS.Clear<double>(null, null, new RequestClear(), (_, __) => _.ExecutionTime.TotalSeconds)));
    	}
	}
}

