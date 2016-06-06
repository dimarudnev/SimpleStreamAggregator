using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SimpleAggregator {
    class CalculatorOptions {
        public string ResultFileName { get; set; }
        public string SourcePath { get; set; }
        public int TimeFrame { get; set; }
        public int FrameCount { get; set; }
        public string DestinationPath { get; internal set; }
        public int BasisCount { get; set; }
        public string[] Basis { get; set; }
    }
}
