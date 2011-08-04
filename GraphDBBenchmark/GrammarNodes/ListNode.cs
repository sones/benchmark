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
using sones.GraphDBBenchmark.Import;
using System.Text;

namespace sones.GraphDBBenchmark.Node
{
    public sealed class ListNode : ABenchmarkGrammarNode
    {
        private String _availablePlugins = String.Empty;

        public void Init(ParsingContext context, ParseTreeNode parseNode, IGraphDS myGraphDS)
        {
            base.InitNode(context, parseNode, myGraphDS);
            _pluginManager.Discover();

            var importPlugins = _pluginManager.GetPluginNameForType<IImport>();
            var benchmarkPlugins = _pluginManager.GetPluginNameForType<IBenchmark>();

            StringBuilder sb = new StringBuilder();

            FindPlugins<IImport>(sb);
            FindPlugins<IBenchmark>(sb);

            _availablePlugins = sb.ToString();
        }

        public override void EvaluateNode(EvaluationContext context, AstMode mode)
        {
            context.Data.Push(_availablePlugins);
        }

        private void FindPlugins<T>(StringBuilder sb)
        {
            var plugins = _pluginManager.GetPluginNameForType<T>();
            var typeName = typeof(T).Name;

            sb.AppendLine(String.Format("Available {0} plugins:", typeName));
            if (plugins.Count() == 0)
            {
                sb.AppendLine(String.Format("  no {0} plugins available", typeName));
            }
            else
            {
                foreach (var aPlugin in plugins)
                {
                    sb.AppendLine(String.Format("  {0}", aPlugin));
                }
            }
        }
    }
}

