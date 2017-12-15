#region Copyright (c) 2017, Roberto Mauro

// Copyright (c) 2017, Roberto Mauro
// This source code is licensed under the MIT license found in the
// LICENSE file in the root directory of this source tree.

#endregion

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using CsvBot.Models;

namespace CsvBot.Workers
{
    /// <summary>
    /// A worker that applies all directives to a csv file
    /// </summary>
    internal class DirectivesWorker
    {
        private readonly List<Directive> m_Directives;
        private readonly string m_Separator = ";";

        /// <summary>
        /// Initialize the worker with a list of <paramref name="directives"/>
        /// </summary>
        /// <param name="directives"></param>
        public DirectivesWorker(List<Directive> directives)
        {
            m_Directives = directives;
        }

        /// <summary>
        /// Initialize the worker with a list of <paramref name="directives"/>
        /// and a csv <paramref name="separator"/>
        /// </summary>
        /// <param name="directives">A list of directives to be applied</param>
        /// <param name="separator">A csv separator</param>
        public DirectivesWorker(List<Directive> directives, string separator)
        {
            m_Directives = directives;
            m_Separator = separator;
        }

        /// <summary>
        /// Applies all Directive to a given <paramref name="filepath"/> content
        /// </summary>
        /// <param name="filepath">CSV file</param>
        /// <returns></returns>
        public string Execute(string filepath)
        {
            if (m_Directives.Count == 0)
            {
                Logger.Warn(@"No directives to be applied");
                return File.ReadAllText(filepath);
            }
            
            Logger.Info(@"Applying directives to file {0}", filepath);

            var lineCounter = 0;
            var contentLines = new List<string>();

            try
            {
                // read every lines in the file and split its content by the csv separator.
                // it then apply each Directive to every column for every line.
                using (var reader = new StreamReader(filepath))
                {
                    while (!reader.EndOfStream)
                    {
                        var line = reader.ReadLine();
                        if (line == null) continue;

                        var columns = line.Split(m_Separator[0]);

                        var arrangedColumns = m_Directives.Aggregate(columns,
                            (current, directive) => ApplyDirective(lineCounter, current, directive));

                        // Reconstruct the csv line after the Directive has been applied
                        contentLines.Add(String.Join(m_Separator, arrangedColumns));
                        lineCounter++;
                    }
                }
            }
            catch (Exception)
            {
                throw new Exception(
                    String.Format(
                        @"Unable to open ""{0}"". The file could be unaccessible, corrupted or already open.", filepath));
            }

            // Reconstruct the csv file content
            return String.Join(Environment.NewLine, contentLines.ToArray());
        }

        private static string[] ApplyDirective(int currentLine, IList<string> columns, Directive directive)
        {
            // prepare a copy of the original columns that will be returned
            // after a strategy as been applied.
            var arrangedColumns = new List<string>(columns);
            // convert the column position specified in the configurations, 
            // to the actual array index.
            var positionIndex = directive.Position - 1;

            // If a rename value exists, rename a csv header
            if (currentLine == 0 && !String.IsNullOrEmpty(directive.Rename))
            {
                // we directly rename the resulting column's header if no action is provided,
                // otherwise rename the original column header before applying an action.
                if (String.IsNullOrEmpty(directive.Action) || directive.Action == "none")
                {
                    arrangedColumns[positionIndex] = directive.Rename;
                }
                else
                {
                    columns[positionIndex] = directive.Rename;
                }
            }

            // no action provided, return the columns as they are without
            // further modifications.
            if (directive.Action != "move" && directive.Action != "copy")
            {
                return arrangedColumns.ToArray();
            }

            // convert the column destination position specified 
            // in the configurations to the actual array index.
            var toIndex = directive.To - 1;
            var content = columns[positionIndex];

            // we need to remove the original column before is moved to a new position.
            // otherwise the positionIndex will change.
            if (directive.Action == "move")
            {
                arrangedColumns.RemoveAt(positionIndex);
            }

            arrangedColumns.Insert(toIndex, content);

            return arrangedColumns.ToArray();
        }
    }
}