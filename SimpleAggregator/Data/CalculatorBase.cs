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
    }
    class CalculatorBase {
        protected CalculatorOptions options;
        protected BackgroundWorker worker;
        int lineCount = 0;


        public bool Cancelled { get { return worker.CancellationPending; } }
        public object Result { get { return lineCount; } }

        public CalculatorBase(BackgroundWorker worker, CalculatorOptions options) {
            this.worker = worker;
            this.options = options;
        }
        void ReportProgress(Stream stream) {
            worker.ReportProgress((int)((100 * stream.Position) / stream.Length));
        }

        public virtual void WriteResult(StreamWriter writer) {
        }

        public virtual void Begin() {

        }
        public virtual void End(int timeIndex) {

        }


        public virtual void AddValue(string rowName, string[] colNames) {
        }
    }
}
