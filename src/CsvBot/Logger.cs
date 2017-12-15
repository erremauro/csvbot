#region Copyright (c) 2017, Roberto Mauro

// Copyright (c) 2017, Roberto Mauro
// This source code is licensed under the MIT license found in the
// LICENSE file in the root directory of this source tree.

#endregion

using System;
using System.IO;

namespace CsvBot
{
    internal static class Logger
    {
        public static string m_Logfile;

        public static string LogFile
        {
            get
            {
                // return an initialized instance of logfile path
                if (m_Logfile != null) return m_Logfile;

                // create the log file path from current executing assemblu path
                m_Logfile = Path.Combine(Utils.ExecutingAssemblyPath(), "log.txt");
                if (!File.Exists(m_Logfile)) return m_Logfile;

                // check current log file size. If over 1MB, tries to roll the file
                var fileInfo = new FileInfo(m_Logfile);
                if (fileInfo.Length < 1000000) return m_Logfile;

                try
                {
                    File.Move(m_Logfile,
                        Path.Combine(Utils.ExecutingAssemblyPath(),
                            String.Format(@"log-{0:yyyy-MM-dd-HH-mm-ss}.txt", DateTime.Now)));
                }
                catch (IOException)
                {
                }

                return m_Logfile;
            }
        }

        public static void Info(string message, params object[] args)
        {
            using (var writer = File.AppendText(LogFile))
            {
                LogMessage(String.Format(message, args), "info", writer);
            }
        }

        public static void Success(string message, params object[] args)
        {
            using (var writer = File.AppendText(LogFile))
            {
                LogMessage(String.Format(message, args), "success", writer);
            }
        }

        public static void Warn(string message, params object[] args)
        {
            using (var writer = File.AppendText(LogFile))
            {
                LogMessage(String.Format(message, args), "warn", writer);
            }
        }

        public static void Error(string message, params object[] args)
        {
            using (var writer = File.AppendText(LogFile))
            {
                LogMessage(String.Format(message, args), "error", writer);
            }
        }

        private static void LogMessage(string message, string level, TextWriter writer)
        {
            writer.WriteLine("[{0:yyyy-mm-dd HH:mm:ss}] {1}\t{2}", DateTime.Now, level.ToUpperInvariant(), message);
        }
    }
}