#region Copyright (c) 2017, Roberto Mauro

// Copyright (c) 2017, Roberto Mauro
// This source code is licensed under the MIT license found in the
// LICENSE file in the root directory of this source tree.

#endregion

using System;
using System.IO;
using CsvBot.Models;
using CsvBot.Workers;

namespace CsvBot
{
    internal class Program
    {
        private static void Main(string[] args)
        {
            Logger.Info("Program started.");

            AppConfig appConfig = null;

            try
            {
                Logger.Info("Reading configuration file");
                appConfig = AppConfigManager.GetConfig();
            }
            catch (Exception err)
            {
                Logger.Error(err.Message);
                Console.WriteLine(@"error: {0}", err.Message);
                Environment.Exit(1);
            }

            string[] sourceFiles;
            FileAttributes fileAttributes = 0;
            var directiveWorker = new DirectivesWorker(appConfig.Directives, appConfig.CsvSeparator);
            var strategyWorker = new FileStrategyWorker(appConfig.FileStrategies);
            
            try
            {
                Logger.Info(@"Checking Source path at {0}", appConfig.SourcePath);
                fileAttributes = File.GetAttributes(appConfig.SourcePath);
            }
            catch (Exception)
            {
                var errorMessage =
                    String.Format(
                        @"Unable to access Source path ""{0}"". The path could be unaccesible due to user permissions or not exists",
                        appConfig.SourcePath);

                Logger.Error(errorMessage);
                Console.WriteLine(@"error: {0}", errorMessage);
                Environment.Exit(1);
            }

            if ((fileAttributes & FileAttributes.Directory) == FileAttributes.Directory)
            {
                sourceFiles = Directory.GetFiles(appConfig.SourcePath, "*.csv");
                if (sourceFiles.Length == 0)
                {
                    var message = String.Format(@"No files to be processed found in {0}", appConfig.SourcePath);
                    Logger.Info(message);
                    Console.WriteLine(message);
                    Environment.Exit(0);
                }
            }
            else
            {
                sourceFiles = new[] {appConfig.SourcePath};
            }

            try
            {
                foreach (var file in sourceFiles)
                {
                    var content = directiveWorker.Execute(file);
                    strategyWorker.Execute(file, content, appConfig.GroupedWith);
                }
            }
            catch (Exception err)
            {
                Logger.Error(err.Message);
                Console.WriteLine(@"error: {0}", err.Message);
                Environment.Exit(1);
            }

            Console.WriteLine(@"Done.");
            Logger.Success("Completed.");
            Environment.Exit(0);
        }
    }
}