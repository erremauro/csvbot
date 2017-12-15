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
using CsvBot.Properties;

namespace CsvBot.Workers
{
    /// <summary>
    /// A Worker that applies File Strategies to a file
    /// </summary>
    internal class FileStrategyWorker
    {
        private readonly List<FileStrategy> m_Strategies;

        /// <summary>
        /// Initialize the worker with a list of file strategies
        /// </summary>
        /// <param name="strategies"></param>
        public FileStrategyWorker(List<FileStrategy> strategies)
        {
            m_Strategies = strategies;
        }

        /// <summary>
        /// Apply each FileStrategy to the directive's parsed <paramref name="content"/>
        /// from the source <paramref name="filepath"/>. 
        /// 
        /// An optional list of comma separated extensions provided by the 
        /// <paramref name="groupedWith"/> parameter will be matched to the filename and 
        /// copied or moved along the source file if a FileStrategy has no "with" attribute.
        /// 
        /// For example, if the source filename is:"test.csv" and "txt,xlsx" is provided
        /// as <paramref name="groupedWith"/>, the method will search for matching
        /// "test.txt" and "test.xlsx" filenames and apply the FileStrategy to them too.
        /// </summary>
        /// <param name="filepath">Source file path</param>
        /// <param name="content">Directive parsed content</param>
        /// <param name="groupedWith">Comma separate list of extensions</param>
        public void Execute(string filepath, string content, string groupedWith = null)
        {
            Logger.Info(@"Applying File Strategies to ""{0}""", filepath);

            var sourceDirectory = Path.GetDirectoryName(filepath);
            if (sourceDirectory == null)
            {
                throw new Exception(
                    String.Format(
                        @"FileStrategy failure. Unable to retrieve source directory. Invalid source path ""{0}""",
                        filepath));
            }

            // If no File Strategies has been provided, just save the file.
            if (m_Strategies.Count == 0)
            {
                try
                {
                    Logger.Info(@"No File Strategies specified. Saving file in place.");
                    File.WriteAllText(filepath, content);
                }
                catch (Exception)
                {
                    throw new Exception(
                        String.Format(
                            "FileStrategy failure. Unable to save {0}. The path could be unaccessible or the file is in use.",
                            filepath));
                }

                return;
            }

            // check if there's a Move strategy, so we will delete the source copy 
            // at the end of the process, after all the file copies has been done.
            var willBeMoved = m_Strategies.Count(q => q.Type.Equals("move")) > 0;

            foreach (var strategy in m_Strategies)
            {
                #region Apply FileStrategy
                // assign groupedWith to strategy if the strategy doesn't have
                // a custom list of file extensions.
                strategy.With = strategy.With ?? groupedWith;

                if (strategy.Type != "move" && strategy.Type != "copy") continue;

                var filename = Path.GetFileName(filepath);
                var destinationDirectory = strategy.Path;
                var targetPath = Path.Combine(destinationDirectory, filename);

                // Manage strategy's "overwrite" definition by deleting, ignoring
                // or throw an error for an existing target file.
                var shouldSkipOverwrite = ManageOverwrite(targetPath, strategy);
                if (shouldSkipOverwrite) continue;

                try
                {
                    File.WriteAllText(targetPath, content);
                }
                catch (Exception)
                {
                    throw new Exception(
                        String.Format(
                            "FileStrategy failure. Unable to {0} {1} to {2}. The path could unaccessible or the file is in use.",
                            strategy.Type, filepath, strategy.Path));
                }

                // applies the strategy to a comma separated list of file extension.
                // if the strategy had no "With" property to begin with, the Execute's "groupedWith"
                // parameter should be already initialized here. See "strategy.With" assignment above.
                if (strategy.With != null)
                {
                    ManageGroupedFiles(filename, sourceDirectory, destinationDirectory, strategy);
                }
                #endregion
            }

            if (willBeMoved)
            {
                File.Delete(filepath);
            }
        }

        /// <summary>
        /// Manage <paramref name="strategy"/>'s "overwrite" definition by deleting,
        /// ignoring or throwing an error for an existing <paramref name="destination"/> file.
        /// </summary>
        /// <param name="destination">Destination file</param>
        /// <param name="strategy">Applied strategy</param>
        /// <returns>True if the overwrite should be skipped, otherwise false</returns>
        private static bool ManageOverwrite(string destination, FileStrategy strategy)
        {
            if (!File.Exists(destination)) return false;

            if (strategy.Overwrite == "true")
            {
                try
                {
                    File.Delete(destination);
                }
                catch (Exception)
                {
                    throw new Exception(
                        String.Format(
                            "FileStrategy failure. Unable to overwrite {0}. The path could be unaccessible or the file is in use.",
                            destination));
                }

                return false;
            }

            if (strategy.Overwrite == "false")
            {
                throw new Exception(
                    String.Format(
                        @"FileStrategy failure. The file at path {0} already exists. Remove the file or change the ""{1}"" strategy attribute for ""{2}"" element.",
                        destination, Resources.OverwriteAttribute, strategy.Type));
            }

            return strategy.Overwrite == "skip" || strategy.Overwrite == "continue";
        }

        private static void ManageGroupedFiles(string filename, string sourceDir, string destinationDir,
            FileStrategy strategy)
        {
            var filenameNoExt = Path.GetFileNameWithoutExtension(filename);
            var sourceFiles = strategy.With.Split(',').Select(q =>
                String.Format(@"{0}.{1}", filenameNoExt, q.Trim()));

            foreach (var sourceFile in sourceFiles)
            {
                var source = Path.Combine(sourceDir, sourceFile);
                if (!File.Exists(source)) continue;

                var destination = Path.Combine(destinationDir, sourceFile);

                // Manage strategy's "overwrite" definition by deleting, ignoring
                // or throw an error for an existing target file.
                var shouldSkip = ManageOverwrite(destination, strategy);
                if (shouldSkip) continue;

                try
                {
                    switch (strategy.Type)
                    {
                        case "move":
                            File.Move(source, destination);
                            break;
                        case "copy":
                            File.Copy(source, destination);
                            break;
                    }
                }
                catch (Exception)
                {
                    throw new Exception(
                        String.Format(
                            @"FileStrategy failure. Unable to {0} related file ""{1}"" to ""{2}"". The path could be unaccessible or the source file is open.",
                            strategy.Type, source, destinationDir));
                }
            }
        }
    }
}