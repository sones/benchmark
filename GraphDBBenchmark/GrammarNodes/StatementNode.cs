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
	public sealed class StatementNode : ABenchmarkGrammarNode
	{
		public override void Init(ParsingContext context, ParseTreeNode parseNode)
		{
            base.Init(context, parseNode);

            foreach (var aChild in parseNode.ChildNodes)
            {
                if (aChild.AstNode != null)
                {
                    AddChild(string.Empty, aChild);
                }
            }
		}
		
		public override void EvaluateNode(EvaluationContext context, AstMode mode)
		{
            if (ChildNodes.Count == 0)
            {
                return;
            }

            StringBuilder sb = new StringBuilder();

            foreach (var aChild in ChildNodes)
            {
                aChild.Evaluate(context, AstMode.Read);

                for (int i = 0; i < context.Data.Count; i++)
			    {
                    sb.AppendLine(context.Data.Top.ToString());
                    context.Data.Pop();
			    }
            }

            context.Data.Push(sb.ToString());
    	}
	}
}

