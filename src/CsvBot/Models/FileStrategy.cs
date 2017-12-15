#region Copyright (c) 2017, Roberto Mauro

// Copyright (c) 2017, Roberto Mauro
// This source code is licensed under the MIT license found in the
// LICENSE file in the root directory of this source tree.

#endregion

namespace CsvBot.Models
{
    internal class FileStrategy
    {
        public string Type { get; set; }
        public string Path { get; set; }
        public string Overwrite { get; set; }
        public string With { get; set; }
    }
}