#region Copyright (c) 2017, Roberto Mauro

// Copyright (c) 2017, Roberto Mauro
// This source code is licensed under the MIT license found in the
// LICENSE file in the root directory of this source tree.

#endregion

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml.Linq;
using CsvBot.Models;
using CsvBot.Properties;

namespace CsvBot
{
    /// <summary>
    /// Construct the AppConfig object by reading and initializing if necessary
    /// the application config file.
    /// </summary>
    internal static class AppConfigManager
    {
        private static string m_ConfigPath;

        public static string ConfigPath
        {
            get
            {
                return m_ConfigPath ??
                       (m_ConfigPath = Path.Combine(Utils.ExecutingAssemblyPath(), Resources.ConfigFileName));
            }
        }

        /// <summary>
        /// Get the App configurations for the workers
        /// </summary>
        /// <returns>The App configuration object</returns>
        public static AppConfig GetConfig()
        {
            Initialize();

            XDocument xmlDoc;

            try
            {
                xmlDoc = XDocument.Load(ConfigPath);
            }
            catch (Exception err)
            {
                throw new Exception(String.Format(@"Configuration file ""{0}"" not properly formatted. {1}", ConfigPath,
                    err.Message));
            }


            var xmlRoot = xmlDoc.Element(Resources.ConfigElement);

            if (xmlRoot == null)
            {
                throw new Exception(String.Format(@"Missing root element ""{0}"" in ""{1}""",
                    Resources.ConfigElement, ConfigPath));
            }

            return new AppConfig
            {
                SourcePath = GetSourcePath(xmlRoot),
                Directives = GetDirectives(xmlRoot),
                FileStrategies = GetFileStrategies(xmlRoot),
                CsvSeparator = GetSeparator(xmlRoot),
                GroupedWith = GetGroupedWith(xmlRoot)
            };
        }

        /// <summary>
        /// Initialize a new configuration file if
        /// no configuration file is found in the assembly directory.
        /// </summary>
        public static void Initialize()
        {
            if (File.Exists(ConfigPath)) return;

            try
            {
                File.WriteAllText(ConfigPath, Resources.config);
                var message =
                    String.Format(
                        @"Configuration file not found. A new configuration file has been created at path ""{0}"". You can now set your configurations.",
                        ConfigPath);
                Logger.Warn(message);
                Console.WriteLine(message);
                Environment.Exit(0);
            }
            catch (IOException)
            {
                throw new Exception(
                    String.Format(
                        @"Unable to initialize configuration file at path ""{0}"". The path could be unaccessible.",
                        ConfigPath)
                    );
            }
        }

        private static string GetSourcePath(XContainer root)
        {
            var xSource = root.Element(Resources.SourceElement);

            if (xSource == null)
            {
                throw new Exception(String.Format(@"Missing ""{0}"" element in ""{1}""", Resources.SourceElement,
                    ConfigPath));
            }

            var xSourcePath = xSource.Attribute(Resources.PathAttribute);

            if (xSourcePath == null || String.IsNullOrEmpty(xSourcePath.Value))
            {
                throw new Exception(String.Format(@"Missing {0}'s ""{1}"" attribute in ""{2}""", Resources.SourceElement,
                    Resources.PathAttribute, ConfigPath));
            }

            return xSourcePath.Value;
        }

        private static string GetGroupedWith(XContainer root)
        {
            var xGroupedWith = root.Element(Resources.GroupedWithElement);
            return xGroupedWith == null ? null : xGroupedWith.Value;
        }

        private static List<FileStrategy> GetFileStrategies(XContainer root)
        {
            var fileStrategy = new List<FileStrategy>();
            var xFileStrategy = root.Element(Resources.FileStrategyElement);

            if (xFileStrategy == null)
            {
                throw new Exception(String.Format(@"No {0} element found in ""{1}""", Resources.FileStrategyElement,
                    ConfigPath));
            }

            foreach (var strategy in xFileStrategy.Elements())
            {
                var strategyType = strategy.Name.ToString().ToLowerInvariant();
                var xPath = strategy.Attribute(Resources.PathAttribute);
                var xOverwrite = strategy.Attribute(Resources.OverwriteAttribute);

                // Path is a required attribute so we immediately check for its existance.
                if (xPath == null)
                {
                    throw new Exception(String.Format(@"Missing {0}'s ""{1}"" attribute in ""{2}""",
                        strategyType, Resources.PathAttribute, ConfigPath));
                }

                // "overwrite" attribute can only have values of true, false, skip or continue
                var overwrite = xOverwrite == null ? "true" : xOverwrite.Value.ToLowerInvariant();
                var validOverwrites = new[] {"true", "false", "skip", "continue"};
                if (!validOverwrites.Contains(overwrite))
                {
                    throw new Exception(
                        String.Format(@"Invalid strategy attribute ""{0}"" for ""{1}"". Accepted values are: {2}",
                            Resources.OverwriteAttribute, strategy.Name, String.Join(", ", validOverwrites)));
                }

                var xWith = strategy.Attribute(Resources.WithAttribute);
                var with = xWith == null ? null : xWith.Value;

                fileStrategy.Add(new FileStrategy
                {
                    Type = strategyType,
                    Path = xPath.Value,
                    Overwrite = overwrite,
                    With = with
                });
            }

            return fileStrategy;
        }

        private static List<Directive> GetDirectives(XContainer root)
        {
            var directives = new List<Directive>();
            var xDirectives = root.Element(Resources.DirectivesElement);

            if (xDirectives == null)
            {
                throw new Exception(String.Format(@"No {0} element found in ""{1}""", Resources.DirectivesElement,
                    ConfigPath));
            }

            foreach (var xDirective in xDirectives.Elements())
            {
                var xName = xDirective.Attribute(Resources.NameAttribute);
                var xTo = xDirective.Attribute(Resources.ToAttribute);
                var xPosition = xDirective.Attribute(Resources.PositionAttribute);
                var xRename = xDirective.Attribute(Resources.RenameAttribute);
                var xAction = xDirective.Attribute(Resources.ActionAttribute);

                var name = String.Empty;
                var rename = String.Empty;
                var action = "none";
                var to = -1;
                var position = -1;

                if (xName != null) name = xName.Value;
                if (xTo != null) to = Int32.Parse(xTo.Value);
                if (xPosition != null) position = Int32.Parse(xPosition.Value);
                if (xRename != null) rename = xRename.Value;
                if (xAction != null) action = xAction.Value.ToLowerInvariant();

                directives.Add(new Directive
                {
                    Name = name,
                    Action = action,
                    Position = position,
                    To = to,
                    Rename = rename
                });
            }

            return directives;
        }

        private static string GetSeparator(XContainer root)
        {
            var xSeparator = root.Element(Resources.SeparatorElement);
            return xSeparator == null ? ";" : xSeparator.Value;
        }
    }
}