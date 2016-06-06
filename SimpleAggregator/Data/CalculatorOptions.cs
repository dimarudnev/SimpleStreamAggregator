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
        public string Comment { get; internal set; }



        public void WriteExpirementInfo(IEnumerable<string> sourceFiles, IEnumerable<string> basis) {
            var infoFile = DestinationPath + ResultFileName + ".info.txt";
            using(FileStream fs = new FileStream(infoFile, FileMode.Create)) {
                using(StreamWriter sw = new StreamWriter(fs)) {
                    sw.WriteLine("Source Files: {0}", String.Join(",", sourceFiles));
                    sw.WriteLine("Time frame lenght: {0}", TimeFrame);
                    sw.WriteLine("Time frame count: {0}", FrameCount);
                    sw.WriteLine("Basis count: {0}", BasisCount);
                    sw.WriteLine("Basis: {0}", String.Join(",", basis));

                    sw.WriteLine("Comment: {0}", Comment);
                }
            }
        }
    }
}
