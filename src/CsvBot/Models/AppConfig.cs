#region Copyright (c) 2017, Roberto Mauro

// Copyright (c) 2017, Roberto Mauro
// This source code is licensed under the MIT license found in the
// LICENSE file in the root directory of this source tree.

#endregion

using System.Collections.Generic;

namespace CsvBot.Models
{
    internal class AppConfig
    {
        public string CsvSeparator { get; set; }
        public List<FileStrategy> FileStrategies { get; set; }
        public string SourcePath { get; set; }
        public List<Directive> Directives { get; set; }
        public string GroupedWith { get; set; }
    }
}